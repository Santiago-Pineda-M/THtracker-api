using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Categories;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Categories;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class DeleteCategoryUseCaseTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly DeleteCategoryUseCase _useCase;

    public DeleteCategoryUseCaseTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _useCase = new DeleteCategoryUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTrue_WhenCategoryIsDeleted()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(categoryId);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _useCase.ExecuteAsync(categoryId);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
