using THtracker.Application.DTOs.Users;
using THtracker.Application.Validators.Users;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class CreateUserUseCase
{
    private readonly IUserRepository _repository;

    public CreateUserUseCase(IUserRepository repository)
    {
        _repository = repository;
    }

    public virtual async Task<UserDto> ExecuteAsync(CreateUserRequest request)
    {
        // Validación manual
        var validator = new CreateUserRequestValidator();
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Datos inválidos: {errors}");
        }

        // 1. Validate uniqueness
        if (await _repository.ExistsByEmailAsync(request.Email))
        {
            throw new Exception("User with this email already exists.");
        }

        // Crear entidad User
        var user = new User(request.Name, request.Email);

        // Guardar el usuario
        await _repository.AddAsync(user);

        return new UserDto(user.Id, user.Name, user.Email);
    }
}
