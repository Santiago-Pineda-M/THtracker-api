using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.DTOs.Sessions;
using THtracker.Application.UseCases.Sessions;

namespace THtracker.API.Controllers.v1;

[ApiController]
[Route("sessions")]
public class UserSessionsController : AuthorizedControllerBase
{
    private readonly GetUserSessionsUseCase getSessions;
    private readonly RevokeSessionUseCase revokeSession;
    private readonly LogoutCurrentSessionUseCase logoutCurrentSession;

    public UserSessionsController(
        GetUserSessionsUseCase getSessions,
        RevokeSessionUseCase revokeSession,
        LogoutCurrentSessionUseCase logoutCurrentSession
    )
    {
        this.getSessions = getSessions;
        this.revokeSession = revokeSession;
        this.logoutCurrentSession = logoutCurrentSession;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserSessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMySessions(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var sessions = await this.getSessions.ExecuteAsync(userId, cancellationToken);
        return Ok(sessions);
    }

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
        var result = await this.revokeSession.ExecuteAsync(userId, sessionId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var sessionId = GetSessionId();
        var result = await this.logoutCurrentSession.ExecuteAsync(
            userId,
            sessionId,
            cancellationToken
        );

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.ToActionResult();
    }
}
