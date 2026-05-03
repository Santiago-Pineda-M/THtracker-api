namespace THtracker.Application.Features.UserSessions;

public sealed record UserSessionResponse(
    Guid Id,
    string? DeviceInfo,
    string? IpAddress,
    string? Location,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsActive);
