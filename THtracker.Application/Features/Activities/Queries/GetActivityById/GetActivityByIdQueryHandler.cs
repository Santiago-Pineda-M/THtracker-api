using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Activities.Queries.GetActivityById;

public sealed class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, Result<ActivityResponse>>
{
    private readonly IActivityRepository _activityRepository;

    public GetActivityByIdQueryHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<Result<ActivityResponse>> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (activity == null || activity.UserId != request.UserId)
        {
            return Result.Failure<ActivityResponse>(new Error("NotFound", "La actividad no existe o no tienes acceso."));
        }

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
