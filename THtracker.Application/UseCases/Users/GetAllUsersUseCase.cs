using THtracker.Application.DTOs.Users;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class GetAllUsersUseCase
{
    private readonly IUserRepository _repository;

    public GetAllUsersUseCase(IUserRepository repository)
    {
        _repository = repository;
    }

    public virtual async Task<IEnumerable<UserDto>> ExecuteAsync()
    {
        // Validación manual (no hay parámetros, pero se puede validar contexto si es necesario)
        var users = await _repository.GetAllAsync();
        return users.Select(u => new UserDto(u.Id, u.Name, u.Email));
    }
}
