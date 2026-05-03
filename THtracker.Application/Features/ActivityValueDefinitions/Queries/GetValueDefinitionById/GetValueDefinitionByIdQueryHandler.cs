using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityValueDefinitions.Queries.GetValueDefinitionById;

public sealed class GetValueDefinitionByIdQueryHandler : IRequestHandler<GetValueDefinitionByIdQuery, Result<ActivityValueDefinitionResponse>>
{
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;

    public GetValueDefinitionByIdQueryHandler(
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository)
    {
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Result<ActivityValueDefinitionResponse>> Handle(GetValueDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(request.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != request.UserId)
        {
            return Result.Failure<ActivityValueDefinitionResponse>(new Error("NotFound", "La actividad no existe o no tienes acceso."));
        }

        var definition = await _definitionRepository.GetByIdAsync(request.DefinitionId, cancellationToken);
        
        if (definition == null || definition.ActivityId != request.ActivityId)
        {
            return Result.Failure<ActivityValueDefinitionResponse>(new Error("NotFound", "La definición de valor no existe para esta actividad."));
        }

        return new ActivityValueDefinitionResponse(
            definition.Id,
            definition.ActivityId,
            definition.Name,
            definition.ValueType,
            definition.IsRequired,
            definition.Unit,
            definition.MinValue,
            definition.MaxValue
        );
    }
}
