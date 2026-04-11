using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Sessions;

public class RevokeSessionUseCase
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeSessionUseCase(
        IUserSessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork
    )
    {
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken = default
    )
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
            return Result.Failure(new Error("NotFound", "La sesión no existe."));

        if (session.UserId != userId)
            return Result.Failure(new Error("Forbidden", "No tienes acceso a esta sesión."));

        session.Revoke();
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        // Revocar el Refresh Token asociado
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(session.SessionToken, cancellationToken);
        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.Revoke("unknown", "Session revoked by user");
            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
