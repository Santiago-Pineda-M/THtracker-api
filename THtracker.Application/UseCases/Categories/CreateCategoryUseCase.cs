using THtracker.Application.DTOs.Categories;
using THtracker.Application.Validators.Categories;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Categories;

public class CreateCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponse> ExecuteAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var validator = new CreateCategoryRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Datos inválidos: {errors}");
        }

        var category = new Category(userId, request.Name);
        await _categoryRepository.AddAsync(category, cancellationToken);

        return new CategoryResponse(category.Id, category.UserId, category.Name);
    }
}
