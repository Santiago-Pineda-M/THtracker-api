namespace THtracker.Application.Features.Tasks;

public sealed record TaskResponse(
    Guid Id,
    Guid TaskListId,
    Guid UserId,
    string Content,
    bool IsCompleted,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
