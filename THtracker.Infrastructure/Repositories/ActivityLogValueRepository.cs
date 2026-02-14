using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class ActivityLogValueRepository : IActivityLogValueRepository
{
    private readonly AppDbContext _context;

    public ActivityLogValueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ActivityLogValue>> GetAllByLogAsync(
        Guid activityLogId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ActivityLogValues.Where(v => v.ActivityLogId == activityLogId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ActivityLogValue?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ActivityLogValues.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(
        ActivityLogValue logValue,
        CancellationToken cancellationToken = default
    )
    {
        await _context.ActivityLogValues.AddAsync(logValue, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var logValue = await _context.ActivityLogValues.FindAsync(
            new object[] { id },
            cancellationToken
        );
        if (logValue == null)
            return false;
        _context.ActivityLogValues.Remove(logValue);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
