using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Activities;

public class DeleteActivityUseCase
{
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteActivityUseCase(IActivityRepository activityRepository, IUnitOfWork unitOfWork)
    {
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);
        if (activity == null)
            return Result.Failure(new Error("NotFound", "La actividad no existe."));

        if (activity.UserId != userId)
            return Result.Failure(new Error("Forbidden", "No tienes acceso a esta actividad."));

        await _activityRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
