using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.UserRoles.Commands.AddRoleToUser;

public sealed class AddRoleToUserCommandHandler : IRequestHandler<AddRoleToUserCommand, Result>
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddRoleToUserCommandHandler(IUserRoleRepository userRoleRepository, IUnitOfWork unitOfWork)
    {
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddRoleToUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRoleRepository.IsRoleAssignedAsync(request.UserId, request.RoleId, cancellationToken))
        {
            return Result.Failure(new Error("Conflict", "El rol ya está asignado a este usuario."));
        }

        await _userRoleRepository.AddRoleToUserAsync(request.UserId, request.RoleId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
