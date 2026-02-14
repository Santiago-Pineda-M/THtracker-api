namespace THtracker.Application.DTOs.Auth;

public record TokenResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiry);
