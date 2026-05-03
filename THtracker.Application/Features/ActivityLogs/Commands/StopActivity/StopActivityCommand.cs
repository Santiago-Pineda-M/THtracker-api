using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityLogs.Commands.StopActivity;

public sealed record StopActivityCommand(
    Guid Id, 
    Guid UserId) : IRequest<Result<ActivityLogResponse>>;
