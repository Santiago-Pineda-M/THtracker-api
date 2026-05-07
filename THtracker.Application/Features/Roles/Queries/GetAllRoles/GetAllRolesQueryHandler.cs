using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Roles.Queries.GetAllRoles;

public sealed class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<PaginatedResponse<RoleResponse>>>
{
    private readonly IRoleRepository _roleRepository;

    public GetAllRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<PaginatedResponse<RoleResponse>>> Handle(
        GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var page = await _roleRepository.GetPageAsync(pageNumber, pageSize, cancellationToken);

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
