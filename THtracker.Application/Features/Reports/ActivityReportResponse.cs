namespace THtracker.Application.Features.Reports;

public sealed record ActivityReportResponse(
    Guid UserId,
    DateTime StartDate,
    DateTime EndDate,
    double TotalDurationMinutes,
    IEnumerable<ActivityLogDetailResponse> Logs);

public sealed record ActivityLogDetailResponse(
    Guid LogId,
    DateTime StartedAt,
    DateTime? EndedAt,
    double DurationMinutes,
    ActivityInfoResponse Activity,
    CategoryInfoResponse Category,
    IEnumerable<LogValueInfoResponse> Values);

public sealed record ActivityInfoResponse(
    Guid Id,
    string Name,
    string Color);

public sealed record CategoryInfoResponse(
    Guid Id,
    string Name,
    string Color);

public sealed record LogValueInfoResponse(
    Guid DefinitionId,
    string Name,
    string Value,
    string? Unit,
    string ValueType);
