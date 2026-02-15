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
public class StartActivityUseCaseTests
{
    private readonly Mock<IActivityLogRepository> _logRepositoryMock;
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly StartActivityUseCase _useCase;

    public StartActivityUseCaseTests()
    {
        _logRepositoryMock = new Mock<IActivityLogRepository>();
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new StartActivityUseCase(
            _logRepositoryMock.Object, 
            _activityRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStart_WhenNoActiveLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Exclusive", false);
        var request = new StartActivityLogRequest(activityId);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        _logRepositoryMock.Setup(x => x.GetActiveLogsByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityLog>());

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _logRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ActivityLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenStartingExclusiveWhileActiveExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Exclusive", false);
        var request = new StartActivityLogRequest(activityId);

        var activeLog = new ActivityLog(Guid.NewGuid(), DateTime.UtcNow);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        _logRepositoryMock.Setup(x => x.GetActiveLogsByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityLog> { activeLog });

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OverlapConflict");
        result.Error.Description.Should().Contain("No puedes iniciar una actividad que no permite solapamiento");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenStartingAnyWhileExclusiveIsActive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var newActivity = new Activity(userId, Guid.NewGuid(), "Overlappable", true);
        var request = new StartActivityLogRequest(activityId);

        var activeActivityId = Guid.NewGuid();
        var activeActivity = new Activity(userId, Guid.NewGuid(), "Exclusive Active", false);
        var activeLog = new ActivityLog(activeActivityId, DateTime.UtcNow);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newActivity);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activeActivityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeActivity);
        _logRepositoryMock.Setup(x => x.GetActiveLogsByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityLog> { activeLog });

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OverlapConflict");
        result.Error.Description.Should().Contain("no permite solapamiento");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllow_WhenBothAllowOverlap()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var newActivity = new Activity(userId, Guid.NewGuid(), "Overlappable New", true);
        var request = new StartActivityLogRequest(activityId);

        var activeActivityId = Guid.NewGuid();
        var activeActivity = new Activity(userId, Guid.NewGuid(), "Overlappable Active", true);
        var activeLog = new ActivityLog(activeActivityId, DateTime.UtcNow);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newActivity);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activeActivityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeActivity);
        _logRepositoryMock.Setup(x => x.GetActiveLogsByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityLog> { activeLog });

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _logRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ActivityLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
