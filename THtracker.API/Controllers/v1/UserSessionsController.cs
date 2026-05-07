using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Common;
using THtracker.Application.Interfaces;
using THtracker.Application.Features.UserSessions;
using THtracker.Application.Features.UserSessions.Queries.GetUserSessions;
using THtracker.Application.Features.UserSessions.Commands.RevokeSession;

namespace THtracker.API.Controllers.v1;

[Authorize]
[ApiController]
[Route("sessions")]
public sealed class UserSessionsController : AuthorizedControllerBase
{
    private readonly ISender _sender;

    public UserSessionsController(ICurrentUserService currentUser, ISender sender) : base(currentUser)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<UserSessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMySessions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetUserSessionsQuery(userId, pageNumber, pageSize), ct);
        return result.ToActionResult();
    }

    [HttpPost("{sessionId:guid}/revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new RevokeSessionCommand(sessionId, userId), ct);
        
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.ToActionResult();
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = GetUserId();
        var sessionId = GetSessionId();
        var result = await _sender.Send(new RevokeSessionCommand(sessionId, userId), ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.ToActionResult();
    }
}
