using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogs;

public class StopActivityUseCase
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;

    public StopActivityUseCase(IActivityLogRepository logRepository, IActivityRepository activityRepository)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
    }

    public async Task<ActivityLogResponse?> ExecuteAsync(Guid userId, Guid logId, CancellationToken cancellationToken = default)
    {
        var log = await _logRepository.GetByIdAsync(logId, cancellationToken);
        if (log == null)
            return null;

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != userId)
            throw new Exception("No tienes acceso a este registro.");

        if (log.EndedAt != null)
            throw new Exception("Esta actividad ya ha sido finalizada.");

        log.Stop(DateTime.UtcNow);
        await _logRepository.UpdateAsync(log, cancellationToken);
        
        return new ActivityLogResponse(
            log.Id, 
            log.ActivityId, 
            log.StartedAt, 
            log.EndedAt, 
            (log.EndedAt.Value - log.StartedAt).TotalMinutes
        );
    }
}
