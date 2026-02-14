namespace THtracker.Application.DTOs.ActivityLogs;

public record UpdateActivityLogRequest(
    DateTime StartedAt,
    DateTime? EndedAt
);
