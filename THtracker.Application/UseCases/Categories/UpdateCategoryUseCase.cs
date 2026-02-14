using THtracker.Application.DTOs.Categories;
using THtracker.Application.Validators.Categories;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Categories;

public class UpdateCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponse?> ExecuteAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var validator = new UpdateCategoryRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Datos inválidos: {errors}");
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category == null)
            return null;

        category.UpdateName(request.Name);
        await _categoryRepository.UpdateAsync(category, cancellationToken);

        return new CategoryResponse(category.Id, category.UserId, category.Name);
    }
}
