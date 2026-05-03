using Moq;
using FluentAssertions;
using THtracker.Application.Features.ActivityLogValues;
using THtracker.Application.Features.ActivityLogValues.Commands.SaveLogValues;
using THtracker.Application.Features.ActivityLogValues.Queries.GetLogValues;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Tests.Unit.Application.ActivityLogValues;

public class ActivityLogValueHandlerTests
{
    private readonly Mock<IActivityLogValueRepository> _logValueRepositoryMock;
    private readonly Mock<IActivityLogRepository> _logRepositoryMock;
    private readonly Mock<IActivityValueDefinitionRepository> _definitionRepositoryMock;
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public ActivityLogValueHandlerTests()
    {
        _logValueRepositoryMock = new Mock<IActivityLogValueRepository>();
        _logRepositoryMock = new Mock<IActivityLogRepository>();
        _definitionRepositoryMock = new Mock<IActivityValueDefinitionRepository>();
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    // ─── SAVE LOG VALUES ──────────────────────────────────────────────────────

    [Fact]
    public async Task SaveLogValues_ShouldReturnSuccess_WhenValuesAreValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Correr", "#000", true);
        var log = new ActivityLog(activity.Id, DateTime.UtcNow);
        var definition = new ActivityValueDefinition(activity.Id, "Distancia", "Number", true, "0", null);
        
        var values = new List<LogValueItem> { new LogValueItem(definition.Id, "5.5") };
        var command = new SaveLogValuesCommand(log.Id, values, userId);
        
        var handler = new SaveLogValuesCommandHandler(
            _logValueRepositoryMock.Object, _logRepositoryMock.Object, 
            _definitionRepositoryMock.Object, _activityRepositoryMock.Object, _unitOfWorkMock.Object);

        _logRepositoryMock.Setup(x => x.GetByIdAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        _definitionRepositoryMock.Setup(x => x.GetAllByActivityAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityValueDefinition> { definition });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        _logValueRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ActivityLogValue>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveLogValues_ShouldReturnFailure_WhenLogDoesNotExist()
    {
        // Arrange
        var command = new SaveLogValuesCommand(Guid.NewGuid(), new List<LogValueItem>(), Guid.NewGuid());
        var handler = new SaveLogValuesCommandHandler(
            _logValueRepositoryMock.Object, _logRepositoryMock.Object, 
            _definitionRepositoryMock.Object, _activityRepositoryMock.Object, _unitOfWorkMock.Object);

        _logRepositoryMock.Setup(x => x.GetByIdAsync(command.ActivityLogId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActivityLog?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task SaveLogValues_ShouldReturnFailure_WhenDifferentUser()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var attackerUserId = Guid.NewGuid();
        var activity = new Activity(ownerUserId, Guid.NewGuid(), "Correr", "#000", true);
        var log = new ActivityLog(activity.Id, DateTime.UtcNow);

        var command = new SaveLogValuesCommand(log.Id, new List<LogValueItem>(), attackerUserId);
        
        var handler = new SaveLogValuesCommandHandler(
            _logValueRepositoryMock.Object, _logRepositoryMock.Object, 
            _definitionRepositoryMock.Object, _activityRepositoryMock.Object, _unitOfWorkMock.Object);

        _logRepositoryMock.Setup(x => x.GetByIdAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }

    [Fact]
    public async Task SaveLogValues_ShouldReturnFailure_WhenValueIsInvalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Correr", "#000", true);
        var log = new ActivityLog(activity.Id, DateTime.UtcNow);
        var definition = new ActivityValueDefinition(activity.Id, "Distancia", "Number", true, "0", null);
        
        var values = new List<LogValueItem> { new LogValueItem(definition.Id, "NoEsNumero") };
        var command = new SaveLogValuesCommand(log.Id, values, userId);
        
        var handler = new SaveLogValuesCommandHandler(
            _logValueRepositoryMock.Object, _logRepositoryMock.Object, 
            _definitionRepositoryMock.Object, _activityRepositoryMock.Object, _unitOfWorkMock.Object);

        _logRepositoryMock.Setup(x => x.GetByIdAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        _definitionRepositoryMock.Setup(x => x.GetAllByActivityAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityValueDefinition> { definition });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }

    // ─── QUERIES ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLogValues_ShouldReturnValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Correr", "#000", true);
        var log = new ActivityLog(activity.Id, DateTime.UtcNow);
        var values = new List<ActivityLogValue>
        {
            new ActivityLogValue(log.Id, Guid.NewGuid(), "5.5")
        };

        var query = new GetLogValuesQuery(log.Id, userId);
        var handler = new GetLogValuesQueryHandler(
            _logValueRepositoryMock.Object, _logRepositoryMock.Object, _activityRepositoryMock.Object);

        _logRepositoryMock.Setup(x => x.GetByIdAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        _logValueRepositoryMock.Setup(x => x.GetAllByLogAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(values);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetLogValues_ShouldReturnFailure_WhenDifferentUser()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var attackerUserId = Guid.NewGuid();
        var activity = new Activity(ownerUserId, Guid.NewGuid(), "Correr", "#000", true);
        var log = new ActivityLog(activity.Id, DateTime.UtcNow);

        var query = new GetLogValuesQuery(log.Id, attackerUserId);
        var handler = new GetLogValuesQueryHandler(
            _logValueRepositoryMock.Object, _logRepositoryMock.Object, _activityRepositoryMock.Object);

        _logRepositoryMock.Setup(x => x.GetByIdAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }
}
