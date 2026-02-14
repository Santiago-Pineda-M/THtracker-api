using THtracker.Application.DTOs.Users;
using THtracker.Application.Validators.Users;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class UpdateUserUseCase
{
    private readonly IUserRepository _repository;

    public UpdateUserUseCase(IUserRepository repository)
    {
        _repository = repository;
    }

    public virtual async Task<UserDto?> ExecuteAsync(Guid id, UpdateUserRequest request)
    {
        // Validación manual
        var validator = new UpdateUserRequestValidator();
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Datos inválidos: {errors}");
        }

        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            return null;

        // Check for email uniqueness if changing email
        if (user.Email != request.Email && await _repository.ExistsByEmailAsync(request.Email))
        {
            throw new Exception("User with this email already exists.");
        }

        user.Update(request.Name, request.Email);
        await _repository.UpdateAsync(user);
        return new UserDto(user.Id, user.Name, user.Email);
    }
}
