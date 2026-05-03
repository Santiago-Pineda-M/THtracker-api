using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Roles.Queries.GetRoleById;

public sealed record GetRoleByIdQuery(Guid Id) : IRequest<Result<RoleResponse>>;
