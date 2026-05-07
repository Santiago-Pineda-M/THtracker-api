using Microsoft.AspNetCore.Hosting;

namespace THtracker.API.Middlewares;

public sealed class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        if (!environment.IsDevelopment())
        {
            context.Response.Headers.Append(
                "Content-Security-Policy",
                "default-src 'none'; frame-ancestors 'none'; base-uri 'none'");
        }

        return next(context);
    }
}
