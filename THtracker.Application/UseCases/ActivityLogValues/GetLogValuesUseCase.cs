using THtracker.Application.DTOs.ActivityLogValues;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogValues;

public class GetLogValuesUseCase
{
    private readonly IActivityLogValueRepository _valueRepository;
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;

    public GetLogValuesUseCase(
        IActivityLogValueRepository valueRepository,
        IActivityLogRepository logRepository,
        IActivityRepository activityRepository)
    {
        _valueRepository = valueRepository;
        _logRepository = logRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Result<IEnumerable<LogValueResponse>>> ExecuteAsync(
        Guid userId,
        Guid logId,
        CancellationToken cancellationToken = default)
    {
        var log = await _logRepository.GetByIdAsync(logId, cancellationToken);
        if (log == null)
            return Result.Failure<IEnumerable<LogValueResponse>>(new Error("NotFound", "El registro no existe."));

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != userId)
            return Result.Failure<IEnumerable<LogValueResponse>>(new Error("Forbidden", "No tienes acceso a este registro."));

        var values = await _valueRepository.GetAllByLogAsync(logId, cancellationToken);

        return Result.Success(values.Select(v => new LogValueResponse(
            v.Id,
            v.ActivityLogId,
            v.ValueDefinitionId,
            v.Value
        )));
    }
}
