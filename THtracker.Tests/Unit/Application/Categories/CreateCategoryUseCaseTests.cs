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
public class CreateCategoryUseCaseTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly CreateCategoryUseCase _useCase;

    public CreateCategoryUseCaseTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _useCase = new CreateCategoryUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateCategory_WhenDataIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateCategoryRequest("New Category");
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.UserId.Should().Be(userId);
        _repositoryMock.Verify(x => x.AddAsync(It.Is<Category>(c => c.Name == request.Name && c.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenNameIsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateCategoryRequest("");

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*El nombre de la categoría es obligatorio.*");
    }
}
