using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.UserSessions.Commands.RevokeSession;

public sealed class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand, Result>
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeSessionCommandHandler(
        IUserSessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        
        if (session == null || session.UserId != request.UserId)
        {
            return Result.Failure(new Error("NotFound", "La sesión no existe o no tienes acceso."));
        }

        session.Revoke();
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        // Revocar el Refresh Token asociado si existe y es activo
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
