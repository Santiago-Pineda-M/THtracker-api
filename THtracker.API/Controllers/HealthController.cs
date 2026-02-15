using Microsoft.AspNetCore.Mvc;

namespace THtracker.API.Controllers;

/// <summary>
/// Comprobación de estado del servicio (health check).
/// </summary>
[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Devuelve el estado actual del API.
    /// </summary>
    /// <returns>Estado, nombre del servicio y marca de tiempo.</returns>
    /// <response code="200">Servicio en ejecución.</response>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new HealthResponse(
            Status: "ok",
            Service: "THtracker API",
            Timestamp: DateTime.UtcNow
        ));
    }
}

/// <summary>
/// Respuesta del endpoint de health.
/// </summary>
public record HealthResponse(string Status, string Service, DateTime Timestamp);
