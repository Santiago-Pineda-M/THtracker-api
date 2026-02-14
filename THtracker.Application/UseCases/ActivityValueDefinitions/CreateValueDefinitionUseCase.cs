using THtracker.Application.DTOs.ActivityValueDefinitions;
using THtracker.Application.Validators.ActivityValueDefinitions;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityValueDefinitions;

public class CreateValueDefinitionUseCase
{
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;

    public CreateValueDefinitionUseCase(
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository)
    {
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
    }

    public async Task<ActivityValueDefinitionResponse> ExecuteAsync(Guid userId, Guid activityId, CreateValueDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var validator = new CreateValueDefinitionRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Datos inválidos: {errors}");
        }

        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null)
            throw new Exception("La actividad no existe.");

        if (activity.UserId != userId)
            throw new Exception("No tienes acceso a esta actividad.");

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
