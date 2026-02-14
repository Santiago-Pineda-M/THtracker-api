using Microsoft.AspNetCore.Mvc;
using THtracker.Application.DTOs.Auth;
using THtracker.Application.UseCases.Auth;

namespace THtracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="request">Datos del registro (nombre, email, password).</param>
    /// <param name="useCase">Caso de uso para el registro.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El ID del usuario creado.</returns>
    /// <response code="200">Usuario registrado con éxito.</response>
    /// <response code="400">Error en la validación o el email ya existe.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        [FromServices] RegisterUserUseCase useCase,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var userId = await useCase.ExecuteAsync(request, cancellationToken);
            return Ok(new { UserId = userId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Inicia sesión de un usuario y devuelve tokens de acceso.
    /// </summary>
    /// <param name="request">Credenciales de acceso.</param>
    /// <param name="useCase">Caso de uso para login.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Autenticación exitosa, devuelve JWT y Refresh Token.</response>
    /// <response code="401">Credenciales inválidas.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] LoginUserUseCase useCase,
        CancellationToken cancellationToken
    )
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var response = await useCase.ExecuteAsync(request, ipAddress, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Renueva el token de acceso usando un Refresh Token.
    /// </summary>
    /// <param name="refreshToken">Token de refresco válido.</param>
    /// <param name="useCase">Caso de uso para refrescar token.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Tokens renovados con éxito.</response>
    /// <response code="400">Token de refresco inválido o expirado.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh(
        [FromBody] string refreshToken,
        [FromServices] RefreshTokenUseCase useCase,
        CancellationToken cancellationToken
    )
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var deviceInfo = Request.Headers["User-Agent"].ToString();
        var response = await useCase.ExecuteAsync(
            refreshToken,
            ipAddress,
            deviceInfo,
            cancellationToken
        );
        return Ok(response);
    }

    /// <summary>
    /// Autenticación mediante proveedores sociales (por definir).
    /// </summary>
    [HttpPost("social-login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SocialLogin(
        [FromBody] SocialLoginRequest request,
        [FromServices] SocialLoginUseCase useCase,
        CancellationToken cancellationToken
    )
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var response = await useCase.ExecuteAsync(request, ipAddress, cancellationToken);
        return Ok(response);
    }
}
