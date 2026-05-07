using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class ActivityValueDefinitionRepository : IActivityValueDefinitionRepository
{
    private readonly AppDbContext _context;

    public ActivityValueDefinitionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<ActivityValueDefinition>> GetPageByActivityAsync(
        Guid activityId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ActivityValueDefinitions.AsNoTracking().Where(v => v.ActivityId == activityId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(v => v.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedList<ActivityValueDefinition>(items, total);
    }

    public async Task<IEnumerable<ActivityValueDefinition>> GetAllByActivityAsync(
        Guid activityId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ActivityValueDefinitions.Where(v => v.ActivityId == activityId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ActivityValueDefinition?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ActivityValueDefinitions.FindAsync(
            new object[] { id },
            cancellationToken
        );
    }

    public async Task AddAsync(
        ActivityValueDefinition valueDefinition,
        CancellationToken cancellationToken = default
    )
    {
        await _context.ActivityValueDefinitions.AddAsync(valueDefinition, cancellationToken);
    }

    public async Task UpdateAsync(
        ActivityValueDefinition valueDefinition,
        CancellationToken cancellationToken = default
    )
    {
        _context.ActivityValueDefinitions.Update(valueDefinition);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var valueDefinition = await _context.ActivityValueDefinitions.FindAsync(
            new object[] { id },
            cancellationToken
        );
        if (valueDefinition == null)
            return false;
        _context.ActivityValueDefinitions.Remove(valueDefinition);
        return true;
    }
}
