namespace THtracker.Application.Features.Auth;

public sealed record TokenResponse(
    string AccessToken, 
    string RefreshToken, 
    DateTime RefreshTokenExpiry);
