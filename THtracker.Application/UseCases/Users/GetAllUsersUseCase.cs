using THtracker.Domain.DTOs;
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
        return await _repository.GetAllAsync();
    }
}
