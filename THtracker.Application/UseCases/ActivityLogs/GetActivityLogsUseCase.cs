using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Application.DTOs.ActivityLogValues;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogs;

public class GetActivityLogsUseCase
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;

    public GetActivityLogsUseCase(
        IActivityLogRepository logRepository,
        IActivityRepository activityRepository)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Result<IEnumerable<ActivityLogResponse>>> ExecuteAsync(
        Guid userId,
        GetActivityLogsRequest request,
        CancellationToken cancellationToken = default)
    {
        // Si se provee ActivityId, validamos que exista y pertenezca al usuario
        if (request.ActivityId.HasValue)
        {
            var activity = await _activityRepository.GetByIdAsync(request.ActivityId.Value, cancellationToken);
            if (activity == null)
                return Result.Failure<IEnumerable<ActivityLogResponse>>(new Error("NotFound", "La actividad no existe."));

            if (activity.UserId != userId)
                return Result.Failure<IEnumerable<ActivityLogResponse>>(new Error("Forbidden", "No tienes acceso a esta actividad."));
        }

        var logs = await _logRepository.GetLogsAsync(
            userId, 
            request.ActivityId, 
            request.StartDate, 
            request.EndDate, 
            cancellationToken);

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
