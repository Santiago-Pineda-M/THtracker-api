using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityLogs.Queries.GetActiveActivityLogs;

public sealed class GetActiveActivityLogsQueryHandler : IRequestHandler<GetActiveActivityLogsQuery, Result<PaginatedResponse<ActivityLogResponse>>>
{
    private readonly IActivityLogRepository _logRepository;

    public GetActiveActivityLogsQueryHandler(IActivityLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<Result<PaginatedResponse<ActivityLogResponse>>> Handle(
        GetActiveActivityLogsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var page = await _logRepository.GetActiveLogsPageForUserAsync(
            request.UserId,
            null,
            null,
            null,
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
