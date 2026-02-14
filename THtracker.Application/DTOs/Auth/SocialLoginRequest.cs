namespace THtracker.Application.DTOs.Auth;

public record SocialLoginRequest(
    string Provider, // "Google", "Facebook"
    string Token, // Token received from the social provider on the frontend
    string DeviceInfo
);
