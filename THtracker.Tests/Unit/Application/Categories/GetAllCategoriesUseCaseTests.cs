using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Categories;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Categories;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class GetAllCategoriesUseCaseTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly GetAllCategoriesUseCase _useCase;

    public GetAllCategoriesUseCaseTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _useCase = new GetAllCategoriesUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnCategories_ForSpecificUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categories = new List<Category>
        {
            new Category(userId, "Cat 1"),
            new Category(userId, "Cat 2")
        };

        _repositoryMock.Setup(x => x.GetAllByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Cat 1");
        result.Should().Contain(c => c.Name == "Cat 2");
        _repositoryMock.Verify(x => x.GetAllByUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
