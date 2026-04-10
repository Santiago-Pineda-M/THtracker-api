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
public class GetActivityLogsUseCaseTests
{
    private readonly Mock<IActivityLogRepository> _logRepositoryMock;
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly GetActivityLogsUseCase _useCase;

    public GetActivityLogsUseCaseTests()
    {
        _logRepositoryMock = new Mock<IActivityLogRepository>();
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _useCase = new GetActivityLogsUseCase(_logRepositoryMock.Object, _activityRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnLogs_WhenNoFiltersProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetActivityLogsRequest();
        var logs = new List<ActivityLog>
        {
            new ActivityLog(Guid.NewGuid(), DateTime.UtcNow.AddHours(-1))
        };

        _logRepositoryMock.Setup(x => x.GetLogsAsync(userId, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        _logRepositoryMock.Verify(x => x.GetLogsAsync(userId, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenActivityDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var request = new GetActivityLogsRequest(ActivityId: activityId);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenActivityDoesNotBelongToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var activity = new Activity(otherUserId, Guid.NewGuid(), "Other User Activity");
        var request = new GetActivityLogsRequest(ActivityId: activityId);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }
}
