using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Activities.Queries.GetAllActivities;

public sealed record GetAllActivitiesQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PaginatedResponse<ActivityResponse>>>;
