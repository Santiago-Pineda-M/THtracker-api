using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityValueDefinitions.Commands.UpdateValueDefinition;

public sealed record UpdateValueDefinitionCommand(
    Guid ActivityId,
    Guid DefinitionId,
    string Name,
    string ValueType,
    bool IsRequired,
    string? Unit,
    string? MinValue,
    string? MaxValue,
    Guid UserId = default) : IRequest<Result<ActivityValueDefinitionResponse>>;
