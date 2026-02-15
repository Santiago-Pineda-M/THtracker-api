using THtracker.Application.DTOs.Categories;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Categories;

public class GetCategoryByIdUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryResponse>> ExecuteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
            return Result.Failure<CategoryResponse>(new Error("NotFound", "La categoría no existe."));

        if (category.UserId != userId)
            return Result.Failure<CategoryResponse>(new Error("Forbidden", "No tienes acceso a esta categoría."));

        return new CategoryResponse(category.Id, category.UserId, category.Name);
    }
}
