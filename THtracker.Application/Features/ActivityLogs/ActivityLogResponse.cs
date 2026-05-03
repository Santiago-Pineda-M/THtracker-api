namespace THtracker.Application.Features.ActivityLogs;

public sealed record ActivityLogResponse(
    Guid Id, 
    Guid ActivityId, 
    DateTime StartedAt, 
    DateTime? EndedAt);
