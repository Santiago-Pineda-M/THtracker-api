using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Roles;

public class DeleteRoleUseCase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleUseCase(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var deleted = await _roleRepository.DeleteAsync(roleId, cancellationToken);
        if (!deleted)
        {
            return Result.Failure(new Error("NotFound", "El rol no existe."));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
