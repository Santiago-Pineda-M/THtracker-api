using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityValueDefinitions.Commands.CreateValueDefinition;

public sealed record CreateValueDefinitionCommand(
    Guid ActivityId,
    string Name,
    string ValueType,
    bool IsRequired,
    string? Unit,
    string? MinValue,
    string? MaxValue,
    Guid UserId = default) : IRequest<Result<ActivityValueDefinitionResponse>>;
