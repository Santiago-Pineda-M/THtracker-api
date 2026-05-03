using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;
using THtracker.Application.Features.Roles;

namespace THtracker.Application.Features.UserRoles.Queries.GetUserRoles;

public sealed class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, Result<IEnumerable<RoleResponse>>>
{
    private readonly IUserRoleRepository _userRoleRepository;

    public GetUserRolesQueryHandler(IUserRoleRepository userRoleRepository)
    {
        _userRoleRepository = userRoleRepository;
    }

    public async Task<Result<IEnumerable<RoleResponse>>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _userRoleRepository.GetRolesByUserAsync(request.UserId, cancellationToken);
        
        var response = roles.Select(r => new RoleResponse(r.Id, r.Name));

        return Result.Success(response);
    }
}
