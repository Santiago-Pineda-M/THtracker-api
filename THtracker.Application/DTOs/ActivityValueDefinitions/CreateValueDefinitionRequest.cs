namespace THtracker.Application.DTOs.ActivityValueDefinitions;

public record CreateValueDefinitionRequest(
    string Name,
    string ValueType,
    bool IsRequired = false,
    string? Unit = null,
    string? MinValue = null,
    string? MaxValue = null
);
