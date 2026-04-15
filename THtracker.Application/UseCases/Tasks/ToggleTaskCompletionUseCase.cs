namespace THtracker.Application.UseCases.Tasks;

using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

public class ToggleTaskCompletionUseCase
{
    private readonly ITaskRepository taskRepository;
    private readonly IUnitOfWork unitOfWork;

    public ToggleTaskCompletionUseCase(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        this.taskRepository = taskRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> ExecuteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(id, cancellationToken);

        if (task == null)
        {
            return Result.Failure<bool>(new Error("NotFound", "Task not found"));
        }

        if (task.UserId != userId)
        {
            return Result.Failure<bool>(new Error("Forbidden", "You do not own this task"));
        }

        task.ToggleCompletion();
        await this.taskRepository.UpdateAsync(task, cancellationToken);
        await this.unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
