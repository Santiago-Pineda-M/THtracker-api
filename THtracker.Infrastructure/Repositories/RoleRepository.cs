using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<Role>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Roles.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(r => r.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedList<Role>(items, total);
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles.ToListAsync(cancellationToken);
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(role, cancellationToken);
    }

    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Roles.Update(role);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await GetByIdAsync(id, cancellationToken);
        if (role == null)
            return false;
        _context.Roles.Remove(role);
        return true;
    }
}
