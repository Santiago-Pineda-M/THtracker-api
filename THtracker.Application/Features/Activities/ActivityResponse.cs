namespace THtracker.Application.Features.Activities;

public sealed record ActivityResponse(
    Guid Id, 
    Guid UserId, 
    Guid CategoryId, 
    string Name, 
    string Color, 
    bool AllowOverlap);
