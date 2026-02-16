using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Repositories;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class ActivityRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ActivityRepository _repository;

    public ActivityRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new ActivityRepository(_context);
    }

    [Fact]
    public async Task GetAllByUserAsync_ShouldReturnActivities_ForSpecificUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var activity1 = new Activity(userId, categoryId, "Activity 1");
        var activity2 = new Activity(userId, categoryId, "Activity 2");
        var activityOther = new Activity(otherUserId, categoryId, "Other Activity");

        _context.Activities.AddRange(activity1, activity2, activityOther);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByUserAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Id == activity1.Id);
        result.Should().Contain(a => a.Id == activity2.Id);
        result.Should().NotContain(a => a.Id == activityOther.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnActivity_WhenExists()
    {
        // Arrange
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), "Activity");
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(activity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(activity.Id);
        result.Name.Should().Be("Activity");
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
    public async Task AddAsync_ShouldAddActivity()
    {
        // Arrange
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), "New Activity");

        // Act
        await _repository.AddAsync(activity);
        await _context.SaveChangesAsync();

        // Assert
        var activityInDb = await _context.Activities.FindAsync(activity.Id);
        activityInDb.Should().NotBeNull();
        activityInDb!.Name.Should().Be("New Activity");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateActivity()
    {
        // Arrange
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), "Original Name");
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        // Act
        activity.Update("Updated Name", true);
        await _repository.UpdateAsync(activity);
        await _context.SaveChangesAsync();

        // Assert
        var activityInDb = await _context.Activities.FindAsync(activity.Id);
        activityInDb!.Name.Should().Be("Updated Name");
        activityInDb.AllowOverlap.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteActivity_WhenExists()
    {
        // Arrange
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), "To Delete");
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(activity.Id);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var activityInDb = await _context.Activities.FindAsync(activity.Id);
        activityInDb.Should().BeNull();
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
