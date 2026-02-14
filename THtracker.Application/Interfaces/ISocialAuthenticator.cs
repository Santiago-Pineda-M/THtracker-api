namespace THtracker.Application.Interfaces;

public record SocialUserProfile(
    string Provider,
    string ProviderKey,
    string Email,
    string Name
);

public interface ISocialAuthenticator
{
    Task<SocialUserProfile> AuthenticateAsync(string provider, string token);
}
