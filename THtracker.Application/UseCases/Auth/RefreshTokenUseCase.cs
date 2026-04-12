using THtracker.Application.DTOs.Auth;
using THtracker.Application.Interfaces;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Auth;

public class RefreshTokenUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenUseCase(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository sessionRepository,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork
    )
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _sessionRepository = sessionRepository;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TokenResponse>> ExecuteAsync(
        string refreshToken,
        string ipAddress,
        string deviceInfo,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Failure<TokenResponse>(new Error("Validation", "El refresh token es obligatorio."));

        var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(
            refreshToken,
            cancellationToken
        );

        if (tokenEntity == null || !tokenEntity.IsActive)
        {
            return Result.Failure<TokenResponse>(new Error("Unauthorized", "Invalid or expired refresh token"));
        }

        var user = await _userRepository.GetByIdAsync(tokenEntity.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<TokenResponse>(new Error("NotFound", "User not found"));
        }

        tokenEntity.Revoke(ipAddress, "Token refreshed");
        await _refreshTokenRepository.UpdateAsync(tokenEntity, cancellationToken);

        var newRefreshTokenEntity = _jwtProvider.GenerateRefreshToken(user, ipAddress, deviceInfo);
        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);

        Guid sessionId;
        var currentSession = await _sessionRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (currentSession != null)
        {
            currentSession.Refresh(newRefreshTokenEntity.Token, newRefreshTokenEntity.ExpiryDate, ipAddress, deviceInfo);
            await _sessionRepository.UpdateAsync(currentSession, cancellationToken);
            sessionId = currentSession.Id;
        }
        else
        {
            var newSession = new UserSession(
                user.Id,
                newRefreshTokenEntity.Token,
                newRefreshTokenEntity.ExpiryDate,
                deviceInfo,
                ipAddress
            );
            await _sessionRepository.AddAsync(newSession, cancellationToken);
            sessionId = newSession.Id;
        }

        string newAccessToken = _jwtProvider.GenerateAccessToken(user, sessionId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            newAccessToken,
            newRefreshTokenEntity.Token,
            newRefreshTokenEntity.ExpiryDate
        );
    }
}
