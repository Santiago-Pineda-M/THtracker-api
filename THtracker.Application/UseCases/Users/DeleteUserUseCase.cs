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
        // Validación manual
        if (id == Guid.Empty)
            throw new Exception("El id de usuario es obligatorio.");
        return await _repository.DeleteAsync(id);
    }
}
