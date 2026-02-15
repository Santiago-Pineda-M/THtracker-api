using Microsoft.AspNetCore.Mvc;
using THtracker.API.DTOs;
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
    /// <response code="200">Usuario registrado con éxito.</response>
    /// <response code="400">Error de validación o el email ya existe.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var userId = await _registerUser.ExecuteAsync(request, cancellationToken);
            return Ok(new { UserId = userId });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Inicia sesión y devuelve access token y refresh token.
    /// </summary>
    /// <param name="request">Email, contraseña e información del dispositivo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tokens de acceso y refresco con fecha de expiración.</returns>
    /// <response code="200">Autenticación exitosa.</response>
    /// <response code="401">Credenciales inválidas.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _loginUser.ExecuteAsync(request, ipAddress, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex) when (ex.Message.Contains("Invalid Credentials", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Renueva el access token usando un refresh token válido.
    /// </summary>
    /// <param name="request">Objeto con el refresh token.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Nuevos access token y refresh token.</returns>
    /// <response code="200">Tokens renovados con éxito.</response>
    /// <response code="400">Refresh token inválido o expirado.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var deviceInfo = Request.Headers["User-Agent"].ToString();
            var response = await _refreshToken.ExecuteAsync(
                request.RefreshToken,
                ipAddress,
                deviceInfo,
                cancellationToken
            );
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Autenticación mediante proveedores sociales (Google, Facebook, etc.).
    /// </summary>
    /// <param name="request">Token o datos del proveedor social.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tokens de acceso y refresco.</returns>
    /// <response code="200">Login social exitoso.</response>
    /// <response code="400">Error en la autenticación social.</response>
    [HttpPost("social-login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SocialLogin(
        [FromBody] SocialLoginRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _socialLogin.ExecuteAsync(request, ipAddress, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }
}
