using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityLogs.Commands.UpdateActivityLog;

public sealed class UpdateActivityLogCommandHandler : IRequestHandler<UpdateActivityLogCommand, Result<ActivityLogResponse>>
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateActivityLogCommandHandler(
        IActivityLogRepository logRepository, 
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivityLogResponse>> Handle(UpdateActivityLogCommand request, CancellationToken cancellationToken)
    {
        var log = await _logRepository.GetByIdAsync(request.Id, cancellationToken);
        if (log == null)
        {
            return Result.Failure<ActivityLogResponse>(new Error("NotFound", "El registro no existe."));
        }

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != request.UserId)
        {
            return Result.Failure<ActivityLogResponse>(new Error("NotFound", "El registro no existe o no tienes acceso."));
        }

        var overlapResult = await ValidateOverlapAsync(request.UserId, request.Id, activity, request.StartedAt, request.EndedAt, cancellationToken);
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
            log.EndedAt
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
                return Result.Failure(new Error("OverlapConflict", "Conflicto de solapamiento: esta actividad no permite solapamientos en el horario especificado."));
            }

            foreach (var overlapLog in overlappingLogs)
            {
                var overlapActivity = await _activityRepository.GetByIdAsync(overlapLog.ActivityId, ct);
                if (overlapActivity != null && !overlapActivity.AllowOverlap)
                {
                    return Result.Failure(new Error("OverlapConflict", $"Conflicto con '{overlapActivity.Name}', la cual no permite solapamiento."));
                }
            }
        }

        return Result.Success();
    }
}
