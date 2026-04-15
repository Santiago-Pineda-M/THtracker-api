namespace THtracker.Application.UseCases.Sessions;

using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

public class LogoutCurrentSessionUseCase
{
    private readonly IUserSessionRepository sessionRepository;
    private readonly IRefreshTokenRepository refreshTokenRepository;
    private readonly IUnitOfWork unitOfWork;

    public LogoutCurrentSessionUseCase(
        IUserSessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork
    )
    {
        this.sessionRepository = sessionRepository;
        this.refreshTokenRepository = refreshTokenRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await this.sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            return Result.Failure(new Error("NotFound", "La sesión no existe."));
        }

        if (session.UserId != userId)
        {
            return Result.Failure(new Error("Forbidden", "No tienes acceso a esta sesión."));
        }

        session.Revoke();
        await this.sessionRepository.UpdateAsync(session, cancellationToken);

        var refreshToken = await this.refreshTokenRepository.GetByTokenAsync(session.SessionToken, cancellationToken);
        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.Revoke("unknown", "User logout");
            await this.refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
        }

        await this.unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
