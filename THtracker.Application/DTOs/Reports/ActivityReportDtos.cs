using THtracker.Application.DTOs.ActivityLogValues;

namespace THtracker.Application.DTOs.Reports;

public record ActivityReportRequest(
    DateTime StartDate,
    DateTime EndDate
);

public record ActivityReportResponse(
    Guid UserId,
    DateTime StartDate,
    DateTime EndDate,
    double TotalDurationMinutes,
    IEnumerable<ActivityGroupReportDto> Activities
);

public record ActivityGroupReportDto(
    Guid ActivityId,
    string ActivityName,
    double TotalDurationMinutes,
    IEnumerable<LogReportDto> Logs
);

public record LogReportDto(
    Guid LogId,
    DateTime StartedAt,
    DateTime? EndedAt,
    double DurationMinutes,
    IEnumerable<LogValueReportDto> Values
);

public record LogValueReportDto(
    Guid DefinitionId,
    string Name,
    string Value,
    string? Unit
);
