using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class DeleteUserUseCase
{
    private readonly IUserRepository _repository;

    public DeleteUserUseCase(IUserRepository repository)
    {
        _repository = repository;
    }

    public virtual async Task<bool> ExecuteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }
}
