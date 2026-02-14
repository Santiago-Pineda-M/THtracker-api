using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(
            t => t.Token == token,
            cancellationToken
        );
    }

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Add(refreshToken);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        _context.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        string ipAddress,
        string reason,
        CancellationToken cancellationToken = default
    )
    {
        var tokens = await _context
            .RefreshTokens.Where(t =>
                t.UserId == userId && t.RevokedDate == null && t.ExpiryDate > DateTime.UtcNow
            )
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke(ipAddress, reason);
        }
    }
}
