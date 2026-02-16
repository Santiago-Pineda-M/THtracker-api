using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Repositories;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class UserRoleRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRoleRepository _repository;

    public UserRoleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new UserRoleRepository(_context);
    }

    [Fact]
    public async Task GetRolesByUserAsync_ShouldReturnRoles_ForSpecificUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var role1 = new Role("Admin");
        var role2 = new Role("User");
        var role3 = new Role("Moderator");

        _context.Roles.AddRange(role1, role2, role3);
        await _context.SaveChangesAsync();

        var userRole1 = new UserRole(userId, role1.Id);
        var userRole2 = new UserRole(userId, role2.Id);
        var userRoleOther = new UserRole(otherUserId, role3.Id);

        _context.UserRoles.AddRange(userRole1, userRole2, userRoleOther);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRolesByUserAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Admin");
        result.Should().Contain(r => r.Name == "User");
        result.Should().NotContain(r => r.Name == "Moderator");
    }

    [Fact]
    public async Task GetRolesByUserAsync_ShouldReturnEmptyList_WhenUserHasNoRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetRolesByUserAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ShouldReturnUsers_ForSpecificRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var otherRoleId = Guid.NewGuid();

        var user1 = new User("User 1", "user1@example.com");
        var user2 = new User("User 2", "user2@example.com");
        var user3 = new User("User 3", "user3@example.com");

        _context.Users.AddRange(user1, user2, user3);
        await _context.SaveChangesAsync();

        var userRole1 = new UserRole(user1.Id, roleId);
        var userRole2 = new UserRole(user2.Id, roleId);
        var userRoleOther = new UserRole(user3.Id, otherRoleId);

        _context.UserRoles.AddRange(userRole1, userRole2, userRoleOther);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUsersByRoleAsync(roleId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Name == "User 1");
        result.Should().Contain(u => u.Name == "User 2");
        result.Should().NotContain(u => u.Name == "User 3");
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ShouldReturnEmptyList_WhenRoleHasNoUsers()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        // Act
        var result = await _repository.GetUsersByRoleAsync(roleId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddRoleToUserAsync_ShouldAddRoleToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        await _repository.AddRoleToUserAsync(userId, roleId);
        await _context.SaveChangesAsync();

        // Assert
        var userRole = await _context.UserRoles.FirstOrDefaultAsync(
            ur => ur.UserId == userId && ur.RoleId == roleId
        );
        userRole.Should().NotBeNull();
        userRole!.UserId.Should().Be(userId);
        userRole.RoleId.Should().Be(roleId);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_ShouldRemoveRoleFromUser_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userRole = new UserRole(userId, roleId);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveRoleFromUserAsync(userId, roleId);
        await _context.SaveChangesAsync();

        // Assert
        var userRoleInDb = await _context.UserRoles.FirstOrDefaultAsync(
            ur => ur.UserId == userId && ur.RoleId == roleId
        );
        userRoleInDb.Should().BeNull();
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_ShouldDoNothing_WhenRoleNotAssigned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        await _repository.RemoveRoleFromUserAsync(userId, roleId);
        await _context.SaveChangesAsync();

        // Assert - Should not throw exception
        var userRoleInDb = await _context.UserRoles.FirstOrDefaultAsync(
            ur => ur.UserId == userId && ur.RoleId == roleId
        );
        userRoleInDb.Should().BeNull();
    }

    [Fact]
    public async Task IsRoleAssignedAsync_ShouldReturnTrue_WhenRoleIsAssigned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userRole = new UserRole(userId, roleId);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.IsRoleAssignedAsync(userId, roleId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsRoleAssignedAsync_ShouldReturnFalse_WhenRoleIsNotAssigned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        var result = await _repository.IsRoleAssignedAsync(userId, roleId);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
