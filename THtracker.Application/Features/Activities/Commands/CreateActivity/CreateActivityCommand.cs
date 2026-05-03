using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Activities.Commands.CreateActivity;

public sealed record CreateActivityCommand(
    Guid CategoryId, 
    string Name, 
    string Color, 
    bool AllowOverlap,
    Guid UserId = default) : IRequest<Result<ActivityResponse>>;
