using FluentValidation;
using THtracker.Application.DTOs.Activities;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Activities;

public class CreateActivityUseCase
{
    private readonly IActivityRepository _activityRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateActivityRequest> _validator;

    public CreateActivityUseCase(
        IActivityRepository activityRepository, 
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateActivityRequest> validator)
    {
        _activityRepository = activityRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<ActivityResponse>> ExecuteAsync(Guid userId, CreateActivityRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<ActivityResponse>(new Error("Validation", errors));
        }

        // Validate category
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
            return Result.Failure<ActivityResponse>(new Error("NotFound", "La categoría especificada no existe."));
        
        if (category.UserId != userId)
            return Result.Failure<ActivityResponse>(new Error("Forbidden", "No tienes acceso a esta categoría."));

        var activity = new Activity(userId, request.CategoryId, request.Name, request.AllowOverlap);
        
        await _activityRepository.AddAsync(activity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivityResponse(
            activity.Id, 
            activity.UserId, 
            activity.CategoryId, 
            activity.Name, 
            activity.AllowOverlap
        );
    }
}
