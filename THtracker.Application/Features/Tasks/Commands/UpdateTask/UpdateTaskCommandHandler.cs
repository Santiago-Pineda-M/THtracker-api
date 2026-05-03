using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<TaskResponse>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TaskResponse>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (task == null || task.UserId != request.UserId)
        {
            return Result.Failure<TaskResponse>(new Error("NotFound", "La tarea no existe o no tienes acceso."));
        }

        task.Update(request.Content, request.DueDate);
        
        await _taskRepository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TaskResponse(
            task.Id,
            task.TaskListId,
            task.UserId,
            task.Content,
            task.IsCompleted,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}
