using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogs;

public class StopActivityUseCase
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StopActivityUseCase(
        IActivityLogRepository logRepository, 
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivityLogResponse>> ExecuteAsync(Guid userId, Guid logId, CancellationToken cancellationToken = default)
    {
        var log = await _logRepository.GetByIdAsync(logId, cancellationToken);
        if (log == null)
            return Result.Failure<ActivityLogResponse>(new Error("NotFound", "El registro no existe."));

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != userId)
            return Result.Failure<ActivityLogResponse>(new Error("Forbidden", "No tienes acceso a este registro."));

        if (log.EndedAt != null)
            return Result.Failure<ActivityLogResponse>(new Error("Validation", "Esta actividad ya ha sido finalizada."));

        log.Stop(DateTime.UtcNow);
        await _logRepository.UpdateAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var endedAt = log.EndedAt!.Value;
        return new ActivityLogResponse(
            log.Id,
            log.ActivityId,
            log.StartedAt,
            log.EndedAt,
            (endedAt - log.StartedAt).TotalMinutes
        );
    }
}
