using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Application.DTOs.ActivityLogValues;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogs;

public class GetActiveActivityLogsUseCase
{
    private readonly IActivityLogRepository _logRepository;

    public GetActiveActivityLogsUseCase(IActivityLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<Result<IEnumerable<ActivityLogResponse>>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var logs = await _logRepository.GetActiveLogsByUserAsync(userId, cancellationToken);

        var response = logs.Select(log =>
        {
            double? durationMinutes = null;
            if (log.EndedAt.HasValue)
                durationMinutes = (log.EndedAt.Value - log.StartedAt).TotalMinutes;
            else
                durationMinutes = (DateTime.UtcNow - log.StartedAt).TotalMinutes;

            var values = log.LogValues.Select(v => new LogValueResponse(
                v.Id,
                v.ActivityLogId,
                v.ValueDefinitionId,
                v.Value
            ));

            return new ActivityLogResponse(
                log.Id,
                log.ActivityId,
                log.StartedAt,
                log.EndedAt,
                durationMinutes,
                values);
        });

        return Result.Success(response);
    }
}
