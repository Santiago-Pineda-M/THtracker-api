using THtracker.Application.DTOs.Activities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Activities;

public class GetActivityByIdUseCase
{
    private readonly IActivityRepository _activityRepository;

    public GetActivityByIdUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<ActivityResponse?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);
        if (activity == null)
            return null;

        return new ActivityResponse(
            activity.Id, 
            activity.UserId, 
            activity.CategoryId, 
            activity.Name, 
            activity.AllowOverlap
        );
    }
}
