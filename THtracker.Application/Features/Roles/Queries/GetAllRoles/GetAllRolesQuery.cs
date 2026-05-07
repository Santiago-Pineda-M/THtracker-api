using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Roles.Queries.GetAllRoles;

public sealed record GetAllRolesQuery(
    int PageNumber = 1,
    int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PaginatedResponse<RoleResponse>>>;
