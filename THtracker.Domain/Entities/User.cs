namespace THtracker.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string? SecurityStamp { get; private set; }

    private readonly List<Role> _roles = new();
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private readonly List<UserLogin> _logins = new();
    public IReadOnlyCollection<UserLogin> Logins => _logins.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public User(string name, string email)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public void Update(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdateSecurityStamp();
    }

    public void AddRole(Role role)
    {
        if (!_roles.Any(r => r.Id == role.Id))
        {
            _roles.Add(role);
        }
    }

    public void RemoveRole(Role role)
    {
        var existingRole = _roles.FirstOrDefault(r => r.Id == role.Id);
        if (existingRole != null)
        {
            _roles.Remove(existingRole);
        }
    }

    public void AddLogin(string provider, string key, string? displayName)
    {
        if (!_logins.Any(l => l.LoginProvider == provider && l.ProviderKey == key))
        {
            _logins.Add(new UserLogin(provider, key, displayName, Id));
        }
    }

    public void AddRefreshToken(string token, DateTime expiry, string ip, string device)
    {
        // Optional: Remove old/expired tokens to keep table size managed
        _refreshTokens.Add(new RefreshToken(token, expiry, ip, device, Id));
    }

    public void RevokeRefreshToken(string token, string ip, string reason)
    {
        var existingToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
        existingToken?.Revoke(ip, reason);
    }

    public void RevokeAllRefreshTokens(string ip, string reason)
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke(ip, reason);
        }
    }

    private void UpdateSecurityStamp()
    {
        SecurityStamp = Guid.NewGuid().ToString();
    }
}
