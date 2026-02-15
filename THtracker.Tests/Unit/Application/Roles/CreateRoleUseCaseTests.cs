using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Roles;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Roles;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class CreateRoleUseCaseTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateRoleUseCase _useCase;

    public CreateRoleUseCaseTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new CreateRoleUseCase(_roleRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateRole_WhenNameIsUnique()
    {
        // Arrange
        var roleName = "NewRole";
        _roleRepositoryMock.Setup(x => x.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _useCase.ExecuteAsync(roleName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _roleRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenRoleNameAlreadyExists()
    {
        // Arrange
        var roleName = "ExistingRole";
        _roleRepositoryMock.Setup(x => x.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role(roleName));

        // Act
        var result = await _useCase.ExecuteAsync(roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conflict");
    }
}
