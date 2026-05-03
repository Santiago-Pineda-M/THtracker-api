using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityLogs.Queries.GetActivityLogById;

public sealed record GetActivityLogByIdQuery(Guid Id, Guid UserId) : IRequest<Result<ActivityLogResponse>>;
