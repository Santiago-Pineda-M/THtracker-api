using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityLogs.Commands.StopActivity;

public sealed class StopActivityCommandHandler : IRequestHandler<StopActivityCommand, Result<ActivityLogResponse>>
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StopActivityCommandHandler(
        IActivityLogRepository logRepository, 
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivityLogResponse>> Handle(StopActivityCommand request, CancellationToken cancellationToken)
    {
        var log = await _logRepository.GetByIdAsync(request.Id, cancellationToken);
        if (log == null)
        {
            return Result.Failure<ActivityLogResponse>(new Error("NotFound", "El registro no existe."));
        }

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != request.UserId)
        {
            return Result.Failure<ActivityLogResponse>(new Error("Forbidden", "No tienes acceso a este registro."));
        }

        if (log.EndedAt != null)
        {
            return Result.Failure<ActivityLogResponse>(new Error("Validation", "Esta actividad ya ha sido finalizada."));
        }

        log.Stop(DateTime.UtcNow);
        await _logRepository.UpdateAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivityLogResponse(
            log.Id,
            log.ActivityId,
            log.StartedAt,
            log.EndedAt
        );
    }
}
