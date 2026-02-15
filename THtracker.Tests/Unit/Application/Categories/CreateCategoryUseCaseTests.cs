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
public class CreateCategoryUseCaseTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidator<CreateCategoryRequest>> _validatorMock;
    private readonly CreateCategoryUseCase _useCase;

    public CreateCategoryUseCaseTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IValidator<CreateCategoryRequest>>();
        
        _useCase = new CreateCategoryUseCase(
            _repositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateCategory_WhenDataIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateCategoryRequest("New Category");
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(request.Name);
        result.Value.UserId.Should().Be(userId);
        
        _repositoryMock.Verify(x => x.AddAsync(It.Is<Category>(c => c.Name == request.Name && c.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateCategoryRequest("");
        var failures = new List<ValidationFailure> { new("Name", "Name is required") };
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
