using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityValueDefinitions.Queries.GetAllValueDefinitions;

public sealed class GetAllValueDefinitionsQueryHandler : IRequestHandler<GetAllValueDefinitionsQuery, Result<IEnumerable<ActivityValueDefinitionResponse>>>
{
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;

    public GetAllValueDefinitionsQueryHandler(
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository)
    {
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Result<IEnumerable<ActivityValueDefinitionResponse>>> Handle(GetAllValueDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(request.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != request.UserId)
        {
            return Result.Failure<IEnumerable<ActivityValueDefinitionResponse>>(new Error("NotFound", "La actividad no existe o no tienes acceso."));
        }

        var definitions = await _definitionRepository.GetAllByActivityAsync(request.ActivityId, cancellationToken);
        
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
