using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
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
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidator<UpdateActivityLogRequest>> _validatorMock;
    private readonly UpdateActivityLogUseCase _useCase;

    public UpdateActivityLogUseCaseTests()
    {
        _logRepositoryMock = new Mock<IActivityLogRepository>();
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IValidator<UpdateActivityLogRequest>>();
        
        _useCase = new UpdateActivityLogUseCase(
            _logRepositoryMock.Object, 
            _activityRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _validatorMock.Object);
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

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _logRepositoryMock.Setup(x => x.GetByIdAsync(logId, It.IsAny<CancellationToken>())).ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        _logRepositoryMock.Setup(x => x.GetOverlappingLogsAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), logId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityLog>());

        // Act
        var result = await _useCase.ExecuteAsync(userId, logId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StartedAt.Should().Be(request.StartedAt);
        result.Value.EndedAt.Should().Be(request.EndedAt);
        
        _logRepositoryMock.Verify(x => x.UpdateAsync(log, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenOverlapNotAllowed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var logId = Guid.NewGuid();
        var log = new ActivityLog(activityId, DateTime.UtcNow.AddHours(-5));
        var activity = new Activity(userId, Guid.NewGuid(), "Exclusive", false);
        var request = new UpdateActivityLogRequest(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

        var otherLog = new ActivityLog(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-30));

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _logRepositoryMock.Setup(x => x.GetByIdAsync(logId, It.IsAny<CancellationToken>())).ReturnsAsync(log);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        _logRepositoryMock.Setup(x => x.GetOverlappingLogsAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), logId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActivityLog> { otherLog });

        // Act
        var result = await _useCase.ExecuteAsync(userId, logId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OverlapConflict");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var request = new UpdateActivityLogRequest(DateTime.UtcNow, DateTime.UtcNow.AddHours(-1));
        var failures = new List<ValidationFailure> { new("EndedAt", "End date must be after start date") };
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var result = await _useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
