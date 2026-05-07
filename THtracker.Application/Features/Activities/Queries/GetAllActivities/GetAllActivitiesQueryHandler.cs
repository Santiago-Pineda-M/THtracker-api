using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Activities.Queries.GetAllActivities;

public sealed class GetAllActivitiesQueryHandler : IRequestHandler<GetAllActivitiesQuery, Result<PaginatedResponse<ActivityResponse>>>
{
    private readonly IActivityRepository _activityRepository;

    public GetAllActivitiesQueryHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<Result<PaginatedResponse<ActivityResponse>>> Handle(
        GetAllActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var page = await _activityRepository.GetPageByUserAsync(
            request.UserId,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = page.Items
            .Select(a => new ActivityResponse(
                a.Id,
                a.UserId,
                a.CategoryId,
                a.Name,
                a.Color,
                a.AllowOverlap))
            .ToList();

        return Result.Success(new PaginatedResponse<ActivityResponse>(
            items,
            page.TotalCount,
            pageNumber,
            pageSize));
    }
}
