namespace THtracker.Application.DTOs.Auth;

/// <summary>
/// Solicitud para renovar el access token usando un refresh token.
/// </summary>
/// <param name="RefreshToken">Token de refresco válido.</param>
public record RefreshTokenRequest(string RefreshToken);
