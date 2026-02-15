using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class AddRoleToUserUseCase
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddRoleToUserUseCase(IUserRoleRepository userRoleRepository, IUnitOfWork unitOfWork)
    {
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        if (await _userRoleRepository.IsRoleAssignedAsync(userId, roleId, cancellationToken))
        {
            return Result.Failure(new Error("Conflict", "El rol ya está asignado a este usuario."));
        }

        await _userRoleRepository.AddRoleToUserAsync(userId, roleId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
