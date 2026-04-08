using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.ActivityLogs;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.ActivityLogs;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class GetActiveActivityLogsUseCaseTests
{
    private readonly Mock<IActivityLogRepository> _logRepositoryMock;
    private readonly GetActiveActivityLogsUseCase _useCase;

    public GetActiveActivityLogsUseCaseTests()
    {
        _logRepositoryMock = new Mock<IActivityLogRepository>();
        _useCase = new GetActiveActivityLogsUseCase(_logRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenNoActiveLogsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _logRepositoryMock.Setup(x => x.GetActiveLogsByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityLog>());

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnActiveLogs_WhenTheyExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow.AddHours(-1);
        var activeLog = new ActivityLog(activityId, startedAt);
        
        _logRepositoryMock.Setup(x => x.GetActiveLogsByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityLog> { activeLog });

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var logResponse = result.Value.First();
        logResponse.Id.Should().Be(activeLog.Id);
        logResponse.ActivityId.Should().Be(activityId);
        logResponse.StartedAt.Should().Be(startedAt);
        logResponse.EndedAt.Should().BeNull();
        logResponse.DurationMinutes.Should().BeApproximately(60, 1);
    }
}
