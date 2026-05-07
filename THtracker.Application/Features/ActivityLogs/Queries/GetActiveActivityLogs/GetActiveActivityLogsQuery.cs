using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityLogs.Queries.GetActiveActivityLogs;

public sealed record GetActiveActivityLogsQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PaginatedResponse<ActivityLogResponse>>>;
