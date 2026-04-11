using FluentValidation;
using THtracker.Application.DTOs.Auth;
using THtracker.Application.Interfaces;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Auth;

public class LoginUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<LoginRequest> _validator;

    public LoginUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        IValidator<LoginRequest> validator
    )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<TokenResponse>> ExecuteAsync(
        LoginRequest request,
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

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || user.PasswordHash == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<TokenResponse>(new Error("Unauthorized", "Invalid Credentials"));
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
