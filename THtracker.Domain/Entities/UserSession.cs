namespace THtracker.Domain.Entities;

public class UserSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string SessionToken { get; private set; } = null!;
    public string DeviceInfo { get; private set; } = null!; // "Chrome on Windows"
    public string IpAddress { get; private set; } = null!; // "192.168.1.1"
    public string? Location { get; private set; } // "Bogotá, Colombia"
    public string? UserAgent { get; private set; } // "Mozilla/5.0..."
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsActive { get; private set; }

    private UserSession() { }

    public UserSession(Guid userId, string sessionToken, DateTime expiresAt, string deviceInfo, string ipAddress, string? location = null, string? userAgent = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        SessionToken = sessionToken;
        DeviceInfo = deviceInfo;
        IpAddress = ipAddress;
        Location = location;
        UserAgent = userAgent;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IsActive = true;
    }

    public void Revoke()
    {
        IsActive = false;
        RevokedAt = DateTime.UtcNow;
    }

    public void Refresh(string newToken, DateTime newExpiresAt, string ipAddress, string deviceInfo)
    {
        SessionToken = newToken;
        ExpiresAt = newExpiresAt;
        IpAddress = ipAddress;
        DeviceInfo = deviceInfo;
    }
}
