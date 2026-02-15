using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using THtracker.Application.DTOs.Activities;
using THtracker.Application.UseCases.Activities;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Activities;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class UpdateActivityUseCaseTests
{
    private readonly Mock<IActivityRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidator<UpdateActivityRequest>> _validatorMock;
    private readonly UpdateActivityUseCase _useCase;

    public UpdateActivityUseCaseTests()
    {
        _repositoryMock = new Mock<IActivityRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IValidator<UpdateActivityRequest>>();
        _useCase = new UpdateActivityUseCase(
            _repositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateActivity_WhenExistsAndIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Old Name", false);
        var request = new UpdateActivityRequest("Updated Name", true);

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await _useCase.ExecuteAsync(userId, activityId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");
        result.Value.AllowOverlap.Should().BeTrue();
        
        _repositoryMock.Verify(x => x.UpdateAsync(activity, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var request = new UpdateActivityRequest("Updated Name", true);

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId, activityId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenNotOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var activity = new Activity(otherUserId, Guid.NewGuid(), "Old Name", false);
        var request = new UpdateActivityRequest("Updated Name", true);

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await _useCase.ExecuteAsync(userId, activityId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var request = new UpdateActivityRequest("", true);
        var validationFailures = new List<ValidationFailure> { new("Name", "Name is required") };
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
