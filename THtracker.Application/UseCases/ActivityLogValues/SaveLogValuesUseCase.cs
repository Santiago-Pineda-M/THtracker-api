using THtracker.Application.DTOs.ActivityLogValues;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.ActivityLogValues;

public class SaveLogValuesUseCase
{
    private readonly IActivityLogValueRepository _logValueRepository;
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityValueDefinitionRepository _definitionRepository;
    private readonly IActivityRepository _activityRepository;

    public SaveLogValuesUseCase(
        IActivityLogValueRepository logValueRepository,
        IActivityLogRepository logRepository,
        IActivityValueDefinitionRepository definitionRepository,
        IActivityRepository activityRepository)
    {
        _logValueRepository = logValueRepository;
        _logRepository = logRepository;
        _definitionRepository = definitionRepository;
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<LogValueResponse>> ExecuteAsync(Guid userId, Guid logId, IEnumerable<LogValueRequest> values, CancellationToken cancellationToken = default)
    {
        var log = await _logRepository.GetByIdAsync(logId, cancellationToken);
        if (log == null) throw new Exception("El registro de actividad no existe.");

        var activity = await _activityRepository.GetByIdAsync(log.ActivityId, cancellationToken);
        if (activity == null || activity.UserId != userId) throw new Exception("No tienes acceso a este registro.");

        var definitions = await _definitionRepository.GetAllByActivityAsync(log.ActivityId, cancellationToken);
        var responses = new List<LogValueResponse>();

        foreach (var valReq in values)
        {
            var def = definitions.FirstOrDefault(d => d.Id == valReq.ValueDefinitionId);
            if (def == null) throw new Exception($"La definición de valor {valReq.ValueDefinitionId} no pertenece a esta actividad.");

            ValidateValueType(def, valReq.Value);

            var logValue = new ActivityLogValue(logId, valReq.ValueDefinitionId, valReq.Value);
            await _logValueRepository.AddAsync(logValue, cancellationToken);

            responses.Add(new LogValueResponse(logValue.Id, logValue.ActivityLogId, logValue.ValueDefinitionId, logValue.Value));
        }

        return responses;
    }

    private void ValidateValueType(ActivityValueDefinition def, string value)
    {
        switch (def.ValueType)
        {
            case "Number":
                if (!double.TryParse(value, out var num))
                    throw new Exception($"El valor para '{def.Name}' debe ser un número válido.");
                
                if (!string.IsNullOrEmpty(def.MinValue) && double.TryParse(def.MinValue, out var min) && num < min)
                    throw new Exception($"El valor para '{def.Name}' no puede ser menor a {def.MinValue}.");
                
                if (!string.IsNullOrEmpty(def.MaxValue) && double.TryParse(def.MaxValue, out var max) && num > max)
                    throw new Exception($"El valor para '{def.Name}' no puede ser mayor a {def.MaxValue}.");
                break;

            case "Boolean":
                var lowerVal = value.ToLower();
                if (lowerVal != "true" && lowerVal != "false" && lowerVal != "1" && lowerVal != "0")
                    throw new Exception($"El valor para '{def.Name}' debe ser un booleano (true/false o 1/0).");
                break;

            case "Time":
                if (!TimeSpan.TryParse(value, out _))
                    throw new Exception($"El valor para '{def.Name}' debe ser un formato de tiempo válido (HH:mm:ss).");
                break;
        }
    }
}
