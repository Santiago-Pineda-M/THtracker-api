using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Activities.Commands.UpdateActivity;

public sealed class UpdateActivityCommandHandler : IRequestHandler<UpdateActivityCommand, Result<ActivityResponse>>
{
    private readonly IActivityRepository _activityRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateActivityCommandHandler(
        IActivityRepository activityRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _activityRepository = activityRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivityResponse>> Handle(UpdateActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(request.Id, cancellationToken);
        if (activity == null || activity.UserId != request.UserId)
        {
            return Result.Failure<ActivityResponse>(new Error("NotFound", "La actividad no existe o no tienes acceso."));
        }

        // Validar que la nueva categoría pertenezca al usuario si está cambiando
        if (activity.CategoryId != request.CategoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null || category.UserId != request.UserId)
            {
                return Result.Failure<ActivityResponse>(new Error("Forbidden", "La categoría especificada no es válida o no tienes acceso."));
            }
        }

        activity.Update(request.CategoryId, request.Name, request.Color, request.AllowOverlap);
        
        await _activityRepository.UpdateAsync(activity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivityResponse(
            activity.Id,
            activity.UserId,
            activity.CategoryId,
            activity.Name,
            activity.Color,
            activity.AllowOverlap
        );
    }
}
