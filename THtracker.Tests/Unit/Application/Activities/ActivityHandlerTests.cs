using Moq;
using FluentAssertions;
using THtracker.Application.Features.Activities;
using THtracker.Application.Features.Activities.Commands.CreateActivity;
using THtracker.Application.Features.Activities.Commands.UpdateActivity;
using THtracker.Application.Features.Activities.Commands.DeleteActivity;
using THtracker.Application.Features.Activities.Queries.GetAllActivities;
using THtracker.Application.Features.Activities.Queries.GetActivityById;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Tests.Unit.Application.Activities;

public class ActivityHandlerTests
{
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public ActivityHandlerTests()
    {
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateActivity_ShouldReturnSuccess_WhenCategoryBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Trabajo", null, "#FFF");
        var command = new CreateActivityCommand(category.Id, "Lectura", "#111", false, userId);
        var handler = new CreateActivityCommandHandler(
            _activityRepositoryMock.Object, _categoryRepositoryMock.Object, _unitOfWorkMock.Object);

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Lectura");
        result.Value.UserId.Should().Be(userId);
        _activityRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        // Arrange
        var command = new CreateActivityCommand(Guid.NewGuid(), "Lectura", "#111", false, Guid.NewGuid());
        var handler = new CreateActivityCommandHandler(
            _activityRepositoryMock.Object, _categoryRepositoryMock.Object, _unitOfWorkMock.Object);

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
        _activityRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnFailure_WhenCategoryBelongsToDifferentUser()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var attackerUserId = Guid.NewGuid();
        var category = new Category(ownerUserId, "Privada", null, "#FFF");
        var command = new CreateActivityCommand(category.Id, "Espía", "#000", false, attackerUserId);
        var handler = new CreateActivityCommandHandler(
            _activityRepositoryMock.Object, _categoryRepositoryMock.Object, _unitOfWorkMock.Object);

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateActivity_ShouldReturnSuccess_WhenActivityBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var activity = new Activity(userId, categoryId, "Nombre Viejo", "#FFF", false);
        var command = new UpdateActivityCommand(activity.Id, categoryId, "Nombre Nuevo", "#000", true, userId);
        var handler = new UpdateActivityCommandHandler(
            _activityRepositoryMock.Object, _categoryRepositoryMock.Object, _unitOfWorkMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Nombre Nuevo");
        _activityRepositoryMock.Verify(x => x.UpdateAsync(activity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateActivity_ShouldReturnFailure_WhenActivityBelongsToDifferentUser()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var activity = new Activity(ownerUserId, Guid.NewGuid(), "Actividad", "#FFF", false);
        var command = new UpdateActivityCommand(activity.Id, activity.CategoryId, "Hackeado", "#000", false, Guid.NewGuid());
        var handler = new UpdateActivityCommandHandler(
            _activityRepositoryMock.Object, _categoryRepositoryMock.Object, _unitOfWorkMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
        _activityRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateActivity_ShouldReturnFailure_WhenNewCategoryBelongsToDifferentUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var originalCategoryId = Guid.NewGuid();
        var foreignCategoryId = Guid.NewGuid();
        var activity = new Activity(userId, originalCategoryId, "Actividad", "#FFF", false);
        var foreignCategory = new Category(Guid.NewGuid(), "Ajena", null, "#FFF"); // categoria de otro user

        var command = new UpdateActivityCommand(activity.Id, foreignCategoryId, "Actividad", "#FFF", false, userId);
        var handler = new UpdateActivityCommandHandler(
            _activityRepositoryMock.Object, _categoryRepositoryMock.Object, _unitOfWorkMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(foreignCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(foreignCategory);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }

    // ─── DELETE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteActivity_ShouldReturnSuccess_WhenActivityBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "A Eliminar", "#FFF", false);
        var command = new DeleteActivityCommand(activity.Id, userId);
        var handler = new DeleteActivityCommandHandler(_activityRepositoryMock.Object, _unitOfWorkMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        _activityRepositoryMock.Setup(x => x.DeleteAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteActivity_ShouldReturnFailure_WhenActivityDoesNotExist()
    {
        // Arrange
        var command = new DeleteActivityCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new DeleteActivityCommandHandler(_activityRepositoryMock.Object, _unitOfWorkMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    // ─── QUERIES ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllActivities_ShouldReturnUserActivities()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var activities = new List<Activity>
        {
            new Activity(userId, categoryId, "Lectura", "#111", false),
            new Activity(userId, categoryId, "Ejercicio", "#222", false),
        };
        var query = new GetAllActivitiesQuery(userId);
        var handler = new GetAllActivitiesQueryHandler(_activityRepositoryMock.Object);

        _activityRepositoryMock.Setup(x => x.GetPageByUserAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Activity>(activities, activities.Count));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().AllSatisfy(a => a.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task GetAllActivities_ShouldReturnEmpty_WhenUserHasNoActivities()
    {
        // Arrange
        var query = new GetAllActivitiesQuery(Guid.NewGuid());
        var handler = new GetAllActivitiesQueryHandler(_activityRepositoryMock.Object);

        _activityRepositoryMock.Setup(x => x.GetPageByUserAsync(query.UserId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Activity>([], 0));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActivityById_ShouldReturnActivity_WhenItBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Meditación", "#AAA", false);
        var query = new GetActivityByIdQuery(activity.Id, userId);
        var handler = new GetActivityByIdQueryHandler(_activityRepositoryMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(activity.Id);
        result.Value.Name.Should().Be("Meditación");
    }

    [Fact]
    public async Task GetActivityById_ShouldReturnFailure_WhenActivityDoesNotExist()
    {
        // Arrange
        var query = new GetActivityByIdQuery(Guid.NewGuid(), Guid.NewGuid());
        var handler = new GetActivityByIdQueryHandler(_activityRepositoryMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task GetActivityById_ShouldReturnFailure_WhenActivityBelongsToDifferentUser()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var activity = new Activity(ownerUserId, Guid.NewGuid(), "Privada", "#FFF", false);
        var query = new GetActivityByIdQuery(activity.Id, Guid.NewGuid()); // attacker
        var handler = new GetActivityByIdQueryHandler(_activityRepositoryMock.Object);

        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }
}
