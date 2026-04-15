using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace THtracker.API.Controllers;

/// <summary>
/// Base para controladores que requieren usuario autenticado.
/// Proporciona acceso al ID del usuario desde el JWT.
/// </summary>
public abstract class AuthorizedControllerBase : ControllerBase
{
    /// <summary>
    /// Obtiene el ID del usuario autenticado desde el claim NameIdentifier.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Cuando no existe el claim o no es un Guid válido.</exception>
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID claim not found.");
        }
        return userId;
    }

    protected Guid GetSessionId()
    {
        var sessionIdClaim = User.FindFirstValue("sid");
        if (sessionIdClaim == null || !Guid.TryParse(sessionIdClaim, out var sessionId))
        {
            throw new UnauthorizedAccessException("Session ID claim not found.");
        }
        return sessionId;
    }
}
