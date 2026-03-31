namespace THtracker.Application.DTOs.Categories;

public record UpdateCategoryRequest(
    string Name,
    string Color = "#FFFFFF"
);
