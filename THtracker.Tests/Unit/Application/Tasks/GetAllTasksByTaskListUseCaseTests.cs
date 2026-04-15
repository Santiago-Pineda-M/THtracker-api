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
public class GetAllTasksByTaskListUseCaseTests
{
    private readonly Mock<ITaskRepository> taskRepositoryMock;
    private readonly Mock<ITaskListRepository> taskListRepositoryMock;
    private readonly GetAllTasksByTaskListUseCase useCase;

    public GetAllTasksByTaskListUseCaseTests()
    {
        this.taskRepositoryMock = new Mock<ITaskRepository>();
        this.taskListRepositoryMock = new Mock<ITaskListRepository>();
        this.useCase = new GetAllTasksByTaskListUseCase(
            this.taskRepositoryMock.Object,
            this.taskListRepositoryMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTasks_WhenTaskListExistsAndUserOwnsIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskList = new TaskList(userId, "Mi Lista", "#FF0000");
        var tasks = new List<TaskItem>
        {
            new TaskItem(taskList.Id, userId, "Tarea 1"),
            new TaskItem(taskList.Id, userId, "Tarea 2"),
        };
        this.taskListRepositoryMock.Setup(x =>
                x.GetByIdAsync(taskList.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(taskList);
        this.taskRepositoryMock.Setup(x =>
                x.GetAllByTaskListAsync(taskList.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(tasks);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, taskList.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenTaskListNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        this.taskListRepositoryMock.Setup(x =>
                x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((TaskList?)null);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, nonExistentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenUserDoesNotOwnTaskList()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var taskList = new TaskList(ownerId, "Lista Ajena", "#FF0000");
        this.taskListRepositoryMock.Setup(x =>
                x.GetByIdAsync(taskList.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(taskList);

        // Act
        var result = await this.useCase.ExecuteAsync(otherUserId, taskList.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }
}
