using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using THtracker.Application.DTOs.Categories;
using THtracker.Application.UseCases.Categories;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Categories;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class UpdateCategoryUseCaseTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidator<UpdateCategoryRequest>> _validatorMock;
    private readonly UpdateCategoryUseCase _useCase;

    public UpdateCategoryUseCaseTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IValidator<UpdateCategoryRequest>>();
        
        _useCase = new UpdateCategoryUseCase(
            _repositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateCategory_WhenExistsAndIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category(userId, "Old Name");
        var request = new UpdateCategoryRequest("Updated Name");

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _useCase.ExecuteAsync(userId, categoryId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(request.Name);
        
        _repositoryMock.Verify(x => x.UpdateAsync(It.Is<Category>(c => c.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryRequest("Some Name");
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId, categoryId, request);

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
        var categoryId = Guid.NewGuid();
        var category = new Category(otherUserId, "Old Name");
        var request = new UpdateCategoryRequest("Updated Name");

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _useCase.ExecuteAsync(userId, categoryId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var request = new UpdateCategoryRequest("");
        var failures = new List<ValidationFailure> { new("Name", "Name is required") };
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var result = await _useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
