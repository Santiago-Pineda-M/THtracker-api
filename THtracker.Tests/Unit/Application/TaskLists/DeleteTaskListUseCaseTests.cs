namespace THtracker.Tests.Unit.Application.TaskLists;

using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.TaskLists;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class DeleteTaskListUseCaseTests
{
    private readonly Mock<ITaskListRepository> taskListRepositoryMock;
    private readonly Mock<ITaskRepository> taskRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly DeleteTaskListUseCase useCase;

    public DeleteTaskListUseCaseTests()
    {
        this.taskListRepositoryMock = new Mock<ITaskListRepository>();
        this.taskRepositoryMock = new Mock<ITaskRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.useCase = new DeleteTaskListUseCase(
            this.taskListRepositoryMock.Object,
            this.taskRepositoryMock.Object,
            this.unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDeleteTaskList_WhenUserOwnsIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskList = new TaskList(userId, "Lista", "#FF0000");
        this.taskListRepositoryMock.Setup(x =>
                x.GetByIdAsync(taskList.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(taskList);
        this.taskRepositoryMock.Setup(x =>
                x.GetAllByTaskListAsync(taskList.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new List<TaskItem>());
        this.taskListRepositoryMock.Setup(x =>
                x.DeleteAsync(taskList.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, taskList.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDeleteTaskListAndItsTasks_WhenUserOwnsItAndHasTasks()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskList = new TaskList(userId, "Lista", "#FF0000");
        var task1 = new TaskItem(taskList.Id, userId, "Tarea 1");
        var task2 = new TaskItem(taskList.Id, userId, "Tarea 2");
        this.taskListRepositoryMock.Setup(x =>
                x.GetByIdAsync(taskList.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(taskList);
        this.taskRepositoryMock.Setup(x =>
                x.GetAllByTaskListAsync(taskList.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new List<TaskItem> { task1, task2 });
        this.taskRepositoryMock.Setup(x =>
                x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);
        this.taskListRepositoryMock.Setup(x =>
                x.DeleteAsync(taskList.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, taskList.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.taskRepositoryMock.Verify(
            x => x.DeleteAsync(task1.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        this.taskRepositoryMock.Verify(
            x => x.DeleteAsync(task2.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
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
