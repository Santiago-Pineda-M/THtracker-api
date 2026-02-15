using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Categories;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Categories;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class DeleteCategoryUseCaseTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteCategoryUseCase _useCase;

    public DeleteCategoryUseCaseTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new DeleteCategoryUseCase(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenOwnerDeletes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category(userId, "To Delete");

        _repositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _useCase.ExecuteAsync(userId, categoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        _repositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId, categoryId);

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
        var category = new Category(otherUserId, "Not Yours");

        _repositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _useCase.ExecuteAsync(userId, categoryId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }
}
