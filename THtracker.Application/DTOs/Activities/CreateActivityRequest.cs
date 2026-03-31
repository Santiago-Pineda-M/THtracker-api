namespace THtracker.Application.DTOs.Activities;

public record CreateActivityRequest(
    Guid CategoryId, 
    string Name,
    string Color = "#FFFFFF",
    bool AllowOverlap = false
);
