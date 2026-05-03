using MediatR;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Activities.Queries.GetAllActivities;

public sealed class GetAllActivitiesQueryHandler : IRequestHandler<GetAllActivitiesQuery, IEnumerable<ActivityResponse>>
{
    private readonly IActivityRepository _activityRepository;

    public GetAllActivitiesQueryHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<ActivityResponse>> Handle(GetAllActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = await _activityRepository.GetAllByUserAsync(request.UserId, cancellationToken);
        
        return activities.Select(a => new ActivityResponse(
                a.Id, 
                a.UserId, 
                a.CategoryId, 
                a.Name, 
                a.Color, 
                a.AllowOverlap));
    }
}
