using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Interfaces;
using THtracker.Application.Features.Reports;
using THtracker.Application.Features.Reports.Queries.GetActivityReport;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Reportes detallados de actividades.
/// </summary>
[Authorize]
[ApiController]
[Route("reports")]
public sealed class ReportsController : AuthorizedControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ICurrentUserService currentUser, ISender sender) : base(currentUser)
    {
        _sender = sender;
    }

    /// <summary>
    /// Genera un reporte detallado de registros de actividades con anidamiento completo (Categorías, Valores, etc).
    /// </summary>
    /// <param name="query">Filtros de rango de fechas, categoría y actividad.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpGet("activities")]
    [ProducesResponseType(typeof(ActivityReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActivityReport([FromQuery] GetActivityReportQuery query, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(query with { UserId = userId }, ct);
        return result.ToActionResult();
    }
}
