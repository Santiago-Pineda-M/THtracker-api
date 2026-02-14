using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogs;

public class StartActivityUseCase
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;

    public StartActivityUseCase(IActivityLogRepository logRepository, IActivityRepository activityRepository)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
    }

    public async Task<ActivityLogResponse> ExecuteAsync(Guid userId, StartActivityLogRequest request, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(request.ActivityId, cancellationToken);
        if (activity == null)
            throw new Exception("La actividad no existe.");

        if (activity.UserId != userId)
            throw new Exception("No tienes acceso a esta actividad.");

        // Check for active logs and overlap rules
        var activeLogs = await _logRepository.GetActiveLogsByUserAsync(userId, cancellationToken);
        
        if (activeLogs.Any())
        {
            // If the NEW activity doesn't allow overlap, it can only start if there are NO active logs.
            if (!activity.AllowOverlap)
            {
                throw new Exception("No puedes iniciar una actividad que no permite solapamiento mientras hay otras actividades activas.");
            }

            // If the NEW activity ALLOWS overlap, we must check if all CURRENT active activities also allow it.
            foreach (var activeLog in activeLogs)
            {
                var activeActivity = await _activityRepository.GetByIdAsync(activeLog.ActivityId, cancellationToken);
                if (activeActivity != null && !activeActivity.AllowOverlap)
                {
                    throw new Exception($"No puedes iniciar esta actividad porque '{activeActivity.Name}' está activa y no permite solapamiento.");
                }
            }
        }

        var log = new ActivityLog(request.ActivityId, DateTime.UtcNow);
        await _logRepository.AddAsync(log, cancellationToken);

        return new ActivityLogResponse(log.Id, log.ActivityId, log.StartedAt, log.EndedAt);
    }
}
