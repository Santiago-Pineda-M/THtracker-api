namespace THtracker.Application.DTOs.Tasks;

public record TaskItemResponse(
    Guid Id,
    Guid TaskListId,
    Guid UserId,
    string Content,
    bool IsCompleted,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
