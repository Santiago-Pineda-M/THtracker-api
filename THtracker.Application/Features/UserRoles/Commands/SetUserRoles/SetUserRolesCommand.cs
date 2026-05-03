using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.UserRoles.Commands.SetUserRoles;

public sealed record SetUserRolesCommand(Guid UserId, List<string> RoleNames) : IRequest<Result>;
