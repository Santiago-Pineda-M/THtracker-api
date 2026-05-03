using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.UserRoles.Commands.SetUserRoles;

public sealed class SetUserRolesCommandHandler : IRequestHandler<SetUserRolesCommand, Result>
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetUserRolesCommandHandler(
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SetUserRolesCommand request, CancellationToken cancellationToken)
    {
        // 1. Obtener roles actuales
        var currentRoles = await _userRoleRepository.GetRolesByUserAsync(request.UserId, cancellationToken);
        
        // 2. Remover todos los roles actuales
        foreach (var r in currentRoles)
        {
            await _userRoleRepository.RemoveRoleFromUserAsync(request.UserId, r.Id, cancellationToken);
        }

        // 3. Agregar nuevos roles
        if (request.RoleNames is { Count: > 0 })
        {
            foreach (var roleName in request.RoleNames)
            {
                var role = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
                if (role == null)
                {
                    return Result.Failure(new Error("NotFound", $"El rol '{roleName}' no existe."));
                }

                await _userRoleRepository.AddRoleToUserAsync(request.UserId, role.Id, cancellationToken);
            }
        }

        // 4. Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
