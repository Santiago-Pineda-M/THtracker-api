using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityLogs.Queries.GetActivityLogs;

public sealed record GetActivityLogsQuery(
    Guid UserId,
    Guid? ActivityId = null,
    DateTime? From = null,
    DateTime? To = null,
    int PageNumber = 1,
    int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PaginatedResponse<ActivityLogResponse>>>;
