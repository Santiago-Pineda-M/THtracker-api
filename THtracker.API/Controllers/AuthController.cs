using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.DTOs.Auth;
using THtracker.Application.UseCases.Auth;

namespace THtracker.API.Controllers;

/// <summary>
/// Autenticación: registro, login, refresh token y login social.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserUseCase _registerUser;
    private readonly LoginUserUseCase _loginUser;
    private readonly RefreshTokenUseCase _refreshToken;
    private readonly SocialLoginUseCase _socialLogin;

    public AuthController(
        RegisterUserUseCase registerUser,
        LoginUserUseCase loginUser,
        RefreshTokenUseCase refreshToken,
        SocialLoginUseCase socialLogin
    )
    {
        _registerUser = registerUser;
        _loginUser = loginUser;
        _refreshToken = refreshToken;
        _socialLogin = socialLogin;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="request">Datos del registro (nombre, email, contraseña).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>ID del usuario creado.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _registerUser.ExecuteAsync(request, cancellationToken);
        
        if (result.IsSuccess)
        {
            return Ok(new { UserId = result.Value });
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Inicia sesión y devuelve access token y refresh token.
    /// </summary>
    /// <param name="request">Email, contraseña e información del dispositivo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tokens de acceso y refresco con fecha de expiración.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _loginUser.ExecuteAsync(request, ipAddress, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Renueva el access token usando un refresh token válido.
    /// </summary>
    /// <param name="request">Objeto con el refresh token.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Nuevos access token y refresh token.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken
    )
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var deviceInfo = Request.Headers["User-Agent"].ToString();
        var result = await _refreshToken.ExecuteAsync(
            request.RefreshToken,
            ipAddress,
            deviceInfo,
            cancellationToken
        );
        return result.ToActionResult();
    }

    /// <summary>
    /// Autenticación mediante proveedores sociales (Google, Facebook, etc.).
    /// </summary>
    /// <param name="request">Token o datos del proveedor social.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tokens de acceso y refresco.</returns>
    [HttpPost("social-login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SocialLogin(
        [FromBody] SocialLoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _socialLogin.ExecuteAsync(request, ipAddress, cancellationToken);
        return result.ToActionResult();
    }
}
