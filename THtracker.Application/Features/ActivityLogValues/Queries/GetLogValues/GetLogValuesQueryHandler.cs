using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityLogValues.Queries.GetLogValues;

public sealed class GetLogValuesQueryHandler : IRequestHandler<GetLogValuesQuery, Result<IEnumerable<LogValueResponse>>>
{
    private readonly IActivityLogValueRepository _logValueRepository;
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;

    public GetLogValuesQueryHandler(
        IActivityLogValueRepository logValueRepository,
        IActivityLogRepository logRepository,
        IActivityRepository activityRepository)
    {
        _logValueRepository = logValueRepository;
        _logRepository = logRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Result<IEnumerable<LogValueResponse>>> Handle(GetLogValuesQuery request, CancellationToken cancellationToken)
    {
        var log = await _logRepository.GetByIdAsync(request.ActivityLogId, cancellationToken);
        if (log == null)
            return Result.Failure<IEnumerable<LogValueResponse>>(new Error("NotFound", "El registro de actividad no existe."));

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != request.UserId)
            return Result.Failure<IEnumerable<LogValueResponse>>(new Error("Forbidden", "No tienes acceso a este registro."));

        var values = await _logValueRepository.GetAllByLogAsync(request.ActivityLogId, cancellationToken);
        
        var response = values.Select(v => new LogValueResponse(
            v.Id,
            v.ActivityLogId,
            v.ValueDefinitionId,
            v.Value
        ));

        return Result.Success(response);
    }
}
