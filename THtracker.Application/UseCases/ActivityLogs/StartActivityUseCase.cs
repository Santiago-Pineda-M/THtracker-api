using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogs;

public class StartActivityUseCase
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StartActivityUseCase(
        IActivityLogRepository logRepository, 
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivityLogResponse>> ExecuteAsync(Guid userId, StartActivityLogRequest request, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(request.ActivityId, cancellationToken);
        if (activity == null)
            return Result.Failure<ActivityLogResponse>(new Error("NotFound", "La actividad no existe."));

        if (activity.UserId != userId)
            return Result.Failure<ActivityLogResponse>(new Error("Forbidden", "No tienes acceso a esta actividad."));

        var activeLogs = await _logRepository.GetActiveLogsByUserAsync(userId, cancellationToken);
        
        if (activeLogs.Any())
        {
            if (!activity.AllowOverlap)
            {
                return Result.Failure<ActivityLogResponse>(new Error("OverlapConflict", "No puedes iniciar una actividad que no permite solapamiento mientras hay otras actividades activas."));
            }

            foreach (var activeLog in activeLogs)
            {
                var activeActivity = await _activityRepository.GetByIdAsync(activeLog.ActivityId, cancellationToken);
                if (activeActivity != null && !activeActivity.AllowOverlap)
                {
                    return Result.Failure<ActivityLogResponse>(new Error("OverlapConflict", $"No puedes iniciar esta actividad porque '{activeActivity.Name}' está activa y no permite solapamiento."));
                }
            }
        }

        var log = new ActivityLog(request.ActivityId, DateTime.UtcNow);
        await _logRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivityLogResponse(log.Id, log.ActivityId, log.StartedAt, log.EndedAt);
    }
}
