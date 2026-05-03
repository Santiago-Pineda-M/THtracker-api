using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result<Unit>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (task == null || task.UserId != request.UserId)
        {
            return Result.Failure<Unit>(new Error("NotFound", "La tarea no existe o no tienes acceso."));
        }

        var success = await _taskRepository.DeleteAsync(request.Id, cancellationToken);
        if (!success)
        {
            return Result.Failure<Unit>(new Error("InternalError", "No se pudo eliminar la tarea."));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
