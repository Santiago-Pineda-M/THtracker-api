using FluentValidation;
using THtracker.Application.DTOs.ActivityValueDefinitions;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityValueDefinitions;

public class CreateValueDefinitionUseCase
{
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateValueDefinitionRequest> _validator;

    public CreateValueDefinitionUseCase(
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateValueDefinitionRequest> validator)
    {
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<ActivityValueDefinitionResponse>> ExecuteAsync(Guid userId, Guid activityId, CreateValueDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<ActivityValueDefinitionResponse>(new Error("Validation", errors));
        }

        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null)
            return Result.Failure<ActivityValueDefinitionResponse>(new Error("NotFound", "La actividad no existe."));

        if (activity.UserId != userId)
            return Result.Failure<ActivityValueDefinitionResponse>(new Error("Forbidden", "No tienes acceso a esta actividad."));

        var definition = new ActivityValueDefinition(
            activityId,
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
