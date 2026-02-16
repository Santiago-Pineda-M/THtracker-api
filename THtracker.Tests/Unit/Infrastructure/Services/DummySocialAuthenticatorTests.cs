using FluentAssertions;
using THtracker.Infrastructure.Services;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class DummySocialAuthenticatorTests
{
    private readonly DummySocialAuthenticator _authenticator;

    public DummySocialAuthenticatorTests()
    {
        _authenticator = new DummySocialAuthenticator();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnSocialUserProfile()
    {
        // Arrange
        var provider = "Google";
        var token = "dummy-google-token";

        // Act
        var result = await _authenticator.AuthenticateAsync(provider, token);

        // Assert
        result.Should().NotBeNull();
        result.Provider.Should().Be(provider);
        result.ProviderKey.Should().Be("dummy_key");
        result.Email.Should().Be("social_user@example.com");
        result.Name.Should().Be("Social User");
    }

    [Theory]
    [InlineData("Google", "google-token-123")]
    [InlineData("Facebook", "facebook-token-456")]
    [InlineData("Twitter", "twitter-token-789")]
    public async Task AuthenticateAsync_ShouldReturnSameProfile_ForAnyProvider(string provider, string token)
    {
        // Act
        var result = await _authenticator.AuthenticateAsync(provider, token);

        // Assert
        result.Should().NotBeNull();
        result.Provider.Should().Be(provider);
        result.ProviderKey.Should().Be("dummy_key");
        result.Email.Should().Be("social_user@example.com");
        result.Name.Should().Be("Social User");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldNotThrowException()
    {
        // Arrange
        var provider = "TestProvider";
        var token = "test-token";

        // Act
        Func<Task> act = async () => await _authenticator.AuthenticateAsync(provider, token);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
