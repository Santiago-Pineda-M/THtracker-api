using MediatR;
using THtracker.Domain.Common;
using THtracker.Application.Features.Roles;

namespace THtracker.Application.Features.UserRoles.Queries.GetUserRoles;

public sealed record GetUserRolesQuery(Guid UserId) : IRequest<Result<IEnumerable<RoleResponse>>>;
