using MediatR;
using THtracker.Application.Common;
using THtracker.Application.Features.Roles;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.UserRoles.Queries.GetUserRoles;

public sealed class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, Result<PaginatedResponse<RoleResponse>>>
{
    private readonly IUserRoleRepository _userRoleRepository;

    public GetUserRolesQueryHandler(IUserRoleRepository userRoleRepository)
    {
        _userRoleRepository = userRoleRepository;
    }

    public async Task<Result<PaginatedResponse<RoleResponse>>> Handle(
        GetUserRolesQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var page = await _userRoleRepository.GetRolesPageByUserAsync(
            request.UserId,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = page.Items
            .Select(r => new RoleResponse(r.Id, r.Name))
            .ToList();

        return Result.Success(new PaginatedResponse<RoleResponse>(
            items,
            page.TotalCount,
            pageNumber,
            pageSize));
    }
}
