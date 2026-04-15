namespace THtracker.Application.DTOs.Tasks;

public record CreateTaskItemRequest(Guid TaskListId, string Content, DateTime? DueDate = null);
