namespace THtracker.Application.DTOs.Sessions;

public record UserSessionResponse(
    Guid Id,
    string DeviceInfo,
    string IpAddress,
    string? Location,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsActive
);
