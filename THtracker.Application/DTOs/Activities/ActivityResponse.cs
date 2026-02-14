namespace THtracker.Application.DTOs.Activities;

public record ActivityResponse(
    Guid Id, 
    Guid UserId, 
    Guid CategoryId, 
    string Name, 
    bool AllowOverlap
);
