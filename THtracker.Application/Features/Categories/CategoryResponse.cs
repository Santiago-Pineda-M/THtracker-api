namespace THtracker.Application.Features.Categories;

public sealed record CategoryResponse(
    Guid Id, 
    Guid UserId, 
    string Name, 
    string? Description, 
    string Color);
