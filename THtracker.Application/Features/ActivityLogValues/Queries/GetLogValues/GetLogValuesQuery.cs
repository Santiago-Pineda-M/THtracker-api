using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityLogValues.Queries.GetLogValues;

public sealed record GetLogValuesQuery(Guid ActivityLogId, Guid UserId) : IRequest<Result<IEnumerable<LogValueResponse>>>;
