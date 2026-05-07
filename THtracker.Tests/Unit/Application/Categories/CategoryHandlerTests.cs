using Moq;
using FluentAssertions;
using THtracker.Application.Features.Categories;
using THtracker.Application.Features.Categories.Commands.CreateCategory;
using THtracker.Application.Features.Categories.Commands.UpdateCategory;
using THtracker.Application.Features.Categories.Commands.DeleteCategory;
using THtracker.Application.Features.Categories.Queries.GetAllCategories;
using THtracker.Application.Features.Categories.Queries.GetCategoryById;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Tests.Unit.Application.Categories;

public class CategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public CategoryHandlerTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCategory_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateCategoryCommand("Trabajo", "Proyectos laborales", "#1A1A2E", userId);
        var handler = new CreateCategoryCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Trabajo");
        result.Value.UserId.Should().Be(userId);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateCategory_ShouldReturnSuccess_WhenCategoryBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Viejo Nombre", null, "#FFF");
        var command = new UpdateCategoryCommand(category.Id, "Nuevo Nombre", "Nueva descripción", "#000", userId);
        var handler = new UpdateCategoryCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);

        _repositoryMock.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Nuevo Nombre");
        _repositoryMock.Verify(x => x.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnFailure_WhenCategoryBelongsToDifferentUser()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var attackerUserId = Guid.NewGuid();
        var category = new Category(ownerUserId, "Mi Categoría", null, "#FFF");
        var command = new UpdateCategoryCommand(category.Id, "Hackeado", null, "#000", attackerUserId);
        var handler = new UpdateCategoryCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);

        _repositoryMock.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        // Arrange
        var command = new UpdateCategoryCommand(Guid.NewGuid(), "Nombre", null, "#FFF", Guid.NewGuid());
        var handler = new UpdateCategoryCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    // ─── DELETE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteCategory_ShouldReturnSuccess_WhenCategoryBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var category = new Category(userId, "A Eliminar", null, "#FFF");
        var command = new DeleteCategoryCommand(category.Id, userId);
        var handler = new DeleteCategoryCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);

        _repositoryMock.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _repositoryMock.Setup(x => x.DeleteAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new DeleteCategoryCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    // ─── QUERIES ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllCategories_ShouldReturnUserCategories()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categories = new List<Category>
        {
            new Category(userId, "Trabajo", null, "#111"),
            new Category(userId, "Personal", null, "#222"),
        };
        var query = new GetAllCategoriesQuery(userId);
        var handler = new GetAllCategoriesQueryHandler(_repositoryMock.Object);

        _repositoryMock.Setup(x => x.GetPageByUserAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Category>(categories, categories.Count));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().AllSatisfy(c => c.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnEmpty_WhenUserHasNoCategories()
    {
        // Arrange
        var query = new GetAllCategoriesQuery(Guid.NewGuid());
        var handler = new GetAllCategoriesQueryHandler(_repositoryMock.Object);

        _repositoryMock.Setup(x => x.GetPageByUserAsync(query.UserId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Category>([], 0));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnCategory_WhenItBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Trabajo", "Mi trabajo", "#1A1A2E");
        var query = new GetCategoryByIdQuery(category.Id, userId);
        var handler = new GetCategoryByIdQueryHandler(_repositoryMock.Object);

        _repositoryMock.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(category.Id);
        result.Value.Name.Should().Be("Trabajo");
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        // Arrange
        var query = new GetCategoryByIdQuery(Guid.NewGuid(), Guid.NewGuid());
        var handler = new GetCategoryByIdQueryHandler(_repositoryMock.Object);

        _repositoryMock.Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnFailure_WhenCategoryBelongsToDifferentUser()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var category = new Category(ownerUserId, "Privada", null, "#FFF");
        var query = new GetCategoryByIdQuery(category.Id, Guid.NewGuid()); // otro usuario
        var handler = new GetCategoryByIdQueryHandler(_repositoryMock.Object);

        _repositoryMock.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }
}
