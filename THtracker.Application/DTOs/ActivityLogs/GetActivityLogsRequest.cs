namespace THtracker.Application.DTOs.ActivityLogs;

public record GetActivityLogsRequest(
    Guid? ActivityId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);
