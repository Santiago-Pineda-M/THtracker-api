using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.UserSessions.Queries.GetUserSessions;

public sealed record GetUserSessionsQuery(Guid UserId) : IRequest<Result<IEnumerable<UserSessionResponse>>>;
