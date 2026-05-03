using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Reports.Queries.GetActivityReport;

public sealed record GetActivityReportQuery(
    DateTime StartDate,
    DateTime EndDate,
    List<Guid>? CategoryIds = null,
    List<Guid>? ActivityIds = null,
    double? MinDurationMinutes = null,
    string? SearchTerm = null,
    bool? OnlyCompleted = null,
    Guid UserId = default) : IRequest<Result<ActivityReportResponse>>;
