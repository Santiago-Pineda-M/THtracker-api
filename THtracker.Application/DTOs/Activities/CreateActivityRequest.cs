namespace THtracker.Application.DTOs.Activities;

public record CreateActivityRequest(
    Guid CategoryId, 
    string Name, 
    bool AllowOverlap = false
);
