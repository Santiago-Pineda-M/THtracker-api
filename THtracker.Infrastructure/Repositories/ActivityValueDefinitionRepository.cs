using Microsoft.EntityFrameworkCore;
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
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        ActivityValueDefinition valueDefinition,
        CancellationToken cancellationToken = default
    )
    {
        _context.ActivityValueDefinitions.Update(valueDefinition);
        await _context.SaveChangesAsync(cancellationToken);
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
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
