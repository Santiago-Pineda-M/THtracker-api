namespace THtracker.Application.Features.ActivityValueDefinitions;

public sealed record ActivityValueDefinitionResponse(
    Guid Id,
    Guid ActivityId,
    string Name,
    string ValueType,
    bool IsRequired,
    string? Unit,
    string? MinValue,
    string? MaxValue);
