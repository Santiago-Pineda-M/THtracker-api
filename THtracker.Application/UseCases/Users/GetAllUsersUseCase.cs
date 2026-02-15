using THtracker.Application.DTOs.Users;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class GetAllUsersUseCase
{
    private readonly IUserRepository _repository;

    public GetAllUsersUseCase(IUserRepository repository)
    {
        _repository = repository;
    }

    public virtual async Task<Result<IEnumerable<UserDto>>> ExecuteAsync()
    {
        var users = await _repository.GetAllAsync();
        return Result.Success(users.Select(u => new UserDto(u.Id, u.Name, u.Email)));
    }
}
