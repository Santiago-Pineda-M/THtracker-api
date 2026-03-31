using FluentValidation;
using THtracker.Application.DTOs.Activities;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Activities;

public class UpdateActivityUseCase
{
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateActivityRequest> _validator;

    public UpdateActivityUseCase(
        IActivityRepository activityRepository, 
        IUnitOfWork unitOfWork,
        IValidator<UpdateActivityRequest> validator)
    {
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<ActivityResponse>> ExecuteAsync(Guid userId, Guid activityId, UpdateActivityRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<ActivityResponse>(new Error("Validation", errors));
        }

        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null)
            return Result.Failure<ActivityResponse>(new Error("NotFound", "La actividad no existe."));

        if (activity.UserId != userId)
            return Result.Failure<ActivityResponse>(new Error("Forbidden", "No tienes acceso a esta actividad."));

        activity.Update(request.Name, request.Color, request.AllowOverlap);
        
        await _activityRepository.UpdateAsync(activity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivityResponse(
            activity.Id, 
            activity.UserId, 
            activity.CategoryId, 
            activity.Name, 
            activity.Color,
            activity.AllowOverlap
        );
    }
}
