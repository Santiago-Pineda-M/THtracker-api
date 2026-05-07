using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Features.Auth;
using THtracker.Application.Features.Auth.Login;
using THtracker.Application.Features.Auth.Register;
using THtracker.Application.Features.Auth.RefreshToken;
using THtracker.Application.Features.Users.Queries.GetUserById;

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
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(UsersController.GetById),
                nameof(UsersController).Replace("Controller", string.Empty),
                new { id = result.Value.Id },
                result.Value);
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
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Renueva el access token usando un refresh token válido.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] SubmitRefreshToken body,
        CancellationToken cancellationToken)
    {
        var deviceInfo = Request.Headers["User-Agent"].ToString() ?? "unknown";

        var result = await _sender.Send(
            new RefreshTokenCommand(body.RefreshToken, deviceInfo),
            cancellationToken);
        return result.ToActionResult();
    }
}
