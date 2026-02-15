using FluentValidation;
using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogs;

public class UpdateActivityLogUseCase
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateActivityLogRequest> _validator;

    public UpdateActivityLogUseCase(
        IActivityLogRepository logRepository, 
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateActivityLogRequest> validator)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<ActivityLogResponse>> ExecuteAsync(Guid userId, Guid logId, UpdateActivityLogRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<ActivityLogResponse>(new Error("Validation", errors));
        }

        var log = await _logRepository.GetByIdAsync(logId, cancellationToken);
        if (log == null)
            return Result.Failure<ActivityLogResponse>(new Error("NotFound", "El registro no existe."));

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != userId)
            return Result.Failure<ActivityLogResponse>(new Error("Forbidden", "No tienes acceso a este registro."));

        var overlapResult = await ValidateOverlapAsync(userId, logId, activity, request.StartedAt, request.EndedAt, cancellationToken);
        if (overlapResult.IsFailure)
        {
            return Result.Failure<ActivityLogResponse>(overlapResult.Error);
        }

        log.UpdatePeriod(request.StartedAt, request.EndedAt);

        await _logRepository.UpdateAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivityLogResponse(
            log.Id,
            log.ActivityId,
            log.StartedAt,
            log.EndedAt,
            log.EndedAt.HasValue ? (log.EndedAt.Value - log.StartedAt).TotalMinutes : null
        );
    }

    private async Task<Result> ValidateOverlapAsync(Guid userId, Guid logId, Activity activity, DateTime start, DateTime? end, CancellationToken ct)
    {
        var effectiveEnd = end ?? DateTime.UtcNow;

        var overlappingLogs = await _logRepository.GetOverlappingLogsAsync(userId, start, effectiveEnd, logId, ct);
        
        if (overlappingLogs.Any())
        {
            if (!activity.AllowOverlap)
            {
                return Result.Failure(new Error("OverlapConflict", "No puedes mover este registro a esta fecha porque no permite solapamiento y ya hay otra actividad en ese horario."));
            }

            foreach (var overlapLog in overlappingLogs)
            {
                var overlapActivity = await _activityRepository.GetByIdAsync(overlapLog.ActivityId, ct);
                if (overlapActivity != null && !overlapActivity.AllowOverlap)
                {
                    return Result.Failure(new Error("OverlapConflict", $"No puedes editar este registro porque coincide con '{overlapActivity.Name}', la cual no permite solapamiento."));
                }
            }
        }

        return Result.Success();
    }
}
