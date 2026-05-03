using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Comprobación detallada del estado del servicio.
/// </summary>
[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Devuelve un reporte detallado del estado de salud de los componentes del sistema.
    /// </summary>
    /// <response code="200">Todos los componentes están saludables.</response>
    /// <response code="503">Al menos un componente crítico falló.</response>
    [HttpGet("details")]
    [ProducesResponseType(typeof(HealthDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthDetailsResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDetails(CancellationToken ct)
    {
        var report = await _healthCheckService.CheckHealthAsync(ct);

        var response = new HealthDetailsResponse(
            Status: report.Status.ToString(),
            TotalDuration: report.TotalDuration.TotalMilliseconds,
            Entries: report.Entries.Select(e => new HealthEntryResponse(
                Component: e.Key,
                Status: e.Value.Status.ToString(),
                Description: e.Value.Description,
                Duration: e.Value.Duration.TotalMilliseconds
            )),
            Timestamp: DateTime.UtcNow
        );

        return report.Status == HealthStatus.Healthy 
            ? Ok(response) 
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}

public sealed record HealthDetailsResponse(
    string Status, 
    double TotalDuration, 
    IEnumerable<HealthEntryResponse> Entries, 
    DateTime Timestamp);

public sealed record HealthEntryResponse(
    string Component, 
    string Status, 
    string? Description, 
    double Duration);
