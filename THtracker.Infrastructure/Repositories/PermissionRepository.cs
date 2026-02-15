using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _context;

    public PermissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Permissions.ToListAsync(cancellationToken);
    }

    public async Task<Permission?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Permissions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        await _context.Permissions.AddAsync(permission, cancellationToken);
    }

    public async Task UpdateAsync(
        Permission permission,
        CancellationToken cancellationToken = default
    )
    {
        _context.Permissions.Update(permission);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await GetByIdAsync(id, cancellationToken);
        if (permission == null)
            return false;
        _context.Permissions.Remove(permission);
        return true;
    }
}
