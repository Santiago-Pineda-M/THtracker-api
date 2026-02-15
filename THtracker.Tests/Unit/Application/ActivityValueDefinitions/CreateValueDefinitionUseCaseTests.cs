using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using THtracker.Application.DTOs.ActivityValueDefinitions;
using THtracker.Application.UseCases.ActivityValueDefinitions;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.ActivityValueDefinitions;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class CreateValueDefinitionUseCaseTests
{
    private readonly Mock<IActivityValueDefinitionRepository> _definitionRepositoryMock;
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidator<CreateValueDefinitionRequest>> _validatorMock;
    private readonly CreateValueDefinitionUseCase _useCase;

    public CreateValueDefinitionUseCaseTests()
    {
        _definitionRepositoryMock = new Mock<IActivityValueDefinitionRepository>();
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IValidator<CreateValueDefinitionRequest>>();
        
        _useCase = new CreateValueDefinitionUseCase(
            _definitionRepositoryMock.Object, 
            _activityRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreate_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Running", true);
        var request = new CreateValueDefinitionRequest("Distance", "Number", true, "km");

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await _useCase.ExecuteAsync(userId, activityId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Distance");
        result.Value.ValueType.Should().Be("Number");
        result.Value.IsRequired.Should().BeTrue();
        result.Value.Unit.Should().Be("km");
        
        _definitionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ActivityValueDefinition>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var request = new CreateValueDefinitionRequest("", "Number");
        var failures = new List<ValidationFailure> { new("Name", "El nombre es obligatorio") };
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var result = await _useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenActivityDoesNotBelongToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var activity = new Activity(otherUserId, Guid.NewGuid(), "Secret Activity", true);
        var request = new CreateValueDefinitionRequest("Extra Data", "Text");

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await _useCase.ExecuteAsync(userId, activityId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }
}
