using THtracker.Domain.DTOs;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class CreateUserUseCase
{
    private readonly IUserRepository _repository;

    public CreateUserUseCase(IUserRepository repository)
    {
        _repository = repository;
    }

    public virtual async Task<UserDto> ExecuteAsync(CreateUserDto dto)
    {
        // Business validation would go here
        // For example: validate email format, check for duplicates, etc.
        
        return await _repository.CreateAsync(dto);
    }
}
