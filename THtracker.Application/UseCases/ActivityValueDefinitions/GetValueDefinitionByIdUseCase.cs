using THtracker.Application.DTOs.ActivityValueDefinitions;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityValueDefinitions;

public class GetValueDefinitionByIdUseCase
{
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;

    public GetValueDefinitionByIdUseCase(
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository)
    {
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Result<ActivityValueDefinitionResponse>> ExecuteAsync(
        Guid userId,
        Guid activityId,
        Guid definitionId,
        CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null)
            return Result.Failure<ActivityValueDefinitionResponse>(new Error("NotFound", "La actividad no existe."));

        if (activity.UserId != userId)
            return Result.Failure<ActivityValueDefinitionResponse>(new Error("Forbidden", "No tienes acceso a esta actividad."));

        var definition = await _definitionRepository.GetByIdAsync(definitionId, cancellationToken);
        if (definition == null || definition.ActivityId != activityId)
            return Result.Failure<ActivityValueDefinitionResponse>(new Error("NotFound", "La definición no existe."));

        return new ActivityValueDefinitionResponse(
            definition.Id,
            definition.ActivityId,
            definition.Name,
            definition.ValueType,
            definition.IsRequired,
            definition.Unit,
            definition.MinValue,
            definition.MaxValue);
    }
}
