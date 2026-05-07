using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Application.Interfaces;

namespace THtracker.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClientIpProvider _clientIpProvider;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        IClientIpProvider clientIpProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
        _clientIpProvider = clientIpProvider;
    }

    public async Task<Result<TokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        if (user == null || user.PasswordHash == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<TokenResponse>(new Error("Unauthorized", "Credenciales inválidas."));
        }

        var clientIp = _clientIpProvider.GetClientIpAddress();

        var refreshTokenEntity = _jwtProvider.GenerateRefreshToken(
            user,
            clientIp,
            request.DeviceInfo
        );
        
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        var userSession = new UserSession(
            user.Id,
            refreshTokenEntity.Token,
            refreshTokenEntity.ExpiryDate,
            request.DeviceInfo,
            clientIp
        );
        
        await _sessionRepository.AddAsync(userSession, cancellationToken);

        string accessToken = _jwtProvider.GenerateAccessToken(user, userSession.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            accessToken,
            refreshTokenEntity.Token,
            refreshTokenEntity.ExpiryDate
        );
    }
}
