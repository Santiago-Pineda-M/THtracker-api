using MediatR;
using THtracker.Application.Common;
using THtracker.Application.Features.ActivityValueDefinitions;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityValueDefinitions.Queries.GetAllValueDefinitions;

public sealed class GetAllValueDefinitionsQueryHandler : IRequestHandler<GetAllValueDefinitionsQuery, Result<PaginatedResponse<ActivityValueDefinitionResponse>>>
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

    public async Task<Result<PaginatedResponse<ActivityValueDefinitionResponse>>> Handle(
        GetAllValueDefinitionsQuery request,
        CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(request.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != request.UserId)
        {
            return Result.Failure<PaginatedResponse<ActivityValueDefinitionResponse>>(
                new Error("NotFound", "La actividad no existe o no tienes acceso."));
        }

        var (pageNumber, pageSize) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var page = await _definitionRepository.GetPageByActivityAsync(
            request.ActivityId,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = page.Items
            .Select(d => new ActivityValueDefinitionResponse(
                d.Id,
                d.ActivityId,
                d.Name,
                d.ValueType,
                d.IsRequired,
                d.Unit,
                d.MinValue,
                d.MaxValue))
            .ToList();

        return Result.Success(new PaginatedResponse<ActivityValueDefinitionResponse>(
            items,
            page.TotalCount,
            pageNumber,
            pageSize));
    }
}
