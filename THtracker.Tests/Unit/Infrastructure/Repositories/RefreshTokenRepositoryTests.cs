using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Repositories;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class RefreshTokenRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly RefreshTokenRepository _repository;

    public RefreshTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new RefreshTokenRepository(_context);
    }

    [Fact]
    public async Task GetByTokenAsync_ShouldReturnToken_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = new RefreshToken(
            "test-token-123",
            DateTime.UtcNow.AddDays(7),
            "192.168.1.1",
            "Chrome on Windows",
            userId
        );
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTokenAsync("test-token-123");

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("test-token-123");
        result.UserId.Should().Be(userId);
        result.CreatedByIp.Should().Be("192.168.1.1");
        result.DeviceInfo.Should().Be("Chrome on Windows");
    }

    [Fact]
    public async Task GetByTokenAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByTokenAsync("non-existent-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = new RefreshToken(
            "new-token-456",
            DateTime.UtcNow.AddDays(30),
            "10.0.0.1",
            "Firefox on Linux",
            userId
        );

        // Act
        await _repository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Assert
        var tokenInDb = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "new-token-456");
        tokenInDb.Should().NotBeNull();
        tokenInDb!.UserId.Should().Be(userId);
        tokenInDb.CreatedByIp.Should().Be("10.0.0.1");
        tokenInDb.DeviceInfo.Should().Be("Firefox on Linux");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = new RefreshToken(
            "update-token",
            DateTime.UtcNow.AddDays(7),
            "192.168.1.1",
            "Safari on Mac",
            userId
        );
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();

        // Act
        token.Revoke("192.168.1.2", "User logged out");
        await _repository.UpdateAsync(token);
        await _context.SaveChangesAsync();

        // Assert
        var tokenInDb = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "update-token");
        tokenInDb.Should().NotBeNull();
        tokenInDb!.IsRevoked.Should().BeTrue();
        tokenInDb.RevokedByIp.Should().Be("192.168.1.2");
        tokenInDb.ReasonRevoked.Should().Be("User logged out");
    }

    [Fact]
    public async Task RevokeAllForUserAsync_ShouldRevokeAllActiveTokens_ForSpecificUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var token1 = new RefreshToken(
            "token-1",
            DateTime.UtcNow.AddDays(7),
            "192.168.1.1",
            "Device 1",
            userId
        );
        var token2 = new RefreshToken(
            "token-2",
            DateTime.UtcNow.AddDays(7),
            "192.168.1.2",
            "Device 2",
            userId
        );
        var token3 = new RefreshToken(
            "token-3",
            DateTime.UtcNow.AddDays(7),
            "192.168.1.3",
            "Device 3",
            otherUserId
        );
        var expiredToken = new RefreshToken(
            "expired-token",
            DateTime.UtcNow.AddDays(-1),
            "192.168.1.4",
            "Device 4",
            userId
        );

        _context.RefreshTokens.AddRange(token1, token2, token3, expiredToken);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RevokeAllForUserAsync(userId, "192.168.1.100", "Security measure");
        await _context.SaveChangesAsync();

        // Assert
        var token1InDb = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "token-1");
        var token2InDb = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "token-2");
        var token3InDb = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "token-3");
        var expiredTokenInDb = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "expired-token");

        token1InDb!.IsRevoked.Should().BeTrue();
        token1InDb.RevokedByIp.Should().Be("192.168.1.100");
        token1InDb.ReasonRevoked.Should().Be("Security measure");

        token2InDb!.IsRevoked.Should().BeTrue();
        token2InDb.RevokedByIp.Should().Be("192.168.1.100");
        token2InDb.ReasonRevoked.Should().Be("Security measure");

        token3InDb!.IsRevoked.Should().BeFalse(); // Different user
        expiredTokenInDb!.IsRevoked.Should().BeFalse(); // Already expired
    }

    [Fact]
    public async Task RevokeAllForUserAsync_ShouldNotRevokeAlreadyRevokedTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = new RefreshToken(
            "already-revoked",
            DateTime.UtcNow.AddDays(7),
            "192.168.1.1",
            "Device",
            userId
        );
        token.Revoke("192.168.1.2", "First revocation");
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RevokeAllForUserAsync(userId, "192.168.1.3", "Second revocation");
        await _context.SaveChangesAsync();

        // Assert
        var tokenInDb = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "already-revoked");
        tokenInDb!.RevokedByIp.Should().Be("192.168.1.2"); // Should keep original revocation
        tokenInDb.ReasonRevoked.Should().Be("First revocation");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
