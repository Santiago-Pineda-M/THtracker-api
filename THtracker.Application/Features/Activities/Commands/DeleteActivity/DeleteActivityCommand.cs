using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Activities.Commands.DeleteActivity;

public sealed record DeleteActivityCommand(Guid Id, Guid UserId) : IRequest<Result<Unit>>;
