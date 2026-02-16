using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Repositories;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class RoleRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly RoleRepository _repository;

    public RoleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new RoleRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRoles()
    {
        // Arrange
        var role1 = new Role("Admin");
        var role2 = new Role("User");
        var role3 = new Role("Moderator");

        _context.Roles.AddRange(role1, role2, role3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(r => r.Name == "Admin");
        result.Should().Contain(r => r.Name == "User");
        result.Should().Contain(r => r.Name == "Moderator");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoRolesExist()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRole_WhenExists()
    {
        // Arrange
        var role = new Role("SuperAdmin");
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(role.Id);
        result.Name.Should().Be("SuperAdmin");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnRole_WhenExists()
    {
        // Arrange
        var role = new Role("Editor");
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Editor");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Editor");
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByNameAsync("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddRole()
    {
        // Arrange
        var role = new Role("Guest");

        // Act
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Assert
        var roleInDb = await _context.Roles.FindAsync(role.Id);
        roleInDb.Should().NotBeNull();
        roleInDb!.Name.Should().Be("Guest");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRole()
    {
        // Arrange
        var role = new Role("Original");
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateAsync(role);
        await _context.SaveChangesAsync();

        // Assert
        var roleInDb = await _context.Roles.FindAsync(role.Id);
        roleInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteRole_WhenExists()
    {
        // Arrange
        var role = new Role("ToDelete");
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(role.Id);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var roleInDb = await _context.Roles.FindAsync(role.Id);
        roleInDb.Should().BeNull();
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
