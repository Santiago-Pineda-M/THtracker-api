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
public class CreateTaskItemUseCaseTests
{
    private readonly Mock<ITaskRepository> taskRepositoryMock;
    private readonly Mock<ITaskListRepository> taskListRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<IValidator<CreateTaskItemRequest>> validatorMock;
    private readonly CreateTaskItemUseCase useCase;

    public CreateTaskItemUseCaseTests()
    {
        this.taskRepositoryMock = new Mock<ITaskRepository>();
        this.taskListRepositoryMock = new Mock<ITaskListRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.validatorMock = new Mock<IValidator<CreateTaskItemRequest>>();
        this.useCase = new CreateTaskItemUseCase(
            this.taskRepositoryMock.Object,
            this.taskListRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.validatorMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateTask_WhenDataIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskList = new TaskList(userId, "Lista", "#FF0000");
        var request = new CreateTaskItemRequest(taskList.Id, "Nueva Tarea", null);
        this.taskListRepositoryMock.Setup(x =>
                x.GetByIdAsync(taskList.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(taskList);
        this.validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await this.useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Nueva Tarea");
        result.Value.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTaskItemRequest(Guid.NewGuid(), "", null);
        var failures = new List<ValidationFailure>
        {
            new("Content", "El contenido es obligatorio"),
        };
        this.validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var result = await this.useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenTaskListNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var request = new CreateTaskItemRequest(taskListId, "Tarea", null);
        this.validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        this.taskListRepositoryMock.Setup(x =>
                x.GetByIdAsync(taskListId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((TaskList?)null);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }
}
