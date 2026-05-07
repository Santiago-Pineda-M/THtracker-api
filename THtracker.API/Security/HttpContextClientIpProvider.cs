using THtracker.Application.Interfaces;

namespace THtracker.API.Security;

public sealed class HttpContextClientIpProvider(IHttpContextAccessor httpContextAccessor) : IClientIpProvider
{
    public string GetClientIpAddress()
    {
        var remote = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
        return remote?.ToString() ?? "unknown";
    }
}
