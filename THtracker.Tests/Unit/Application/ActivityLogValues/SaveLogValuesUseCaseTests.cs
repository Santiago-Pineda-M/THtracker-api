using FluentAssertions;
using Moq;
using THtracker.Application.DTOs.ActivityLogValues;
using THtracker.Application.UseCases.ActivityLogValues;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.ActivityLogValues;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class SaveLogValuesUseCaseTests
{
    private readonly Mock<IActivityLogValueRepository> _logValueRepositoryMock;
    private readonly Mock<IActivityLogRepository> _logRepositoryMock;
    private readonly Mock<IActivityValueDefinitionRepository> _definitionRepositoryMock;
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SaveLogValuesUseCase _useCase;

    public SaveLogValuesUseCaseTests()
    {
        _logValueRepositoryMock = new Mock<IActivityLogValueRepository>();
        _logRepositoryMock = new Mock<IActivityLogRepository>();
        _definitionRepositoryMock = new Mock<IActivityValueDefinitionRepository>();
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        _useCase = new SaveLogValuesUseCase(
            _logValueRepositoryMock.Object,
            _logRepositoryMock.Object,
            _definitionRepositoryMock.Object,
            _activityRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSaveValues_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var logId = Guid.NewGuid();
        var log = new ActivityLog(activityId, DateTime.UtcNow);
        var activity = new Activity(userId, Guid.NewGuid(), "Running", true);
        var definition = new ActivityValueDefinition(activityId, "Distance", "Number");

        _logRepositoryMock.Setup(x => x.GetByIdAsync(logId, It.IsAny<CancellationToken>())).ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        _definitionRepositoryMock.Setup(x => x.GetAllByActivityAsync(activityId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ActivityValueDefinition> { definition });
        
        var requests = new List<LogValueRequest> { new LogValueRequest(definition.Id, "10") };

        // Act
        var result = await _useCase.ExecuteAsync(userId, logId, requests);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        _logValueRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ActivityLogValue>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenNumberIsInvalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var logId = Guid.NewGuid();
        var log = new ActivityLog(activityId, DateTime.UtcNow);
        var activity = new Activity(userId, Guid.NewGuid(), "Running", true);
        var definition = new ActivityValueDefinition(activityId, "Distance", "Number");

        _logRepositoryMock.Setup(x => x.GetByIdAsync(logId, It.IsAny<CancellationToken>())).ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        _definitionRepositoryMock.Setup(x => x.GetAllByActivityAsync(activityId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ActivityValueDefinition> { definition });

        var requests = new List<LogValueRequest> { new LogValueRequest(definition.Id, "invalid") };

        // Act
        var result = await _useCase.ExecuteAsync(userId, logId, requests);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenUserDoesNotOwnLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var logId = Guid.NewGuid();
        var log = new ActivityLog(Guid.NewGuid(), DateTime.UtcNow);
        var activity = new Activity(otherUserId, Guid.NewGuid(), "Running", true);

        _logRepositoryMock.Setup(x => x.GetByIdAsync(logId, It.IsAny<CancellationToken>())).ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(log.ActivityId, It.IsAny<CancellationToken>())).ReturnsAsync(activity);

        // Act
        var result = await _useCase.ExecuteAsync(userId, logId, new List<LogValueRequest>());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }
}
