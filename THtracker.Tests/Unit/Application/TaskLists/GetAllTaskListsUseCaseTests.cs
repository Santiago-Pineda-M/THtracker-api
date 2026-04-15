namespace THtracker.Tests.Unit.Application.TaskLists;

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using THtracker.Application.DTOs.TaskLists;
using THtracker.Application.UseCases.TaskLists;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class GetAllTaskListsUseCaseTests
{
    private readonly Mock<ITaskListRepository> repositoryMock;
    private readonly GetAllTaskListsUseCase useCase;

    public GetAllTaskListsUseCaseTests()
    {
        this.repositoryMock = new Mock<ITaskListRepository>();
        this.useCase = new GetAllTaskListsUseCase(this.repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAllTaskLists_WhenUserHasTaskLists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskLists = new List<TaskList>
        {
            new TaskList(userId, "Lista 1", "#FF0000"),
            new TaskList(userId, "Lista 2", "#00FF00"),
        };

        this.repositoryMock.Setup(x => x.GetAllByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskLists);

        // Act
        var result = await this.useCase.ExecuteAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Lista 1");
        result.Last().Name.Should().Be("Lista 2");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenUserHasNoTaskLists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        this.repositoryMock.Setup(x => x.GetAllByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskList>());

        // Act
        var result = await this.useCase.ExecuteAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }
}
