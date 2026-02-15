using THtracker.Application.DTOs.ActivityValueDefinitions;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityValueDefinitions;

public class GetValueDefinitionsUseCase
{
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;

    public GetValueDefinitionsUseCase(
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository)
    {
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Result<IEnumerable<ActivityValueDefinitionResponse>>> ExecuteAsync(Guid userId, Guid activityId, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null)
            return Result.Failure<IEnumerable<ActivityValueDefinitionResponse>>(new Error("NotFound", "La actividad no existe."));

        if (activity.UserId != userId)
            return Result.Failure<IEnumerable<ActivityValueDefinitionResponse>>(new Error("Forbidden", "No tienes acceso a esta actividad."));

        var definitions = await _definitionRepository.GetAllByActivityAsync(activityId, cancellationToken);

        var response = definitions.Select(d => new ActivityValueDefinitionResponse(
            d.Id,
            d.ActivityId,
            d.Name,
            d.ValueType,
            d.IsRequired,
            d.Unit,
            d.MinValue,
            d.MaxValue
        ));

        return Result.Success(response);
    }
}
