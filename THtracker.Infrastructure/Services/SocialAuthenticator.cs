using THtracker.Application.Interfaces;

namespace THtracker.Infrastructure.Services;

public class DummySocialAuthenticator : ISocialAuthenticator
{
    public Task<SocialUserProfile> AuthenticateAsync(string provider, string token)
    {
        // This is a dummy implementation. 
        // In a real app, you would use HttpClient to call Google/Facebook APIs.
        
        return Task.FromResult(new SocialUserProfile(
            provider,
            "dummy_key",
            "social_user@example.com",
            "Social User"
        ));
    }
}
