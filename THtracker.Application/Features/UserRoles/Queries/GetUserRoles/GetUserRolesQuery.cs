using MediatR;
using THtracker.Application.Common;
using THtracker.Application.Features.Roles;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.UserRoles.Queries.GetUserRoles;

public sealed record GetUserRolesQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PaginatedResponse<RoleResponse>>>;
