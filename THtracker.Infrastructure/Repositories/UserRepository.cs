using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.Include(u => u.Roles).ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context
            .Users.Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Users.Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        return true;
    }
}
