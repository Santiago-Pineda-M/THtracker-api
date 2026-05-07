using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using THtracker.Application.Constants;

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
    /// Devuelve el estado general del servicio sin detalles.
    /// </summary>
    /// <response code="200">Servicio saludable.</response>
    /// <response code="503">Servicio degradado o no saludable.</response>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var report = await _healthCheckService.CheckHealthAsync(ct);
        var response = new HealthStatusResponse(report.Status.ToString(), DateTime.UtcNow);

        return report.Status == HealthStatus.Healthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    /// <summary>
    /// Devuelve un reporte detallado del estado de salud de los componentes del sistema.
    /// </summary>
    /// <response code="200">Todos los componentes están saludables.</response>
    /// <response code="503">Al menos un componente crítico falló.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
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

public sealed record HealthStatusResponse(string Status, DateTime Timestamp);

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
