using THtracker.Application.DTOs.Auth;
using THtracker.Application.Interfaces;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Auth;

public class SocialLoginUseCase
{
    private readonly ISocialAuthenticator _socialAuthenticator;
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SocialLoginUseCase(
        ISocialAuthenticator socialAuthenticator,
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork
    )
    {
        _socialAuthenticator = socialAuthenticator;
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TokenResponse> ExecuteAsync(
        SocialLoginRequest request,
        string ipAddress,
        CancellationToken cancellationToken = default
    )
    {
        // Validación manual
        var validator = new Validators.Auth.SocialLoginRequestValidator();
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Datos inválidos: {errors}");
        }
        // 1. Authenticate with social provider
        var profile = await _socialAuthenticator.AuthenticateAsync(request.Provider, request.Token);

        // 2. Find or Create User
        var user = await _userRepository.GetByEmailAsync(profile.Email, cancellationToken);

        if (user == null)
        {
            // Register new user automatically
            user = new User(profile.Name, profile.Email);
            user.AddLogin(profile.Provider, profile.ProviderKey, profile.Name);
            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            // Check if login provider is already linked
            if (!user.Logins.Any(l => l.LoginProvider == profile.Provider))
            {
                user.AddLogin(profile.Provider, profile.ProviderKey, profile.Name);
                await _userRepository.UpdateAsync(user, cancellationToken);
            }
        }

        // 3. Generate Tokens
        string accessToken = _jwtProvider.GenerateAccessToken(user);
        var refreshTokenEntity = _jwtProvider.GenerateRefreshToken(
            user,
            ipAddress,
            request.DeviceInfo
        );

        // 4. Save
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            accessToken,
            refreshTokenEntity.Token,
            refreshTokenEntity.ExpiryDate
        );
    }
}
