using Moq;
using FluentAssertions;
using THtracker.Application.Features.ActivityValueDefinitions;
using THtracker.Application.Features.ActivityValueDefinitions.Commands.CreateValueDefinition;
using THtracker.Application.Features.ActivityValueDefinitions.Commands.UpdateValueDefinition;
using THtracker.Application.Features.ActivityValueDefinitions.Commands.DeleteValueDefinition;
using THtracker.Application.Features.ActivityValueDefinitions.Queries.GetAllValueDefinitions;
using THtracker.Application.Features.ActivityValueDefinitions.Queries.GetValueDefinitionById;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using MediatR;

namespace THtracker.Tests.Unit.Application.ActivityValueDefinitions;

public class ActivityValueDefinitionHandlerTests
{
    private readonly Mock<IActivityValueDefinitionRepository> _definitionRepositoryMock;
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public ActivityValueDefinitionHandlerTests()
    {
        _definitionRepositoryMock = new Mock<IActivityValueDefinitionRepository>();
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    // --- CREATE VALUE DEFINITION ---

    [Fact]
    public async Task CreateValueDefinition_ShouldReturnSuccess_WhenActivityExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Correr", "#000", true);
        var command = new CreateValueDefinitionCommand(activity.Id, "Distancia", "Number", true, "km", "0", null, userId);
        
        var handler = new CreateValueDefinitionCommandHandler(
            _definitionRepositoryMock.Object, _activityRepositoryMock.Object, _unitOfWorkMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Distancia");
        _definitionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ActivityValueDefinition>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateValueDefinition_ShouldReturnFailure_WhenActivityDoesNotExist()
    {
        // Arrange
        var command = new CreateValueDefinitionCommand(Guid.NewGuid(), "Distancia", "Number", true, "km", "0", null, Guid.NewGuid());
        var handler = new CreateValueDefinitionCommandHandler(
            _definitionRepositoryMock.Object, _activityRepositoryMock.Object, _unitOfWorkMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(command.ActivityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    // --- UPDATE VALUE DEFINITION ---

    [Fact]
    public async Task UpdateValueDefinition_ShouldReturnSuccess_WhenDefinitionExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Correr", "#000", true);
        var definition = new ActivityValueDefinition(activity.Id, "Distancia", "Number", true, "km", "0", null);
        var command = new UpdateValueDefinitionCommand(activity.Id, definition.Id, "Distancia Actualizada", "Number", true, "km", "1", null, userId);
        
        var handler = new UpdateValueDefinitionCommandHandler(
            _definitionRepositoryMock.Object, _activityRepositoryMock.Object, _unitOfWorkMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        _definitionRepositoryMock.Setup(x => x.GetByIdAsync(definition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Distancia Actualizada");
        _definitionRepositoryMock.Verify(x => x.UpdateAsync(definition, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- DELETE VALUE DEFINITION ---

    [Fact]
    public async Task DeleteValueDefinition_ShouldReturnSuccess_WhenDefinitionExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Correr", "#000", true);
        var definition = new ActivityValueDefinition(activity.Id, "Distancia", "Number", true, "km", "0", null);
        var command = new DeleteValueDefinitionCommand(activity.Id, definition.Id, userId);
        
        var handler = new DeleteValueDefinitionCommandHandler(
            _definitionRepositoryMock.Object, _activityRepositoryMock.Object, _unitOfWorkMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        _definitionRepositoryMock.Setup(x => x.GetByIdAsync(definition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);
        _definitionRepositoryMock.Setup(x => x.DeleteAsync(definition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _definitionRepositoryMock.Verify(x => x.DeleteAsync(definition.Id, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- QUERIES ---

    [Fact]
    public async Task GetAllValueDefinitions_ShouldReturnList_WhenActivityExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Correr", "#000", true);
        var definitions = new List<ActivityValueDefinition>
        {
            new ActivityValueDefinition(activity.Id, "Distancia", "Number", true, "km", "0", null)
        };
        var query = new GetAllValueDefinitionsQuery(activity.Id, userId);
        var handler = new GetAllValueDefinitionsQueryHandler(
            _definitionRepositoryMock.Object, _activityRepositoryMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        _definitionRepositoryMock.Setup(x => x.GetAllByActivityAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definitions);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetValueDefinitionById_ShouldReturnDefinition_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Correr", "#000", true);
        var definition = new ActivityValueDefinition(activity.Id, "Distancia", "Number", true, "km", "0", null);
        var query = new GetValueDefinitionByIdQuery(activity.Id, definition.Id, userId);
        var handler = new GetValueDefinitionByIdQueryHandler(
            _definitionRepositoryMock.Object, _activityRepositoryMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        _definitionRepositoryMock.Setup(x => x.GetByIdAsync(definition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(definition.Id);
    }
}
