using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly AppDbContext _context;

    public ActivityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<Activity>> GetPageByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Activities.AsNoTracking().Where(a => a.UserId == userId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedList<Activity>(items, total);
    }

    public async Task<IEnumerable<Activity>> GetAllByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Activities.Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Activity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Activities.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        await _context.Activities.AddAsync(activity, cancellationToken);
    }

    public async Task UpdateAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        _context.Activities.Update(activity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await _context.Activities.FindAsync(new object[] { id }, cancellationToken);
        if (activity == null)
            return false;
        _context.Activities.Remove(activity);
        return true;
    }
}
