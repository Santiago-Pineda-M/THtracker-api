using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Repositories;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class ActivityLogValueRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ActivityLogValueRepository _repository;

    public ActivityLogValueRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new ActivityLogValueRepository(_context);
    }

    [Fact]
    public async Task GetAllByLogAsync_ShouldReturnLogValues_ForSpecificActivityLog()
    {
        // Arrange
        var activityLogId = Guid.NewGuid();
        var otherActivityLogId = Guid.NewGuid();
        var valueDefId = Guid.NewGuid();

        var value1 = new ActivityLogValue(activityLogId, valueDefId, "10");
        var value2 = new ActivityLogValue(activityLogId, valueDefId, "20");
        var valueOther = new ActivityLogValue(otherActivityLogId, valueDefId, "30");

        _context.ActivityLogValues.AddRange(value1, value2, valueOther);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByLogAsync(activityLogId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(v => v.Id == value1.Id);
        result.Should().Contain(v => v.Id == value2.Id);
        result.Should().NotContain(v => v.Id == valueOther.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnLogValue_WhenExists()
    {
        // Arrange
        var logValue = new ActivityLogValue(Guid.NewGuid(), Guid.NewGuid(), "42");
        _context.ActivityLogValues.Add(logValue);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(logValue.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(logValue.Id);
        result.Value.Should().Be("42");
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
    public async Task AddAsync_ShouldAddLogValue()
    {
        // Arrange
        var logValue = new ActivityLogValue(Guid.NewGuid(), Guid.NewGuid(), "100");

        // Act
        await _repository.AddAsync(logValue);
        await _context.SaveChangesAsync();

        // Assert
        var logValueInDb = await _context.ActivityLogValues.FindAsync(logValue.Id);
        logValueInDb.Should().NotBeNull();
        logValueInDb!.Value.Should().Be("100");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteLogValue_WhenExists()
    {
        // Arrange
        var logValue = new ActivityLogValue(Guid.NewGuid(), Guid.NewGuid(), "To Delete");
        _context.ActivityLogValues.Add(logValue);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(logValue.Id);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var logValueInDb = await _context.ActivityLogValues.FindAsync(logValue.Id);
        logValueInDb.Should().BeNull();
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
