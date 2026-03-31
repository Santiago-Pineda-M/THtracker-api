using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly AppDbContext _context;

    public UserSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserSession>> GetAllByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .UserSessions.Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetActiveByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .UserSessions.Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.UserSessions.FirstOrDefaultAsync(
            s => s.Id == id,
            cancellationToken
        );
    }

    public async Task<UserSession?> GetByTokenAsync(
        string sessionToken,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.UserSessions.FirstOrDefaultAsync(
            s => s.SessionToken == sessionToken,
            cancellationToken
        );
    }

    public async Task AddAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        await _context.UserSessions.AddAsync(session, cancellationToken);
    }

    public async Task UpdateAsync(
        UserSession session,
        CancellationToken cancellationToken = default
    )
    {
        _context.UserSessions.Update(session);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var session = await _context.UserSessions.FindAsync(new object[] { id }, cancellationToken);
        if (session == null)
            return false;
        _context.UserSessions.Remove(session);
        return true;
    }
}
