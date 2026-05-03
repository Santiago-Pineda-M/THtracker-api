using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityLogs.Queries.GetActiveActivityLogs;

public sealed record GetActiveActivityLogsQuery(Guid UserId) : IRequest<Result<IEnumerable<ActivityLogResponse>>>;
