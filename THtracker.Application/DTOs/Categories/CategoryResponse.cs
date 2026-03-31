namespace THtracker.Application.DTOs.Categories;

public record CategoryResponse(
    Guid Id, 
    Guid UserId, 
    string Color,
    string Name
);
