namespace THtracker.Application.DTOs.ActivityLogValues;

public record LogValueRequest(
    Guid ValueDefinitionId,
    string Value
);

public record LogValueResponse(
    Guid Id,
    Guid ActivityLogId,
    Guid ValueDefinitionId,
    string Value
);
