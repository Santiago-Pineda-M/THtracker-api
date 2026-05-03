using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityLogs.Commands.StartActivity;

public sealed record StartActivityCommand(
    Guid ActivityId, 
    Guid UserId = default) : IRequest<Result<ActivityLogResponse>>;
