namespace THtracker.Application.DTOs.Activities;

public record UpdateActivityRequest(
    string Name,
    string Color = "#FFFFFF",
    bool AllowOverlap = false
);
