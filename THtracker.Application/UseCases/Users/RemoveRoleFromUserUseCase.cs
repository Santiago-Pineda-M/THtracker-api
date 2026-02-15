using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class RemoveRoleFromUserUseCase
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleFromUserUseCase(IUserRoleRepository userRoleRepository, IUnitOfWork unitOfWork)
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
        await _userRoleRepository.RemoveRoleFromUserAsync(userId, roleId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
