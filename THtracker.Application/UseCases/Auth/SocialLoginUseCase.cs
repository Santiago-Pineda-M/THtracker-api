using FluentValidation;
using THtracker.Application.DTOs.Auth;
using THtracker.Application.Interfaces;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Auth;

public class SocialLoginUseCase
{
    private readonly ISocialAuthenticator _socialAuthenticator;
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<SocialLoginRequest> _validator;

    public SocialLoginUseCase(
        ISocialAuthenticator socialAuthenticator,
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        IValidator<SocialLoginRequest> validator
    )
    {
        _socialAuthenticator = socialAuthenticator;
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<TokenResponse>> ExecuteAsync(
        SocialLoginRequest request,
        string ipAddress,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<TokenResponse>(new Error("Validation", errors));
        }

        var profile = await _socialAuthenticator.AuthenticateAsync(request.Provider, request.Token);
        if (profile == null)
        {
            return Result.Failure<TokenResponse>(new Error("Unauthorized", "Social authentication failed."));
        }

        var user = await _userRepository.GetByEmailAsync(profile.Email, cancellationToken);

        if (user == null)
        {
            user = new User(profile.Name, profile.Email);
            user.AddLogin(profile.Provider, profile.ProviderKey, profile.Name);
            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            if (!user.Logins.Any(l => l.LoginProvider == profile.Provider))
            {
                user.AddLogin(profile.Provider, profile.ProviderKey, profile.Name);
                await _userRepository.UpdateAsync(user, cancellationToken);
            }
        }

        string accessToken = _jwtProvider.GenerateAccessToken(user);
        var refreshTokenEntity = _jwtProvider.GenerateRefreshToken(
            user,
            ipAddress,
            request.DeviceInfo
        );

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        var userSession = new UserSession(
            user.Id,
            refreshTokenEntity.Token,
            refreshTokenEntity.ExpiryDate,
            request.DeviceInfo,
            ipAddress
        );
        await _sessionRepository.AddAsync(userSession, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            accessToken,
            refreshTokenEntity.Token,
            refreshTokenEntity.ExpiryDate
        );
    }
}
