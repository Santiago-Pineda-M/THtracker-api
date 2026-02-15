using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.DTOs.Reports;
using THtracker.Application.UseCases.Reports;

namespace THtracker.API.Controllers;

/// <summary>
/// Reportes y estadísticas de actividad.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/reports")]
public class ReportsController : AuthorizedControllerBase
{
    private readonly GetUserActivityReportUseCase _getReport;

    public ReportsController(GetUserActivityReportUseCase getReport)
    {
        _getReport = getReport;
    }

    /// <summary>
    /// Genera un reporte completo de actividades para el usuario en un periodo.
    /// </summary>
    /// <param name="startDate">Fecha de inicio del periodo.</param>
    /// <param name="endDate">Fecha de fin del periodo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("activities")]
    [ProducesResponseType(typeof(ActivityReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActivityReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var request = new ActivityReportRequest(startDate, endDate);
        var result = await _getReport.ExecuteAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }
}
