using FluentAssertions;
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
    private readonly CreateActivityUseCase _useCase;

    public CreateActivityUseCaseTests()
    {
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _useCase = new CreateActivityUseCase(_activityRepositoryMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateActivity_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category(userId, "Category 1");
        var request = new CreateActivityRequest(categoryId, "New Activity", true);

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Activity");
        _activityRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = new CreateActivityRequest(categoryId, "New Activity", true);

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("La categoría especificada no existe.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenCategoryDoesNotBelongToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category(otherUserId, "Category 1");
        var request = new CreateActivityRequest(categoryId, "New Activity", true);

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("No tienes acceso a esta categoría.");
    }
}
