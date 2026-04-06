using FluentValidation;
using THtracker.Application.DTOs.ActivityValueDefinitions;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityValueDefinitions;

public class UpdateValueDefinitionUseCase
{
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateValueDefinitionRequest> _validator;

    public UpdateValueDefinitionUseCase(
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateValueDefinitionRequest> validator)
    {
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<ActivityValueDefinitionResponse>> ExecuteAsync(
        Guid userId,
        Guid activityId,
        Guid definitionId,
        UpdateValueDefinitionRequest request,
        CancellationToken cancellationToken = default)
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

        var definition = await _definitionRepository.GetByIdAsync(definitionId, cancellationToken);
        if (definition == null || definition.ActivityId != activityId)
            return Result.Failure<ActivityValueDefinitionResponse>(new Error("NotFound", "La definición no existe."));

        var currentName = request.Name ?? definition.Name;
        var currentValueType = request.ValueType ?? definition.ValueType;
        var currentIsRequired = request.IsRequired ?? definition.IsRequired;
        var currentUnit = request.Unit ?? definition.Unit;
        var currentMinValue = request.MinValue ?? definition.MinValue;
        var currentMaxValue = request.MaxValue ?? definition.MaxValue;

        definition.Update(
            currentName,
            currentValueType,
            currentIsRequired,
            currentUnit,
            currentMinValue,
            currentMaxValue
        );

        await _definitionRepository.UpdateAsync(definition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivityValueDefinitionResponse(
            definition.Id,
            definition.ActivityId,
            definition.Name,
            definition.ValueType,
            definition.IsRequired,
            definition.Unit,
            definition.MinValue,
            definition.MaxValue);
    }
}