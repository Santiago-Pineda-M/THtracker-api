using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskResponse>>
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskByIdQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskResponse>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (task == null || task.UserId != request.UserId)
        {
            return Result.Failure<TaskResponse>(new Error("NotFound", "La tarea no existe o no tienes acceso."));
        }

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
