using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;
using THtracker.Application.Interfaces;

namespace THtracker.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository sessionRepository,
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Obtener y validar el Refresh Token actual
        var oldRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        
        if (oldRefreshToken == null || !oldRefreshToken.IsActive)
        {
            return Result.Failure<TokenResponse>(new Error("Unauthorized", "Refresh Token inválido o expirado."));
        }

        // 2. Obtener la sesión asociada al token
        var session = await _sessionRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        
        if (session == null || !session.IsActive)
        {
            return Result.Failure<TokenResponse>(new Error("Unauthorized", "Sesión de usuario no encontrada o inactiva."));
        }

        // 3. Obtener el usuario
        var user = await _userRepository.GetByIdAsync(oldRefreshToken.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<TokenResponse>(new Error("NotFound", "Usuario no encontrado."));
        }

        // 4. Rotar el Refresh Token (Seguridad: un solo uso)
        oldRefreshToken.Revoke(request.IpAddress, "Reemplazado por nuevo token");
        await _refreshTokenRepository.UpdateAsync(oldRefreshToken, cancellationToken);

        var newRefreshTokenEntity = _jwtProvider.GenerateRefreshToken(
            user,
            request.IpAddress,
            request.DeviceInfo
        );
        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);

        // 5. ACTUALIZAR la sesión existente (No crear una nueva)
        session.Refresh(
            newRefreshTokenEntity.Token,
            newRefreshTokenEntity.ExpiryDate,
            request.IpAddress,
            request.DeviceInfo
        );
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        // 6. Generar nuevo Access Token vinculado a la sesión original
        string accessToken = _jwtProvider.GenerateAccessToken(user, session.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            accessToken,
            newRefreshTokenEntity.Token,
            newRefreshTokenEntity.ExpiryDate
        );
    }
}
