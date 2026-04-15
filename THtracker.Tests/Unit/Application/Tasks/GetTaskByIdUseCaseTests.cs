namespace THtracker.Tests.Unit.Application.Tasks;

using FluentAssertions;
using Moq;
using THtracker.Application.DTOs.Tasks;
using THtracker.Application.UseCases.Tasks;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class GetTaskByIdUseCaseTests
{
    private readonly Mock<ITaskRepository> repositoryMock;
    private readonly GetTaskByIdUseCase useCase;

    public GetTaskByIdUseCaseTests()
    {
        this.repositoryMock = new Mock<ITaskRepository>();
        this.useCase = new GetTaskByIdUseCase(this.repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTask_WhenTaskExistsAndUserOwnsIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = new TaskItem(Guid.NewGuid(), userId, "Mi Tarea");
        this.repositoryMock.Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, task.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Mi Tarea");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenTaskNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        this.repositoryMock.Setup(x => x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, nonExistentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenUserDoesNotOwnTask()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var task = new TaskItem(Guid.NewGuid(), ownerId, "Tarea Ajena");
        this.repositoryMock.Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var result = await this.useCase.ExecuteAsync(otherUserId, task.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }
}
