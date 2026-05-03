using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityLogs.Queries.GetActivityLogs;

public sealed record GetActivityLogsQuery(
    Guid UserId, 
    Guid? ActivityId = null, 
    DateTime? From = null, 
    DateTime? To = null) : IRequest<Result<IEnumerable<ActivityLogResponse>>>;
