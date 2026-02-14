using THtracker.Application.DTOs.Activities;
using THtracker.Application.Validators.Activities;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Activities;

public class CreateActivityUseCase
{
    private readonly IActivityRepository _activityRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateActivityUseCase(IActivityRepository activityRepository, ICategoryRepository categoryRepository)
    {
        _activityRepository = activityRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ActivityResponse> ExecuteAsync(Guid userId, CreateActivityRequest request, CancellationToken cancellationToken = default)
    {
        var validator = new CreateActivityRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Datos inválidos: {errors}");
        }

        // Validate category
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
            throw new Exception("La categoría especificada no existe.");
        
        if (category.UserId != userId)
            throw new Exception("No tienes acceso a esta categoría.");

        var activity = new Activity(userId, request.CategoryId, request.Name, request.AllowOverlap);
        await _activityRepository.AddAsync(activity, cancellationToken);

        return new ActivityResponse(
            activity.Id, 
            activity.UserId, 
            activity.CategoryId, 
            activity.Name, 
            activity.AllowOverlap
        );
    }
}
