using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Application.DTOs.ActivityLogValues;
using THtracker.Domain.Common;
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

    public async Task<Result<IEnumerable<ActivityLogResponse>>> ExecuteAsync(
        Guid userId,
        Guid activityId,
        CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null)
            return Result.Failure<IEnumerable<ActivityLogResponse>>(new Error("NotFound", "La actividad no existe."));

        if (activity.UserId != userId)
            return Result.Failure<IEnumerable<ActivityLogResponse>>(new Error("Forbidden", "No tienes acceso a esta actividad."));

        var logs = await _logRepository.GetAllByActivityAsync(activityId, cancellationToken);

        var response = logs.Select(log =>
        {
            double? durationMinutes = null;
            if (log.EndedAt.HasValue)
                durationMinutes = (log.EndedAt.Value - log.StartedAt).TotalMinutes;
            
            var values = log.LogValues.Select(v => new LogValueResponse(
                v.Id,
                v.ActivityLogId,
                v.ValueDefinitionId,
                v.Value
            ));

            return new ActivityLogResponse(
                log.Id, 
                log.ActivityId, 
                log.StartedAt, 
                log.EndedAt, 
                durationMinutes,
                values);
        });

        return Result.Success(response);
    }
}
