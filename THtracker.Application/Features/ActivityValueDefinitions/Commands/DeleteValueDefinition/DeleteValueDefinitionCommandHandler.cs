using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityValueDefinitions.Commands.DeleteValueDefinition;

public sealed class DeleteValueDefinitionCommandHandler : IRequestHandler<DeleteValueDefinitionCommand, Result<Unit>>
{
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteValueDefinitionCommandHandler(
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork)
    {
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteValueDefinitionCommand request, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(request.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != request.UserId)
        {
            return Result.Failure<Unit>(new Error("NotFound", "La actividad no existe o no tienes acceso."));
        }

        var definition = await _definitionRepository.GetByIdAsync(request.DefinitionId, cancellationToken);
        if (definition == null || definition.ActivityId != request.ActivityId)
        {
            return Result.Failure<Unit>(new Error("NotFound", "La definición de valor no existe para esta actividad."));
        }

        var success = await _definitionRepository.DeleteAsync(request.DefinitionId, cancellationToken);
        if (!success)
        {
            return Result.Failure<Unit>(new Error("InternalError", "No se pudo eliminar la definición."));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
