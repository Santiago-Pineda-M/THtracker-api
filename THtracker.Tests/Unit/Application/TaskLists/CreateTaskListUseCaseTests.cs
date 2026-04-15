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
public class CreateTaskListUseCaseTests
{
    private readonly Mock<ITaskListRepository> repositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<IValidator<CreateTaskListRequest>> validatorMock;
    private readonly CreateTaskListUseCase useCase;

    public CreateTaskListUseCaseTests()
    {
        this.repositoryMock = new Mock<ITaskListRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.validatorMock = new Mock<IValidator<CreateTaskListRequest>>();
        this.useCase = new CreateTaskListUseCase(
            this.repositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.validatorMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateTaskList_WhenDataIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTaskListRequest("Nueva Lista", "#FF0000");
        this.validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await this.useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Nueva Lista");
        result.Value.Color.Should().Be("#FF0000");
        this.repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<TaskList>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        this.unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTaskListRequest("", "#GGGGGG");
        var failures = new List<ValidationFailure> { new("Name", "El nombre es obligatorio") };
        this.validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var result = await this.useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
