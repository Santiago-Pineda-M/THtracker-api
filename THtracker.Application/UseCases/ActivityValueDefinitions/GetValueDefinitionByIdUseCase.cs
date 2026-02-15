using THtracker.Application.DTOs.ActivityValueDefinitions;
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

    public async Task<ActivityValueDefinitionResponse?> ExecuteAsync(
        Guid userId,
        Guid activityId,
        Guid definitionId,
        CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null || activity.UserId != userId)
            return null;

        var definition = await _definitionRepository.GetByIdAsync(definitionId, cancellationToken);
        if (definition == null || definition.ActivityId != activityId)
            return null;

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
