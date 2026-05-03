using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.UserSessions.Commands.RevokeSession;

public sealed record RevokeSessionCommand(Guid SessionId, Guid UserId) : IRequest<Result>;
