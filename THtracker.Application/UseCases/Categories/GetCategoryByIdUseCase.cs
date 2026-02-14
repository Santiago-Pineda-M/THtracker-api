using THtracker.Application.DTOs.Categories;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Categories;

public class GetCategoryByIdUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponse?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
            return null;

        return new CategoryResponse(category.Id, category.UserId, category.Name);
    }
}
