namespace THtracker.Domain.Entities;

public class UserSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string SessionToken { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }

    private UserSession() { }

    public UserSession(Guid userId, string sessionToken, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        SessionToken = sessionToken;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IsActive = true;
    }

    public void Revoke()
    {
        IsActive = false;
    }
}
