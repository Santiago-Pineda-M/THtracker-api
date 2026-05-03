using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityLogs.Queries.GetActivityLogs;

public sealed class GetActivityLogsQueryHandler : IRequestHandler<GetActivityLogsQuery, Result<IEnumerable<ActivityLogResponse>>>
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;

    public GetActivityLogsQueryHandler(
        IActivityLogRepository logRepository, 
        IActivityRepository activityRepository)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Result<IEnumerable<ActivityLogResponse>>> Handle(GetActivityLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _logRepository.GetActiveLogsByUserAsync(request.UserId, cancellationToken);

        var filteredLogs = logs.AsEnumerable();

        if (request.ActivityId.HasValue)
        {
            filteredLogs = filteredLogs.Where(l => l.ActivityId == request.ActivityId.Value);
        }

        if (request.From.HasValue)
        {
            filteredLogs = filteredLogs.Where(l => l.StartedAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            filteredLogs = filteredLogs.Where(l => l.StartedAt <= request.To.Value);
        }

        var response = filteredLogs.Select(l => new ActivityLogResponse(
            l.Id, 
            l.ActivityId, 
            l.StartedAt, 
            l.EndedAt));

        return Result.Success(response);
    }
}
