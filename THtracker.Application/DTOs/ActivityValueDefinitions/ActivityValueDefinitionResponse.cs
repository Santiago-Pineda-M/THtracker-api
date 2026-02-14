namespace THtracker.Application.DTOs.ActivityValueDefinitions;

public record ActivityValueDefinitionResponse(
    Guid Id,
    Guid ActivityId,
    string Name,
    string ValueType,
    bool IsRequired,
    string? Unit,
    string? MinValue,
    string? MaxValue
);
