namespace THtracker.Application.DTOs.ActivityLogs;

public record ActivityLogResponse(
    Guid Id, 
    Guid ActivityId, 
    DateTime StartedAt, 
    DateTime? EndedAt,
    double? DurationMinutes = null
);
