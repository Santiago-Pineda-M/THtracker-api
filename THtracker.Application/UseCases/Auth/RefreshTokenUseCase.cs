using THtracker.Application.DTOs.Auth;
using THtracker.Application.Interfaces;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Auth;

public class RefreshTokenUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenUseCase(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork
    )
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<TokenResponse> ExecuteAsync(
        string refreshToken,
        string ipAddress,
        string deviceInfo,
        CancellationToken cancellationToken = default
    )
    {
        // Validación manual
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new Exception("El refresh token es obligatorio.");
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new Exception("La dirección IP es obligatoria.");
        if (string.IsNullOrWhiteSpace(deviceInfo))
            throw new Exception("La información del dispositivo es obligatoria.");
        // 1. Get refresh token from store
        var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(
            refreshToken,
            cancellationToken
        );

        if (tokenEntity == null || !tokenEntity.IsActive)
        {
            throw new Exception("Invalid or expired refresh token");
        }

        // 2. Get User
        var user = await _userRepository.GetByIdAsync(tokenEntity.UserId, cancellationToken);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        // 3. Revoke old token
        tokenEntity.Revoke(ipAddress, "Token refreshed");
        await _refreshTokenRepository.UpdateAsync(tokenEntity, cancellationToken);

        // 4. Generate new tokens
        string newAccessToken = _jwtProvider.GenerateAccessToken(user);
        var newRefreshTokenEntity = _jwtProvider.GenerateRefreshToken(user, ipAddress, deviceInfo);

        // 5. Save new refresh token
        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            newAccessToken,
            newRefreshTokenEntity.Token,
            newRefreshTokenEntity.ExpiryDate
        );
    }
}
