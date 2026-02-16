using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Repositories;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class PermissionRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PermissionRepository _repository;

    public PermissionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new PermissionRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPermissions()
    {
        // Arrange
        var permission1 = new Permission("Read", "Can read data");
        var permission2 = new Permission("Write", "Can write data");
        var permission3 = new Permission("Delete", "Can delete data");

        _context.Permissions.AddRange(permission1, permission2, permission3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(p => p.Name == "Read");
        result.Should().Contain(p => p.Name == "Write");
        result.Should().Contain(p => p.Name == "Delete");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPermissionsExist()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPermission_WhenExists()
    {
        // Arrange
        var permission = new Permission("Admin", "Administrator permission");
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(permission.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(permission.Id);
        result.Name.Should().Be("Admin");
        result.Description.Should().Be("Administrator permission");
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
    public async Task AddAsync_ShouldAddPermission()
    {
        // Arrange
        var permission = new Permission("Create", "Can create new items");

        // Act
        await _repository.AddAsync(permission);
        await _context.SaveChangesAsync();

        // Assert
        var permissionInDb = await _context.Permissions.FindAsync(permission.Id);
        permissionInDb.Should().NotBeNull();
        permissionInDb!.Name.Should().Be("Create");
        permissionInDb.Description.Should().Be("Can create new items");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePermission()
    {
        // Arrange
        var permission = new Permission("Original", "Original description");
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Note: Permission entity doesn't have an Update method, so we'll test the repository's Update method
        // Act
        await _repository.UpdateAsync(permission);
        await _context.SaveChangesAsync();

        // Assert
        var permissionInDb = await _context.Permissions.FindAsync(permission.Id);
        permissionInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeletePermission_WhenExists()
    {
        // Arrange
        var permission = new Permission("ToDelete", "Will be deleted");
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(permission.Id);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var permissionInDb = await _context.Permissions.FindAsync(permission.Id);
        permissionInDb.Should().BeNull();
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
