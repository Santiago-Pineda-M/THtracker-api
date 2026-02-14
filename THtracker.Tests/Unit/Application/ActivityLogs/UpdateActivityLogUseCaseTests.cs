using FluentAssertions;
using Moq;
using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Application.UseCases.ActivityLogs;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.ActivityLogs;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class UpdateActivityLogUseCaseTests
{
    private readonly Mock<IActivityLogRepository> _logRepositoryMock;
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly UpdateActivityLogUseCase _useCase;

    public UpdateActivityLogUseCaseTests()
    {
        _logRepositoryMock = new Mock<IActivityLogRepository>();
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _useCase = new UpdateActivityLogUseCase(_logRepositoryMock.Object, _activityRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdate_WhenValidAndNoOverlap()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var logId = Guid.NewGuid();
        var log = new ActivityLog(activityId, DateTime.UtcNow.AddHours(-2));
        var activity = new Activity(userId, Guid.NewGuid(), "Work", false);
        var request = new UpdateActivityLogRequest(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

        _logRepositoryMock.Setup(x => x.GetByIdAsync(logId, It.IsAny<CancellationToken>())).ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        _logRepositoryMock.Setup(x => x.GetOverlappingLogsAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), logId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityLog>());

        // Act
        var result = await _useCase.ExecuteAsync(userId, logId, request);

        // Assert
        result.Should().NotBeNull();
        result!.StartedAt.Should().Be(request.StartedAt);
        result.EndedAt.Should().Be(request.EndedAt);
        _logRepositoryMock.Verify(x => x.UpdateAsync(log, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenOverlapNotAllowed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var logId = Guid.NewGuid();
        var log = new ActivityLog(activityId, DateTime.UtcNow.AddHours(-5));
        var activity = new Activity(userId, Guid.NewGuid(), "Exclusive", false);
        var request = new UpdateActivityLogRequest(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

        var otherLog = new ActivityLog(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-30));

        _logRepositoryMock.Setup(x => x.GetByIdAsync(logId, It.IsAny<CancellationToken>())).ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        _logRepositoryMock.Setup(x => x.GetOverlappingLogsAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), logId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityLog> { otherLog });

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(userId, logId, request);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*no permite solapamiento*");
    }
}
