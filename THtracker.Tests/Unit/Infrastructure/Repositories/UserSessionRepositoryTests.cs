using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Repositories;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class UserSessionRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserSessionRepository _repository;

    public UserSessionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new UserSessionRepository(_context);
    }

    [Fact]
    public async Task GetAllByUserAsync_ShouldReturnSessions_ForSpecificUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var session1 = new UserSession(userId, "token-1", DateTime.UtcNow.AddHours(1), "Chrome on Windows", "192.168.1.1");
        var session2 = new UserSession(userId, "token-2", DateTime.UtcNow.AddHours(2), "Firefox on Linux", "192.168.1.2");
        var sessionOther = new UserSession(otherUserId, "token-3", DateTime.UtcNow.AddHours(1), "Safari on Mac", "192.168.1.3");

        _context.UserSessions.AddRange(session1, session2, sessionOther);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByUserAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.SessionToken == "token-1");
        result.Should().Contain(s => s.SessionToken == "token-2");
        result.Should().NotContain(s => s.SessionToken == "token-3");
    }

    [Fact]
    public async Task GetAllByUserAsync_ShouldReturnEmptyList_WhenUserHasNoSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetAllByUserAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByTokenAsync_ShouldReturnSession_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = new UserSession(userId, "unique-token-123", DateTime.UtcNow.AddDays(1), "Chrome on Windows", "192.168.1.1");
        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTokenAsync("unique-token-123");

        // Assert
        result.Should().NotBeNull();
        result!.SessionToken.Should().Be("unique-token-123");
        result.UserId.Should().Be(userId);
        result.IsActive.Should().BeTrue();
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
    public async Task AddAsync_ShouldAddSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = new UserSession(userId, "new-session-token", DateTime.UtcNow.AddHours(24), "Chrome on Windows", "192.168.1.1");

        // Act
        await _repository.AddAsync(session);
        await _context.SaveChangesAsync();

        // Assert
        var sessionInDb = await _context.UserSessions.FindAsync(session.Id);
        sessionInDb.Should().NotBeNull();
        sessionInDb!.SessionToken.Should().Be("new-session-token");
        sessionInDb.UserId.Should().Be(userId);
        sessionInDb.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = new UserSession(userId, "session-to-update", DateTime.UtcNow.AddHours(1), "Chrome on Windows", "192.168.1.1");
        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        session.Revoke();
        await _repository.UpdateAsync(session);
        await _context.SaveChangesAsync();

        // Assert
        var sessionInDb = await _context.UserSessions.FindAsync(session.Id);
        sessionInDb.Should().NotBeNull();
        sessionInDb!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteSession_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = new UserSession(userId, "session-to-delete", DateTime.UtcNow.AddHours(1), "Chrome on Windows", "192.168.1.1");
        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(session.Id);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var sessionInDb = await _context.UserSessions.FindAsync(session.Id);
        sessionInDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
