using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityLogs.Queries.GetActivityLogById;

public sealed class GetActivityLogByIdQueryHandler : IRequestHandler<GetActivityLogByIdQuery, Result<ActivityLogResponse>>
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;

    public GetActivityLogByIdQueryHandler(
        IActivityLogRepository logRepository, 
        IActivityRepository activityRepository)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Result<ActivityLogResponse>> Handle(GetActivityLogByIdQuery request, CancellationToken cancellationToken)
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

        return new ActivityLogResponse(
            log.Id, 
            log.ActivityId, 
            log.StartedAt, 
            log.EndedAt);
    }
}
