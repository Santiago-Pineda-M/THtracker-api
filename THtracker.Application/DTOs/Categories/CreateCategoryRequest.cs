namespace THtracker.Application.DTOs.Categories;

public record CreateCategoryRequest(
    string Name,
    string Color = "#FFFFFF"
);
