namespace THtracker.Application.Features.ActivityLogValues;

public sealed record LogValueResponse(
    Guid Id,
    Guid ActivityLogId,
    Guid ValueDefinitionId,
    string Value);
