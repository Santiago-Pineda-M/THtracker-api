using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Activities.Commands.UpdateActivity;

public sealed record UpdateActivityCommand(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Color,
    bool AllowOverlap,
    Guid UserId = default) : IRequest<Result<ActivityResponse>>;
