using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(
        Guid userId,
        string ipAddress,
        string reason,
        CancellationToken cancellationToken = default
    );
}
