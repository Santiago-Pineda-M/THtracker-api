using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Reports.Queries.GetActivityReport;

public sealed class GetActivityReportQueryHandler : IRequestHandler<GetActivityReportQuery, Result<ActivityReportResponse>>
{
    private readonly IActivityLogRepository _logRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetActivityReportQueryHandler(
        IActivityLogRepository logRepository,
        IActivityRepository activityRepository,
        ICategoryRepository categoryRepository)
    {
        _logRepository = logRepository;
        _activityRepository = activityRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<ActivityReportResponse>> Handle(GetActivityReportQuery request, CancellationToken cancellationToken)
    {
        // 1. Validar rango de fechas
        if (request.StartDate > request.EndDate)
        {
            return Result.Failure<ActivityReportResponse>(new Error("Validation", "La fecha de inicio no puede ser posterior a la de fin."));
        }

        // 2. Obtener logs filtrados directamente desde el repositorio (Optimización SQL)
        // El repositorio ya hace los Joins e Includes necesarios para evitar N+1 y aplicar filtros múltiples.
        var logs = await _logRepository.GetReportLogsAsync(
            request.UserId,
            request.StartDate,
            request.EndDate,
            request.CategoryIds,
            request.ActivityIds,
            request.SearchTerm,
            request.OnlyCompleted,
            cancellationToken
        );

        // 3. Cargar datos maestros para mapeo de nombres de categorías/actividades (Eager loading ya aplicado en logs, pero para Actividad/Categoría Info)
        var userActivities = (await _activityRepository.GetAllByUserAsync(request.UserId, cancellationToken)).ToDictionary(a => a.Id);
        var userCategories = (await _categoryRepository.GetAllByUserAsync(request.UserId, cancellationToken)).ToDictionary(c => c.Id);

        // 4. Mapeo anidado completo y filtro de duración mínima
        var logDetails = logs.Select(log => {
            var activity = userActivities[log.ActivityId];
            var category = userCategories[activity.CategoryId];
            var duration = log.GetDurationInInterval(request.StartDate, request.EndDate).TotalMinutes;

            var values = log.LogValues.Select(v => new LogValueInfoResponse(
                v.ValueDefinitionId,
                v.ValueDefinition.Name,
                v.Value,
                v.ValueDefinition.Unit,
                v.ValueDefinition.ValueType
            )).ToList();

            return new ActivityLogDetailResponse(
                log.Id,
                log.StartedAt,
                log.EndedAt,
                duration,
                new ActivityInfoResponse(activity.Id, activity.Name, activity.Color),
                new CategoryInfoResponse(category.Id, category.Name, category.Color),
                values
            );
        })
        .Where(l => !request.MinDurationMinutes.HasValue || l.DurationMinutes >= request.MinDurationMinutes.Value)
        .ToList();

        var totalDuration = logDetails.Sum(l => l.DurationMinutes);

        return Result.Success(new ActivityReportResponse(
            request.UserId,
            request.StartDate,
            request.EndDate,
            totalDuration,
            logDetails
        ));
    }
}
