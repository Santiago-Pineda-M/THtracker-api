using FluentAssertions;
using Moq;
using THtracker.Application.DTOs.ActivityValueDefinitions;
using THtracker.Application.UseCases.ActivityValueDefinitions;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.ActivityValueDefinitions;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class CreateValueDefinitionUseCaseTests
{
    private readonly Mock<IActivityValueDefinitionRepository> _definitionRepositoryMock;
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly CreateValueDefinitionUseCase _useCase;

    public CreateValueDefinitionUseCaseTests()
    {
        _definitionRepositoryMock = new Mock<IActivityValueDefinitionRepository>();
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _useCase = new CreateValueDefinitionUseCase(_definitionRepositoryMock.Object, _activityRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreate_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Running", true);
        var request = new CreateValueDefinitionRequest("Distance", "Number", true, "km");

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await _useCase.ExecuteAsync(userId, activityId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Distance");
        result.ValueType.Should().Be("Number");
        result.IsRequired.Should().BeTrue();
        result.Unit.Should().Be("km");
        _definitionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ActivityValueDefinition>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_ShouldThrow_WhenNameIsInvalid(string invalidName)
    {
        // Arrange
        var request = new CreateValueDefinitionRequest(invalidName, "Number");

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*El nombre es obligatorio*");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenTypeIsInvalid()
    {
        // Arrange
        var request = new CreateValueDefinitionRequest("Distance", "InvalidType");

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*El tipo debe ser uno de: Number, Text, Boolean, Time*");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenActivityDoesNotBelongToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var activityId = Guid.NewGuid();
        var activity = new Activity(otherUserId, Guid.NewGuid(), "Secret Activity", true);
        var request = new CreateValueDefinitionRequest("Extra Data", "Text");

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(userId, activityId, request);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("No tienes acceso a esta actividad.");
    }
}
