using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Features.Auth;
using THtracker.Application.Features.Auth.Login;
using THtracker.Application.Features.Auth.Register;
using THtracker.Application.Features.Auth.RefreshToken;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Autenticación: registro, login, refresh token y login social.
/// </summary>
[ApiController]
[Route("auth")]
[Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        
        if (result.IsSuccess)
        {
            return Ok(new { UserId = result.Value });
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Inicia sesión y devuelve access token y refresh token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Inyectamos la IP en el comando antes de enviarlo
        var result = await _sender.Send(command with { IpAddress = ipAddress }, cancellationToken);
        
        return result.ToActionResult();
    }

    /// <summary>
    /// Renueva el access token usando un refresh token válido.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] string refreshToken,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var deviceInfo = Request.Headers["User-Agent"].ToString() ?? "unknown";

        var result = await _sender.Send(new RefreshTokenCommand(
            refreshToken, 
            ipAddress, 
            deviceInfo), cancellationToken);
        
        return result.ToActionResult();
    }
}
