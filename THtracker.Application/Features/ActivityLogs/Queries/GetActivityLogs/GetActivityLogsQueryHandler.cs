using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityLogs.Queries.GetActivityLogs;

public sealed class GetActivityLogsQueryHandler : IRequestHandler<GetActivityLogsQuery, Result<PaginatedResponse<ActivityLogResponse>>>
{
    private readonly IActivityLogRepository _logRepository;

    public GetActivityLogsQueryHandler(IActivityLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<Result<PaginatedResponse<ActivityLogResponse>>> Handle(
        GetActivityLogsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var page = await _logRepository.GetLogsPageForUserAsync(
            request.UserId,
            request.ActivityId,
            request.From,
            request.To,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = page.Items
            .Select(l => new ActivityLogResponse(
                l.Id,
                l.ActivityId,
                l.StartedAt,
                l.EndedAt))
            .ToList();

        return Result.Success(new PaginatedResponse<ActivityLogResponse>(
            items,
            page.TotalCount,
            pageNumber,
            pageSize));
    }
}
