using THtracker.Application.DTOs.ActivityValueDefinitions;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityValueDefinitions;

public class DeleteValueDefinitionUseCase
{
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteValueDefinitionUseCase(
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork)
    {
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> ExecuteAsync(
        Guid userId,
        Guid activityId,
        Guid definitionId,
        CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null)
            return Result.Failure<bool>(new Error("NotFound", "La actividad no existe."));

        if (activity.UserId != userId)
            return Result.Failure<bool>(new Error("Forbidden", "No tienes acceso a esta actividad."));

        var definition = await _definitionRepository.GetByIdAsync(definitionId, cancellationToken);
        if (definition == null || definition.ActivityId != activityId)
            return Result.Failure<bool>(new Error("NotFound", "La definición no existe."));

        var deleted = await _definitionRepository.DeleteAsync(definitionId, cancellationToken);
        if (!deleted)
            return Result.Failure<bool>(new Error("Internal", "Error al eliminar la definición."));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}