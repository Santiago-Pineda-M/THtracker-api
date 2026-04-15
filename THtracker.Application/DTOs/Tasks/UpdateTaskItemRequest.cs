namespace THtracker.Application.DTOs.Tasks;

public record UpdateTaskItemRequest(string Content, DateTime? DueDate);
