using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Activities.Commands.CreateActivity;

public sealed class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, Result<ActivityResponse>>
{
    private readonly IActivityRepository _activityRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateActivityCommandHandler(
        IActivityRepository activityRepository, 
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _activityRepository = activityRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivityResponse>> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result.Failure<ActivityResponse>(new Error("NotFound", "La categoría especificada no existe."));
        }
        
        if (category.UserId != request.UserId)
        {
            return Result.Failure<ActivityResponse>(new Error("Forbidden", "No tienes acceso a esta categoría."));
        }

        var activity = new Activity(request.UserId, request.CategoryId, request.Name, request.Color, request.AllowOverlap);
        
        await _activityRepository.AddAsync(activity, cancellationToken);
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
