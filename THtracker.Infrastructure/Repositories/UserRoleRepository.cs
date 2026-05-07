using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly AppDbContext _context;

    public UserRoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<Role>> GetRolesPageByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserRoles.AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Join(_context.Roles.AsNoTracking(), ur => ur.RoleId, r => r.Id, (_, r) => r);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(r => r.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedList<Role>(items, total);
    }

    public async Task<IEnumerable<Role>> GetRolesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .UserRoles.Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role!)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .UserRoles.Where(ur => ur.RoleId == roleId)
            .Include(ur => ur.User)
            .Select(ur => ur.User!)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRoleToUserAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        var userRole = new UserRole(userId, roleId);
        await _context.UserRoles.AddAsync(userRole, cancellationToken);
    }

    public async Task RemoveRoleFromUserAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        var userRole = await _context.UserRoles.FirstOrDefaultAsync(
            ur => ur.UserId == userId && ur.RoleId == roleId,
            cancellationToken
        );
        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
        }
    }

    public async Task<bool> IsRoleAssignedAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.UserRoles.AnyAsync(
            ur => ur.UserId == userId && ur.RoleId == roleId,
            cancellationToken
        );
    }
}
