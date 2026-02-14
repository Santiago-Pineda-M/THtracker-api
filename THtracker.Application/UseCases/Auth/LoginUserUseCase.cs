using THtracker.Application.DTOs.Auth;
using THtracker.Application.Interfaces;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Auth;

public class LoginUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork
    )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TokenResponse> ExecuteAsync(
        LoginRequest request,
        string ipAddress,
        CancellationToken cancellationToken = default
    )
    {
        // Validación manual
        var validator = new Validators.Auth.LoginRequestValidator();
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Datos inválidos: {errors}");
        }
        // 1. Find User by Email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            throw new Exception("Invalid Credentials");
        }

        // 2. Verify Password
        if (
            user.PasswordHash == null
            || !_passwordHasher.Verify(request.Password, user.PasswordHash)
        )
        {
            throw new Exception("Invalid Credentials");
        }

        // 3. Generate Access Token service
        string accessToken = _jwtProvider.GenerateAccessToken(user);

        // 4. Create Refresh Token
        var refreshTokenEntity = _jwtProvider.GenerateRefreshToken(
            user,
            ipAddress,
            request.DeviceInfo
        );

        // 5. Save Refresh Token
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            accessToken,
            refreshTokenEntity.Token,
            refreshTokenEntity.ExpiryDate
        );
    }
}
