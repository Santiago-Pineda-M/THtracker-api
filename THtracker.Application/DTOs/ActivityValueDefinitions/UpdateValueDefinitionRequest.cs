namespace THtracker.Application.DTOs.ActivityValueDefinitions;

public record UpdateValueDefinitionRequest(
    string? Name = null,
    string? ValueType = null,
    bool? IsRequired = null,
    string? Unit = null,
    string? MinValue = null,
    string? MaxValue = null
);