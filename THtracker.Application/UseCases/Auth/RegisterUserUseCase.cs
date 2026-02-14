using THtracker.Application.DTOs.Auth;
using THtracker.Application.Validators.Auth;
using THtracker.Application.Interfaces;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Auth;

public class RegisterUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork
    )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecuteAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // Validación manual
        var validator = new RegisterUserRequestValidator();
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Datos inválidos: {errors}");
        }
        // 1. Validate uniqueness (Domain Rule)
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new Exception("User with this email already exists."); // Should use a custom DomainException
        }

        // 2. Create Entity
        var user = new User(request.Name, request.Email);

        // 3. Apply Business Logic (Hashing)
        string passwordHash = _passwordHasher.Hash(request.Password);
        user.SetPassword(passwordHash);

        // 4. Persist
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
