using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Repositories;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class ActivityLogRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ActivityLogRepository _repository;

    public ActivityLogRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new ActivityLogRepository(_context);
    }

    [Fact]
    public async Task GetActiveLogsByUserAsync_ShouldReturnActiveLogs_ForSpecificUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var activity1 = new Activity(userId, Guid.NewGuid(), "Activity 1");
        var activity2 = new Activity(otherUserId, Guid.NewGuid(), "Activity 2");

        var log1 = new ActivityLog(activity1.Id, DateTime.UtcNow.AddHours(-1));
        var log2 = new ActivityLog(activity2.Id, DateTime.UtcNow.AddHours(-1));
        var logFinished = new ActivityLog(activity1.Id, DateTime.UtcNow.AddHours(-2));
        logFinished.Stop(DateTime.UtcNow.AddHours(-1));

        _context.Activities.AddRange(activity1, activity2);
        _context.ActivityLogs.AddRange(log1, log2, logFinished);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveLogsByUserAsync(userId);

        // Assert
        result.Should().ContainSingle();
        result.First().Id.Should().Be(log1.Id);
    }

    [Fact]
    public async Task GetOverlappingLogsAsync_ShouldReturnOverlappingLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Activity");
        _context.Activities.Add(activity);

        // 10:00 - 12:00
        var log = new ActivityLog(activity.Id, new DateTime(2024, 1, 1, 10, 0, 0));
        log.Stop(new DateTime(2024, 1, 1, 12, 0, 0));
        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        // Overlap: 11:00 - 13:00
        var start = new DateTime(2024, 1, 1, 11, 0, 0);
        var end = new DateTime(2024, 1, 1, 13, 0, 0);

        // Act
        var result = await _repository.GetOverlappingLogsAsync(userId, start, end);

        // Assert
        result.Should().ContainSingle(l => l.Id == log.Id);
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
