using FluentValidation;
using THtracker.Application.DTOs.Categories;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Categories;

public class UpdateCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateCategoryRequest> _validator;

    public UpdateCategoryUseCase(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateCategoryRequest> validator)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<CategoryResponse>> ExecuteAsync(Guid userId, Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<CategoryResponse>(new Error("Validation", errors));
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category == null)
            return Result.Failure<CategoryResponse>(new Error("NotFound", "La categoría no existe."));

        if (category.UserId != userId)
            return Result.Failure<CategoryResponse>(new Error("Forbidden", "No tienes acceso a esta categoría."));

        category.UpdateName(request.Name);
        
        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryResponse(category.Id, category.UserId, category.Name);
    }
}
