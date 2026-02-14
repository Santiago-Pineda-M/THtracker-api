namespace THtracker.Application.DTOs.Activities;

public record UpdateActivityRequest(
    string Name, 
    bool AllowOverlap
);
