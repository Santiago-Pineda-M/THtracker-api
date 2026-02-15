using THtracker.Application.DTOs.ActivityLogValues;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogValues;

public class SaveLogValuesUseCase
{
    private readonly IActivityLogValueRepository _logValueRepository;
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveLogValuesUseCase(
        IActivityLogValueRepository logValueRepository,
        IActivityLogRepository logRepository,
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork)
    {
        _logValueRepository = logValueRepository;
        _logRepository = logRepository;
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<LogValueResponse>>> ExecuteAsync(Guid userId, Guid logId, IEnumerable<LogValueRequest> values, CancellationToken cancellationToken = default)
    {
        var log = await _logRepository.GetByIdAsync(logId, cancellationToken);
        if (log == null)
            return Result.Failure<IEnumerable<LogValueResponse>>(new Error("NotFound", "El registro de actividad no existe."));

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != userId)
            return Result.Failure<IEnumerable<LogValueResponse>>(new Error("Forbidden", "No tienes acceso a este registro."));

        var definitions = await _definitionRepository.GetAllByActivityAsync(log.ActivityId, cancellationToken);
        var responses = new List<LogValueResponse>();

        foreach (var valReq in values)
        {
            var def = definitions.FirstOrDefault(d => d.Id == valReq.ValueDefinitionId);
            if (def == null)
                return Result.Failure<IEnumerable<LogValueResponse>>(new Error("Validation", $"La definición de valor {valReq.ValueDefinitionId} no pertenece a esta actividad."));

            var validationResult = ValidateValueType(def, valReq.Value);
            if (validationResult.IsFailure)
            {
                return Result.Failure<IEnumerable<LogValueResponse>>(validationResult.Error);
            }

            var logValue = new ActivityLogValue(logId, valReq.ValueDefinitionId, valReq.Value);
            await _logValueRepository.AddAsync(logValue, cancellationToken);

            responses.Add(new LogValueResponse(logValue.Id, logValue.ActivityLogId, logValue.ValueDefinitionId, logValue.Value));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success<IEnumerable<LogValueResponse>>(responses);
    }

    private Result ValidateValueType(ActivityValueDefinition def, string value)
    {
        switch (def.ValueType)
        {
            case "Number":
                if (!double.TryParse(value, out var num))
                    return Result.Failure(new Error("Validation", $"El valor para '{def.Name}' debe ser un número válido."));
                
                if (!string.IsNullOrEmpty(def.MinValue) && double.TryParse(def.MinValue, out var min) && num < min)
                    return Result.Failure(new Error("Validation", $"El valor para '{def.Name}' no puede ser menor a {def.MinValue}."));
                
                if (!string.IsNullOrEmpty(def.MaxValue) && double.TryParse(def.MaxValue, out var max) && num > max)
                    return Result.Failure(new Error("Validation", $"El valor para '{def.Name}' no puede ser mayor a {def.MaxValue}."));
                break;

            case "Boolean":
                var lowerVal = value.ToLower();
                if (lowerVal != "true" && lowerVal != "false" && lowerVal != "1" && lowerVal != "0")
                    return Result.Failure(new Error("Validation", $"El valor para '{def.Name}' debe ser un booleano (true/false o 1/0)."));
                break;

            case "Time":
                if (!TimeSpan.TryParse(value, out _))
                    return Result.Failure(new Error("Validation", $"El valor para '{def.Name}' debe ser un formato de tiempo válido (HH:mm:ss)."));
                break;
        }

        return Result.Success();
    }
}
