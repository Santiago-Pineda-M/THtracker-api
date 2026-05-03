using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Activities.Queries.GetAllActivities;

public sealed record GetAllActivitiesQuery(Guid UserId) : IRequest<IEnumerable<ActivityResponse>>;
