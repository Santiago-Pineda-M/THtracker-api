using THtracker.Application.DTOs.Reports;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Reports;

public class GetUserActivityReportUseCase
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;

    public GetUserActivityReportUseCase(
        IActivityLogRepository logRepository,
        IActivityRepository activityRepository)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Result<ActivityReportResponse>> ExecuteAsync(
        Guid userId,
        ActivityReportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.StartDate > request.EndDate)
        {
            return Result.Failure<ActivityReportResponse>(new Error("Validation", "La fecha de inicio no puede ser posterior a la de fin."));
        }

        var logs = await _logRepository.GetLogsAsync(userId, null, request.StartDate, request.EndDate, cancellationToken);
        
        // Necesitamos los nombres de las actividades. Como el repositorio de logs devuelve ActivityLog, 
        // y ese log está asociado a una actividad, pero no incluimos el objeto Activity en el Select del repo (solo el log).
        // Sin embargo, podemos mapearlo si incluimos Activity en el Include, o si lo buscamos después.
        // Vamos a optimizar el repo para traer el nombre de la actividad también.
        
        // Re-evaluando: El repo Join con Activities pero solo selecciona Log. 
        // Si queremos el nombre de la actividad, deberíamos traerlo.
        
        // Por ahora, vamos a agrupar los logs por ActivityId y buscar los nombres de las actividades involucradas.
        var activityIds = logs.Select(l => l.ActivityId).Distinct().ToList();
        var activities = new Dictionary<Guid, string>();
        
        foreach (var aid in activityIds)
        {
            var activity = await _activityRepository.GetByIdAsync(aid, cancellationToken);
            if (activity != null)
            {
                activities[aid] = activity.Name;
            }
        }

        var activityGroups = logs.GroupBy(l => l.ActivityId)
            .Select(group =>
            {
                var activityId = group.Key;
                var activityName = activities.ContainsKey(activityId) ? activities[activityId] : "Actividad desconocida";
                
                var logDtos = group.Select(log =>
                {
                    var duration = log.GetDurationInInterval(request.StartDate, request.EndDate).TotalMinutes;
                    var values = log.LogValues.Select(v => new LogValueReportDto(
                        v.ValueDefinitionId,
                        v.ValueDefinition.Name,
                        v.Value,
                        v.ValueDefinition.Unit
                    ));

                    return new LogReportDto(
                        log.Id,
                        log.StartedAt,
                        log.EndedAt,
                        duration,
                        values
                    );
                }).ToList();

                return new ActivityGroupReportDto(
                    activityId,
                    activityName,
                    logDtos.Sum(l => l.DurationMinutes),
                    logDtos
                );
            }).ToList();

        var totalDuration = activityGroups.Sum(a => a.TotalDurationMinutes);

        return Result.Success(new ActivityReportResponse(
            userId,
            request.StartDate,
            request.EndDate,
            totalDuration,
            activityGroups
        ));
    }
}
