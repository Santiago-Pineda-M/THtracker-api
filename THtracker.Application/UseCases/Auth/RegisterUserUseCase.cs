using FluentValidation;
using THtracker.Application.DTOs.Auth;
using THtracker.Application.Interfaces;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Auth;

public class RegisterUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RegisterUserRequest> _validator;

    public RegisterUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IValidator<RegisterUserRequest> validator
    )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<Guid>> ExecuteAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<Guid>(new Error("Validation", errors));
        }

        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Result.Failure<Guid>(new Error("Conflict", "User with this email already exists."));
        }

        var user = new User(request.Name, request.Email);

        string passwordHash = _passwordHasher.Hash(request.Password);
        user.SetPassword(passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
