using THtracker.Application.DTOs.Activities;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Activities;

public class GetActivityByIdUseCase
{
    private readonly IActivityRepository _activityRepository;

    public GetActivityByIdUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<Result<ActivityResponse>> ExecuteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);
        if (activity == null)
            return Result.Failure<ActivityResponse>(new Error("NotFound", "La actividad no existe."));

        if (activity.UserId != userId)
            return Result.Failure<ActivityResponse>(new Error("Forbidden", "No tienes acceso a esta actividad."));

        return new ActivityResponse(
            activity.Id, 
            activity.UserId, 
            activity.CategoryId, 
            activity.Name, 
            activity.AllowOverlap
        );
    }
}
