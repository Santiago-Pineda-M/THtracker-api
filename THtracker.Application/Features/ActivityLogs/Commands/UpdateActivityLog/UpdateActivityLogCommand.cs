using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityLogs.Commands.UpdateActivityLog;

public sealed record UpdateActivityLogCommand(
    Guid Id,
    DateTime StartedAt,
    DateTime? EndedAt,
    Guid UserId = default) : IRequest<Result<ActivityLogResponse>>;
