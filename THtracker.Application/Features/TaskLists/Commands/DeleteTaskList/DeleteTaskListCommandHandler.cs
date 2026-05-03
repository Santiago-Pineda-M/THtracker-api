using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.TaskLists.Commands.DeleteTaskList;

public sealed class DeleteTaskListCommandHandler : IRequestHandler<DeleteTaskListCommand, Result>
{
    private readonly ITaskListRepository _taskListRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskListCommandHandler(
        ITaskListRepository taskListRepository,
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork)
    {
        _taskListRepository = taskListRepository;
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteTaskListCommand request, CancellationToken cancellationToken)
    {
        var taskList = await _taskListRepository.GetByIdAsync(request.Id, cancellationToken);

        if (taskList == null || taskList.UserId != request.UserId)
        {
            return Result.Failure(new Error("NotFound", "La lista de tareas no existe o no tienes acceso."));
        }

        // Eliminar las tareas asociadas a la lista
        var tasks = await _taskRepository.GetAllByTaskListAsync(request.Id, cancellationToken);
        foreach (var task in tasks)
        {
            await _taskRepository.DeleteAsync(task.Id, cancellationToken);
        }

        await _taskListRepository.DeleteAsync(request.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
