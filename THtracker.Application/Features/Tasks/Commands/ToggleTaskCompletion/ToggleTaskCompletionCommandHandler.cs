using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Tasks.Commands.ToggleTaskCompletion;

public sealed class ToggleTaskCompletionCommandHandler : IRequestHandler<ToggleTaskCompletionCommand, Result<Unit>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleTaskCompletionCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(ToggleTaskCompletionCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (task == null || task.UserId != request.UserId)
        {
            return Result.Failure<Unit>(new Error("NotFound", "La tarea no existe o no tienes acceso."));
        }

        task.ToggleCompletion();
        
        await _taskRepository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
