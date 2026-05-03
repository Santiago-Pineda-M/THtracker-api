using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.UserRoles.Commands.RemoveRoleFromUser;

public sealed record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId) : IRequest<Result>;
