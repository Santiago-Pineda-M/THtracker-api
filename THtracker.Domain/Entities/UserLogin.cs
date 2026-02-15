namespace THtracker.Domain.Entities;

public class UserLogin
{
    public Guid Id { get; private set; }
    public string LoginProvider { get; private set; } = null!; // e.g., "Google", "Facebook"
    public string ProviderKey { get; private set; } = null!; // User ID from the provider
    public string? ProviderDisplayName { get; private set; }
    public Guid UserId { get; private set; }

    private UserLogin() { }

    public UserLogin(
        string loginProvider,
        string providerKey,
        string? providerDisplayName,
        Guid userId
    )
    {
        Id = Guid.NewGuid();
        LoginProvider = loginProvider;
        ProviderKey = providerKey;
        ProviderDisplayName = providerDisplayName;
        UserId = userId;
    }
}
