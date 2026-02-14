using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Categories;

public class DeleteCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public DeleteCategoryUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.DeleteAsync(id, cancellationToken);
    }
}
