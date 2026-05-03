using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Activities.Commands.DeleteActivity;

public sealed class DeleteActivityCommandHandler : IRequestHandler<DeleteActivityCommand, Result<Unit>>
{
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteActivityCommandHandler(IActivityRepository activityRepository, IUnitOfWork unitOfWork)
    {
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(request.Id, cancellationToken);
        if (activity == null || activity.UserId != request.UserId)
        {
            return Result.Failure<Unit>(new Error("NotFound", "La actividad no existe o no tienes acceso."));
        }

        var success = await _activityRepository.DeleteAsync(request.Id, cancellationToken);
        if (!success)
        {
            return Result.Failure<Unit>(new Error("InternalError", "No se pudo eliminar la actividad."));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
