using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Activities.Queries.GetActivityById;

public sealed record GetActivityByIdQuery(Guid Id, Guid UserId) : IRequest<Result<ActivityResponse>>;
