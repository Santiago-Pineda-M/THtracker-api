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
    private readonly CreateRoleUseCase _useCase;

    public CreateRoleUseCaseTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _useCase = new CreateRoleUseCase(_roleRepositoryMock.Object);
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
        result.Should().NotBeEmpty();
        _roleRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenRoleNameAlreadyExists()
    {
        // Arrange
        var roleName = "ExistingRole";
        _roleRepositoryMock.Setup(x => x.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role(roleName));

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(roleName);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Role '{roleName}' already exists.");
    }
}
