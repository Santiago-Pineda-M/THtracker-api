using THtracker.Domain.DTOs;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class UpdateUserUseCase
{
    private readonly IUserRepository _repository;

    public UpdateUserUseCase(IUserRepository repository)
    {
        _repository = repository;
    }

    public virtual async Task<UserDto?> ExecuteAsync(Guid id, UpdateUserDto dto)
    {
        return await _repository.UpdateAsync(id, dto);
    }
}
