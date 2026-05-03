namespace THtracker.Application.Features.TaskLists;

public sealed record TaskListResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Color,
    DateTime CreatedAt,
    DateTime UpdatedAt);
