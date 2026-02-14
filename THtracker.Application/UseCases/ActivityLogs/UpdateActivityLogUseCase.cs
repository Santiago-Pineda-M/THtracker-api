using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Application.Validators.ActivityLogs;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogs;

public class UpdateActivityLogUseCase
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;

    public UpdateActivityLogUseCase(IActivityLogRepository logRepository, IActivityRepository activityRepository)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
    }

    public async Task<ActivityLogResponse?> ExecuteAsync(Guid userId, Guid logId, UpdateActivityLogRequest request, CancellationToken cancellationToken = default)
    {
        var validator = new UpdateActivityLogRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Datos inválidos: {errors}");
        }

        var log = await _logRepository.GetByIdAsync(logId, cancellationToken);
        if (log == null) return null;

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != userId)
            throw new Exception("No tienes acceso a este registro.");

        // Overlap Validation for Edit
        // We only check if the activity (current or others) doesn't allow overlap
        await ValidateOverlapAsync(userId, logId, activity, request.StartedAt, request.EndedAt, cancellationToken);

        // Update domain entity
        log.UpdatePeriod(request.StartedAt, request.EndedAt);

        await _logRepository.UpdateAsync(log, cancellationToken);

        return new ActivityLogResponse(
            log.Id,
            log.ActivityId,
            log.StartedAt,
            log.EndedAt,
            log.EndedAt.HasValue ? (log.EndedAt.Value - log.StartedAt).TotalMinutes : null
        );
    }

    private async Task ValidateOverlapAsync(Guid userId, Guid logId, Activity activity, DateTime start, DateTime? end, CancellationToken ct)
    {
        var effectiveEnd = end ?? DateTime.UtcNow;

        var overlappingLogs = await _logRepository.GetOverlappingLogsAsync(userId, start, effectiveEnd, logId, ct);
        
        if (overlappingLogs.Any())
        {
            // If the activity being edited doesn't allow overlap, it's an error.
            if (!activity.AllowOverlap)
            {
                throw new Exception("No puedes mover este registro a esta fecha porque no permite solapamiento y ya hay otra actividad en ese horario.");
            }

            // If it allows overlap, we must check if all OVERLAPPING activities also allow it.
            foreach (var overlapLog in overlappingLogs)
            {
                var overlapActivity = await _activityRepository.GetByIdAsync(overlapLog.ActivityId, ct);
                if (overlapActivity != null && !overlapActivity.AllowOverlap)
                {
                    throw new Exception($"No puedes editar este registro porque coincide con '{overlapActivity.Name}', la cual no permite solapamiento.");
                }
            }
        }
    }
}
