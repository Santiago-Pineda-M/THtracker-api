using THtracker.Application.DTOs.Activities;
using THtracker.Application.Validators.Activities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Activities;

public class UpdateActivityUseCase
{
    private readonly IActivityRepository _activityRepository;

    public UpdateActivityUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<ActivityResponse?> ExecuteAsync(Guid activityId, UpdateActivityRequest request, CancellationToken cancellationToken = default)
    {
        var validator = new UpdateActivityRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Datos inválidos: {errors}");
        }

        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null)
            return null;

        activity.Update(request.Name, request.AllowOverlap);
        await _activityRepository.UpdateAsync(activity, cancellationToken);

        return new ActivityResponse(
            activity.Id, 
            activity.UserId, 
            activity.CategoryId, 
            activity.Name, 
            activity.AllowOverlap
        );
    }
}
