using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Roles.Queries.GetAllRoles;

public sealed record GetAllRolesQuery() : IRequest<Result<IEnumerable<RoleResponse>>>;
