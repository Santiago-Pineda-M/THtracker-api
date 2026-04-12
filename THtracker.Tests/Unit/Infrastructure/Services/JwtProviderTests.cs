using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Services;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class JwtProviderTests
{
    private readonly JwtProvider _jwtProvider;
    private readonly IConfiguration _configuration;

    public JwtProviderTests()
    {
        var configurationData = new Dictionary<string, string>
        {
            { "Jwt:SecretKey", "ThisIsAVerySecretKeyForTestingPurposesOnly12345" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:AccessTokenExpirationMinutes", "60" },
            { "Jwt:RefreshTokenExpirationDays", "7" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        _jwtProvider = new JwtProvider(_configuration);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = new User("Test User", "test@example.com");

        var sessionId = Guid.NewGuid();

        // Act
        var token = _jwtProvider.GenerateAccessToken(user, sessionId);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        jwtToken.Should().NotBeNull();
        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeUserClaims()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com");

        var sessionId = Guid.NewGuid();

        // Act
        var token = _jwtProvider.GenerateAccessToken(user, sessionId);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        var sidClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sid");

        subClaim.Should().NotBeNull();
        subClaim!.Value.Should().Be(user.Id.ToString());
        
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be("john@example.com");
        
        jtiClaim.Should().NotBeNull();
        jtiClaim!.Value.Should().NotBeNullOrEmpty();

        sidClaim.Should().NotBeNull();
        sidClaim!.Value.Should().Be(sessionId.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ShouldHaveCorrectExpiration()
    {
        // Arrange
        var user = new User("Test User", "test@example.com");
        var beforeGeneration = DateTime.UtcNow;
        var sessionId = Guid.NewGuid();

        // Act
        var token = _jwtProvider.GenerateAccessToken(user, sessionId);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var expectedExpiration = beforeGeneration.AddMinutes(60);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnValidRefreshToken()
    {
        // Arrange
        var user = new User("Test User", "test@example.com");
        var ipAddress = "192.168.1.1";
        var deviceInfo = "Chrome on Windows";

        // Act
        var refreshToken = _jwtProvider.GenerateRefreshToken(user, ipAddress, deviceInfo);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().NotBeNullOrEmpty();
        refreshToken.UserId.Should().Be(user.Id);
        refreshToken.CreatedByIp.Should().Be(ipAddress);
        refreshToken.DeviceInfo.Should().Be(deviceInfo);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldHaveCorrectExpiration()
    {
        // Arrange
        var user = new User("Test User", "test@example.com");
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var refreshToken = _jwtProvider.GenerateRefreshToken(user, "127.0.0.1", "Test Device");

        // Assert
        var expectedExpiration = beforeGeneration.AddDays(7);
        refreshToken.ExpiryDate.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateUniqueTokens()
    {
        // Arrange
        var user = new User("Test User", "test@example.com");

        // Act
        var token1 = _jwtProvider.GenerateRefreshToken(user, "192.168.1.1", "Device 1");
        var token2 = _jwtProvider.GenerateRefreshToken(user, "192.168.1.2", "Device 2");

        // Assert
        token1.Token.Should().NotBe(token2.Token);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldNotBeRevoked()
    {
        // Arrange
        var user = new User("Test User", "test@example.com");

        // Act
        var refreshToken = _jwtProvider.GenerateRefreshToken(user, "192.168.1.1", "Device");

        // Assert
        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.IsActive.Should().BeTrue();
    }
}
