using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogs;

public class GetActivityLogByIdUseCase
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;

    public GetActivityLogByIdUseCase(
        IActivityLogRepository logRepository,
        IActivityRepository activityRepository)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
    }

    public async Task<ActivityLogResponse?> ExecuteAsync(
        Guid userId,
        Guid logId,
        CancellationToken cancellationToken = default)
    {
        var log = await _logRepository.GetByIdAsync(logId, cancellationToken);
        if (log == null)
            return null;

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != userId)
            return null;

        double? durationMinutes = null;
        if (log.EndedAt.HasValue)
            durationMinutes = (log.EndedAt.Value - log.StartedAt).TotalMinutes;

        return new ActivityLogResponse(log.Id, log.ActivityId, log.StartedAt, log.EndedAt, durationMinutes);
    }
}
