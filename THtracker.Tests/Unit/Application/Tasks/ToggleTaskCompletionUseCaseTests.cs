namespace THtracker.Tests.Unit.Application.Tasks;

using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Tasks;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class ToggleTaskCompletionUseCaseTests
{
    private readonly Mock<ITaskRepository> repositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly ToggleTaskCompletionUseCase useCase;

    public ToggleTaskCompletionUseCaseTests()
    {
        this.repositoryMock = new Mock<ITaskRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.useCase = new ToggleTaskCompletionUseCase(
            this.repositoryMock.Object,
            this.unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldToggleTask_WhenUserOwnsTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = new TaskItem(Guid.NewGuid(), userId, "Tarea");
        this.repositoryMock.Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, task.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenTaskNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        this.repositoryMock.Setup(x =>
                x.GetByIdAsync(Guid.NewGuid(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, Guid.NewGuid());

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
