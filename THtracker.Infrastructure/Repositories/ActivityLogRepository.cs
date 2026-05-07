using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly AppDbContext _context;

    public ActivityLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<ActivityLog>> GetLogsPageForUserAsync(
        Guid userId,
        Guid? activityId,
        DateTime? from,
        DateTime? to,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filtered = BuildLogsFilterQuery(userId, activityId, from, to);
        var total = await filtered.CountAsync(cancellationToken);
        var items = await filtered
            .OrderByDescending(log => log.StartedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<ActivityLog>(items, total);
    }

    public async Task<PagedList<ActivityLog>> GetActiveLogsPageForUserAsync(
        Guid userId,
        Guid? activityId,
        DateTime? from,
        DateTime? to,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filtered = BuildLogsFilterQuery(userId, activityId, from, to)
            .Where(log => log.EndedAt == null);

        var total = await filtered.CountAsync(cancellationToken);
        var items = await filtered
            .OrderByDescending(log => log.StartedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<ActivityLog>(items, total);
    }

    private IQueryable<ActivityLog> BuildLogsFilterQuery(
        Guid userId,
        Guid? activityId,
        DateTime? from,
        DateTime? to)
    {
        var query = _context.ActivityLogs
            .AsNoTracking()
            .Where(log => _context.Activities
                .AsNoTracking()
                .Any(activity => activity.Id == log.ActivityId && activity.UserId == userId));

        if (activityId.HasValue)
            query = query.Where(log => log.ActivityId == activityId.Value);
        if (from.HasValue)
            query = query.Where(log => log.StartedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(log => log.StartedAt <= to.Value);

        return query;
    }

    public async Task<IEnumerable<ActivityLog>> GetAllByActivityAsync(
        Guid activityId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ActivityLogs
            .Include(l => l.LogValues)
            .Where(l => l.ActivityId == activityId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ActivityLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ActivityLogs
            .Include(l => l.LogValues)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        await _context.ActivityLogs.AddAsync(log, cancellationToken);
    }

    public Task UpdateAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        _context.ActivityLogs.Update(log);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<ActivityLog>> GetActiveLogsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ActivityLogs
            .Join(_context.Activities,
                log => log.ActivityId,
                activity => activity.Id,
                (log, activity) => new { Log = log, Activity = activity })
            .Where(x => x.Activity.UserId == userId && x.Log.EndedAt == null)
            .Select(x => x.Log)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActivityLog>> GetOverlappingLogsAsync(Guid userId, DateTime start, DateTime end, Guid? excludeLogId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ActivityLogs
            .Join(_context.Activities,
                log => log.ActivityId,
                activity => activity.Id,
                (log, activity) => new { Log = log, Activity = activity })
            .Where(x => x.Activity.UserId == userId);

        if (excludeLogId.HasValue)
        {
            query = query.Where(x => x.Log.Id != excludeLogId.Value);
        }

        // Logic: L.StartedAt < End && (L.EndedAt == null || L.EndedAt > Start)
        return await query
            .Where(x => x.Log.StartedAt < end && (x.Log.EndedAt == null || x.Log.EndedAt > start))
            .Select(x => x.Log)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActivityLog>> GetLogsAsync(
        Guid userId,
        Guid? activityId = null,
        DateTime? start = null,
        DateTime? end = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context.ActivityLogs
            .Include(l => l.LogValues)
                .ThenInclude(v => v.ValueDefinition)
            .Join(_context.Activities,
                log => log.ActivityId,
                activity => activity.Id,
                (log, activity) => new { Log = log, Activity = activity })
            .Where(x => x.Activity.UserId == userId);

        if (activityId.HasValue)
        {
            query = query.Where(x => x.Log.ActivityId == activityId.Value);
        }

        if (start.HasValue)
        {
            query = query.Where(x => x.Log.EndedAt == null || x.Log.EndedAt >= start.Value);
        }

        if (end.HasValue)
        {
            query = query.Where(x => x.Log.StartedAt <= end.Value);
        }

        return await query
            .OrderByDescending(x => x.Log.StartedAt)
            .Select(x => x.Log)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActivityLog>> GetReportLogsAsync(
        Guid userId,
        DateTime start,
        DateTime end,
        List<Guid>? categoryIds = null,
        List<Guid>? activityIds = null,
        string? searchTerm = null,
        bool? onlyCompleted = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context.ActivityLogs
            .Include(l => l.LogValues)
                .ThenInclude(v => v.ValueDefinition)
            .Join(_context.Activities,
                log => log.ActivityId,
                activity => activity.Id,
                (log, activity) => new { Log = log, Activity = activity })
            .Where(x => x.Activity.UserId == userId);

        // Filtro de rango de fechas (solapamiento)
        query = query.Where(x => x.Log.StartedAt < end && (x.Log.EndedAt == null || x.Log.EndedAt > start));

        // Filtro de categorías (múltiples)
        if (categoryIds != null && categoryIds.Any())
        {
            query = query.Where(x => categoryIds.Contains(x.Activity.CategoryId));
        }

        // Filtro de actividades (múltiples)
        if (activityIds != null && activityIds.Any())
        {
            query = query.Where(x => activityIds.Contains(x.Log.ActivityId));
        }

        // Búsqueda por término
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => x.Activity.Name.Contains(searchTerm));
        }

        // Filtro de logs completados
        if (onlyCompleted.HasValue && onlyCompleted.Value)
        {
            query = query.Where(x => x.Log.EndedAt != null);
        }

        return await query
            .OrderByDescending(x => x.Log.StartedAt)
            .Select(x => x.Log)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var log = await _context.ActivityLogs.FindAsync(new object[] { id }, cancellationToken);
        if (log == null)
            return false;
        _context.ActivityLogs.Remove(log);
        return true;
    }
}
