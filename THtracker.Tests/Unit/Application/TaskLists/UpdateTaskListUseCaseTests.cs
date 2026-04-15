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
public class UpdateTaskListUseCaseTests
{
    private readonly Mock<ITaskListRepository> repositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<IValidator<UpdateTaskListRequest>> validatorMock;
    private readonly UpdateTaskListUseCase useCase;

    public UpdateTaskListUseCaseTests()
    {
        this.repositoryMock = new Mock<ITaskListRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.validatorMock = new Mock<IValidator<UpdateTaskListRequest>>();
        this.useCase = new UpdateTaskListUseCase(
            this.repositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.validatorMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateTaskList_WhenDataIsValidAndUserOwnsIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskList = new TaskList(userId, "Vieja Lista", "#FF0000");
        var request = new UpdateTaskListRequest("Nueva Lista", "#00FF00");
        this.repositoryMock.Setup(x => x.GetByIdAsync(taskList.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);
        this.validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await this.useCase.ExecuteAsync(userId, taskList.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Nueva Lista");
        result.Value.Color.Should().Be("#00FF00");
        this.unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenTaskListNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var request = new UpdateTaskListRequest("New Name", "#00FF00");
        this.repositoryMock.Setup(x => x.GetByIdAsync(taskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskList?)null);
        this.validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await this.useCase.ExecuteAsync(userId, taskListId, request);

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
        var request = new UpdateTaskListRequest("Nombre", "#FF0000");
        this.repositoryMock.Setup(x => x.GetByIdAsync(taskList.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);
        this.validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await this.useCase.ExecuteAsync(otherUserId, taskList.Id, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }
}
