using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class DeleteUserUseCase
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserUseCase(IUserRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public virtual async Task<Result> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure(new Error("Validation", "El id de usuario es obligatorio."));
        }

        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return Result.Failure(new Error("NotFound", "User not found."));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
