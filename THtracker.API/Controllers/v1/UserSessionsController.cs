using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.DTOs.Sessions;
using THtracker.Application.UseCases.Sessions;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Gestión de sesiones de usuario: consultar y revocar sesiones activas.
/// </summary>
[ApiController]
[Route("sessions")]
public class UserSessionsController : AuthorizedControllerBase
{
    private readonly GetUserSessionsUseCase _getSessions;
    private readonly RevokeSessionUseCase _revokeSession;

    public UserSessionsController(
        GetUserSessionsUseCase getSessions,
        RevokeSessionUseCase revokeSession
    )
    {
        _getSessions = getSessions;
        _revokeSession = revokeSession;
    }

    /// <summary>
    /// Obtiene todas las sesiones activas del usuario autenticado.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de sesiones activas con información de dispositivo y ubicación.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserSessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMySessions(
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var sessions = await _getSessions.ExecuteAsync(userId, cancellationToken);
        return Ok(sessions);
    }

    /// <summary>
    /// Revoca una sesión específica del usuario autenticado.
    /// </summary>
    /// <param name="sessionId">ID de la sesión a revocar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    [HttpPost("{sessionId}/revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(
        Guid sessionId,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var result = await _revokeSession.ExecuteAsync(userId, sessionId, cancellationToken);
        return result.ToActionResult();
    }
}
