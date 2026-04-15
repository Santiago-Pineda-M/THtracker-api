namespace THtracker.Application.DTOs.TaskLists;

public record TaskListResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Color,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
