using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Roles.Commands.CreateRole;

public sealed record CreateRoleCommand(string Name) : IRequest<Result<RoleResponse>>;
