using THtracker.Application.DTOs.Users;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class GetUserByIdUseCase
{
    private readonly IUserRepository _repository;

    public GetUserByIdUseCase(IUserRepository repository)
    {
        _repository = repository;
    }

    public virtual async Task<Result<UserDto>> ExecuteAsync(Guid id)
    {
        if (id == Guid.Empty)
            return Result.Failure<UserDto>(new Error("Validation", "El id de usuario es obligatorio."));

        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            return Result.Failure<UserDto>(new Error("NotFound", "El usuario no existe."));

        return new UserDto(user.Id, user.Name, user.Email);
    }
}
