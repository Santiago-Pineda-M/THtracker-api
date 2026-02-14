using Microsoft.EntityFrameworkCore;
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

    public async Task<IEnumerable<ActivityLog>> GetAllByActivityAsync(
        Guid activityId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ActivityLogs.Where(l => l.ActivityId == activityId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ActivityLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ActivityLogs.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        await _context.ActivityLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        _context.ActivityLogs.Update(log);
        await _context.SaveChangesAsync(cancellationToken);
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

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var log = await _context.ActivityLogs.FindAsync(new object[] { id }, cancellationToken);
        if (log == null)
            return false;
        _context.ActivityLogs.Remove(log);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
