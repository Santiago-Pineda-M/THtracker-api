namespace THtracker.Tests.Unit.Application.Tasks;

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using THtracker.Application.DTOs.Tasks;
using THtracker.Application.UseCases.Tasks;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class UpdateTaskItemUseCaseTests
{
    private readonly Mock<ITaskRepository> repositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<IValidator<UpdateTaskItemRequest>> validatorMock;
    private readonly UpdateTaskItemUseCase useCase;

    public UpdateTaskItemUseCaseTests()
    {
        this.repositoryMock = new Mock<ITaskRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.validatorMock = new Mock<IValidator<UpdateTaskItemRequest>>();
        this.useCase = new UpdateTaskItemUseCase(
            this.repositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.validatorMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateTask_WhenDataIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = new TaskItem(Guid.NewGuid(), userId, "Vieja Tarea");
        var request = new UpdateTaskItemRequest("Nueva Tarea", null);
        this.repositoryMock.Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        this.validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await this.useCase.ExecuteAsync(userId, task.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Nueva Tarea");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenTaskNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var request = new UpdateTaskItemRequest("Tarea", null);
        this.repositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);
        this.validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await this.useCase.ExecuteAsync(userId, taskId, request);

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
        var request = new UpdateTaskItemRequest("Tarea", null);
        this.repositoryMock.Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        this.validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await this.useCase.ExecuteAsync(otherUserId, task.Id, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }
}
