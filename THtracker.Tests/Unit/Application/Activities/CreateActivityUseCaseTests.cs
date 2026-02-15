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
public class CreateActivityUseCaseTests
{
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidator<CreateActivityRequest>> _validatorMock;
    private readonly CreateActivityUseCase _useCase;

    public CreateActivityUseCaseTests()
    {
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IValidator<CreateActivityRequest>>();
        
        _useCase = new CreateActivityUseCase(
            _activityRepositoryMock.Object, 
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateActivity_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category(userId, "Category 1");
        var request = new CreateActivityRequest(categoryId, "New Activity", true);

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Activity");
        
        _activityRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = new CreateActivityRequest(categoryId, "New Activity", true);

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenCategoryDoesNotBelongToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category(otherUserId, "Category 1");
        var request = new CreateActivityRequest(categoryId, "New Activity", true);

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var request = new CreateActivityRequest(Guid.Empty, "", true);
        var validationFailures = new List<ValidationFailure> { new("Name", "Name is required") };
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _useCase.ExecuteAsync(Guid.NewGuid(), request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
