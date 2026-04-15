namespace THtracker.Tests.Unit.Application.TaskLists;

using FluentAssertions;
using Moq;
using THtracker.Application.DTOs.TaskLists;
using THtracker.Application.UseCases.TaskLists;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class GetTaskListByIdUseCaseTests
{
    private readonly Mock<ITaskListRepository> repositoryMock;
    private readonly GetTaskListByIdUseCase useCase;

    public GetTaskListByIdUseCaseTests()
    {
        this.repositoryMock = new Mock<ITaskListRepository>();
        this.useCase = new GetTaskListByIdUseCase(this.repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTaskList_WhenTaskListExistsAndUserOwnsIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskList = new TaskList(userId, "Mi Lista", "#FF0000");
        this.repositoryMock.Setup(x => x.GetByIdAsync(taskList.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, taskList.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Mi Lista");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenTaskListNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        this.repositoryMock.Setup(x => x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
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
        this.repositoryMock.Setup(x => x.GetByIdAsync(taskList.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        // Act
        var result = await this.useCase.ExecuteAsync(otherUserId, taskList.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }
}
