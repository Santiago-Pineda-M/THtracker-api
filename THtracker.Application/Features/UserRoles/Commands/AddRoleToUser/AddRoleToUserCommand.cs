using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.UserRoles.Commands.AddRoleToUser;

public sealed record AddRoleToUserCommand(Guid UserId, Guid RoleId) : IRequest<Result>;
