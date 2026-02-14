using THtracker.Application.DTOs.Categories;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Categories;

public class GetAllCategoriesUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryResponse>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllByUserAsync(userId, cancellationToken);
        return categories.Select(c => new CategoryResponse(c.Id, c.UserId, c.Name));
    }
}
