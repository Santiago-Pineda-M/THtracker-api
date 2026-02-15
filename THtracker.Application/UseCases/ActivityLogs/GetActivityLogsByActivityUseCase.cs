using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogs;

public class GetActivityLogsByActivityUseCase
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;

    public GetActivityLogsByActivityUseCase(
        IActivityLogRepository logRepository,
        IActivityRepository activityRepository)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<ActivityLogResponse>> ExecuteAsync(
        Guid userId,
        Guid activityId,
        CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null || activity.UserId != userId)
            return Array.Empty<ActivityLogResponse>();

        var logs = await _logRepository.GetAllByActivityAsync(activityId, cancellationToken);

        return logs.Select(log =>
        {
            double? durationMinutes = null;
            if (log.EndedAt.HasValue)
                durationMinutes = (log.EndedAt.Value - log.StartedAt).TotalMinutes;
            return new ActivityLogResponse(log.Id, log.ActivityId, log.StartedAt, log.EndedAt, durationMinutes);
        });
    }
}
