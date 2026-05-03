using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityValueDefinitions.Commands.DeleteValueDefinition;

public sealed record DeleteValueDefinitionCommand(Guid ActivityId, Guid DefinitionId, Guid UserId) : IRequest<Result<Unit>>;
