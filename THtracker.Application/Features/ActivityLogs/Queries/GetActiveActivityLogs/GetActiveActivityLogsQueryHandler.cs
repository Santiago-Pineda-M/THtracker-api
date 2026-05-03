using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityLogs.Queries.GetActiveActivityLogs;

public sealed class GetActiveActivityLogsQueryHandler : IRequestHandler<GetActiveActivityLogsQuery, Result<IEnumerable<ActivityLogResponse>>>
{
    private readonly IActivityLogRepository _logRepository;

    public GetActiveActivityLogsQueryHandler(IActivityLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<Result<IEnumerable<ActivityLogResponse>>> Handle(GetActiveActivityLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _logRepository.GetActiveLogsByUserAsync(request.UserId, cancellationToken);

        var response = logs.Select(l => new ActivityLogResponse(
            l.Id, 
            l.ActivityId, 
            l.StartedAt, 
            l.EndedAt));

        return Result.Success(response);
    }
}
