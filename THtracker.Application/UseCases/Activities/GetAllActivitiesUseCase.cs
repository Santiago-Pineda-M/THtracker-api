using THtracker.Application.DTOs.Activities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Activities;

public class GetAllActivitiesUseCase
{
    private readonly IActivityRepository _activityRepository;

    public GetAllActivitiesUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<ActivityResponse>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var activities = await _activityRepository.GetAllByUserAsync(userId, cancellationToken);
        return activities.Select(a => new ActivityResponse(
            a.Id, 
            a.UserId, 
            a.CategoryId, 
            a.Name, 
            a.AllowOverlap
        ));
    }
}
