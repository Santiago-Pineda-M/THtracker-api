using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Tasks.Commands.ToggleTaskCompletion;

public sealed record ToggleTaskCompletionCommand(Guid Id, Guid UserId) : IRequest<Result<Unit>>;
