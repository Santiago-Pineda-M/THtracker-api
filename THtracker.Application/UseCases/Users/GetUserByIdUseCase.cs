using THtracker.Domain.DTOs;
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
        return await _repository.GetByIdAsync(id);
    }
}
