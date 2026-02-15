namespace THtracker.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiryDate { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public string CreatedByIp { get; private set; } = null!;
    public string DeviceInfo { get; private set; } = null!; // e.g., "Chrome on Windows"
    
    public DateTime? RevokedDate { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReasonRevoked { get; private set; }
    
    public Guid UserId { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsRevoked => RevokedDate != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public RefreshToken(string token, DateTime expiryDate, string createdByIp, string deviceInfo, Guid userId)
    {
        Id = Guid.NewGuid();
        Token = token;
        ExpiryDate = expiryDate;
        CreatedDate = DateTime.UtcNow;
        CreatedByIp = createdByIp;
        DeviceInfo = deviceInfo;
        UserId = userId;
    }

    public void Revoke(string ipAddress, string reason)
    {
        RevokedDate = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        ReasonRevoked = reason;
    }
}
