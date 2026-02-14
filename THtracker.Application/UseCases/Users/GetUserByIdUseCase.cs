using THtracker.Application.DTOs.Users;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class GetUserByIdUseCase
{
    private readonly IUserRepository _repository;

    public GetUserByIdUseCase(IUserRepository repository)
    {
        _repository = repository;
    }

    public virtual async Task<UserDto?> ExecuteAsync(Guid id)
    {
        // Validación manual
        if (id == Guid.Empty)
            throw new Exception("El id de usuario es obligatorio.");
        var user = await _repository.GetByIdAsync(id);
        return user is null ? null : new UserDto(user.Id, user.Name, user.Email);
    }
}
