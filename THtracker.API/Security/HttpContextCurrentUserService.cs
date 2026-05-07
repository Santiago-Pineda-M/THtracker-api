using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using THtracker.Application.Interfaces;

namespace THtracker.API.Security;

public sealed class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid GetUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User ID claim not found.");
        return userId;
    }

    public Guid GetSessionId()
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirstValue("sid");
        if (claim == null || !Guid.TryParse(claim, out var sessionId))
            throw new UnauthorizedAccessException("Session ID claim not found.");
        return sessionId;
    }
}
