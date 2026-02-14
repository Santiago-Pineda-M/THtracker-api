namespace THtracker.Application.DTOs.Auth;

public record LoginRequest(
    string Email,
    string Password,
    string DeviceInfo // "Chrome on Windows", "iPhone 13", etc.
);
