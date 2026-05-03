using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.ActivityValueDefinitions.Commands.CreateValueDefinition;

public sealed class CreateValueDefinitionCommandHandler : IRequestHandler<CreateValueDefinitionCommand, Result<ActivityValueDefinitionResponse>>
{
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateValueDefinitionCommandHandler(
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork)
    {
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivityValueDefinitionResponse>> Handle(CreateValueDefinitionCommand request, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(request.ActivityId, cancellationToken);
        
        if (activity == null || activity.UserId != request.UserId)
        {
            return Result.Failure<ActivityValueDefinitionResponse>(new Error("NotFound", "La actividad no existe o no tienes acceso."));
        }

        var definition = new ActivityValueDefinition(
            request.ActivityId,
            request.Name,
            request.ValueType,
            request.IsRequired,
            request.Unit,
            request.MinValue,
            request.MaxValue
        );

        await _definitionRepository.AddAsync(definition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivityValueDefinitionResponse(
            definition.Id,
            definition.ActivityId,
            definition.Name,
            definition.ValueType,
            definition.IsRequired,
            definition.Unit,
            definition.MinValue,
            definition.MaxValue
        );
    }
}
