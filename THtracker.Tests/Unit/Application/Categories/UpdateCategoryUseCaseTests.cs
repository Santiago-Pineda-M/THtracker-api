using FluentAssertions;
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
    private readonly UpdateCategoryUseCase _useCase;

    public UpdateCategoryUseCaseTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _useCase = new UpdateCategoryUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateCategory_WhenCategoryExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category(userId, "Old Name");
        var request = new UpdateCategoryRequest("Updated Name");

        _repositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _repositoryMock.Setup(x => x.UpdateAsync(category, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(categoryId, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(request.Name);
        _repositoryMock.Verify(x => x.UpdateAsync(It.Is<Category>(c => c.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryRequest("Some Name");
        _repositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _useCase.ExecuteAsync(categoryId, request);

        // Assert
        result.Should().BeNull();
    }
}
