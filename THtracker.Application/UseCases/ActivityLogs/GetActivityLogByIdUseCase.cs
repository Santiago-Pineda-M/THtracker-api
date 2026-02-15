using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Application.DTOs.ActivityLogValues;
using THtracker.Domain.Common;
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

    public async Task<Result<ActivityLogResponse>> ExecuteAsync(
        Guid userId,
        Guid logId,
        CancellationToken cancellationToken = default)
    {
        var log = await _logRepository.GetByIdAsync(logId, cancellationToken);
        if (log == null)
            return Result.Failure<ActivityLogResponse>(new Error("NotFound", "El registro no existe."));

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != userId)
            return Result.Failure<ActivityLogResponse>(new Error("Forbidden", "No tienes acceso a este registro."));

        double? durationMinutes = null;
        if (log.EndedAt.HasValue)
            durationMinutes = (log.EndedAt.Value - log.StartedAt).TotalMinutes;

        var values = log.LogValues.Select(v => new LogValueResponse(
            v.Id,
            v.ActivityLogId,
            v.ValueDefinitionId,
            v.Value
        ));

        return Result.Success(new ActivityLogResponse(
            log.Id, 
            log.ActivityId, 
            log.StartedAt, 
            log.EndedAt, 
            durationMinutes,
            values));
    }
}
