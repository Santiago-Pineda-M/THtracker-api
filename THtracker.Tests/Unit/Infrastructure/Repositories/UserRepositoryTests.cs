using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Repositories;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        var user1 = new User("User 1", "user1@example.com");
        var user2 = new User("User 2", "user2@example.com");
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Name == "User 1" && u.Email == "user1@example.com");
        result.Should().Contain(u => u.Name == "User 2" && u.Email == "user2@example.com");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        var user = new User("John Doe", "john@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _repository.GetByIdAsync(nonExistentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser()
    {
        var user = new User("Jane Smith", "jane@example.com");

        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be("Jane Smith");
        user.Email.Should().Be("jane@example.com");
        var userInDb = await _context.Users.FindAsync(user.Id);
        userInDb.Should().NotBeNull();
        userInDb!.Name.Should().Be("Jane Smith");
        userInDb.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser_WhenUserExists()
    {
        var user = new User("Original Name", "original@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        user.Update("Updated Name", "updated@example.com");

        await _repository.UpdateAsync(user);

        var userInDb = await _context.Users.FindAsync(user.Id);
        userInDb!.Name.Should().Be("Updated Name");
        userInDb.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser_WhenUserExists()
    {
        var user = new User("To Delete", "delete@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAsync(user.Id);
        await _context.SaveChangesAsync();

        result.Should().BeTrue();
        var userInDb = await _context.Users.FindAsync(user.Id);
        userInDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _repository.DeleteAsync(nonExistentId);

        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
