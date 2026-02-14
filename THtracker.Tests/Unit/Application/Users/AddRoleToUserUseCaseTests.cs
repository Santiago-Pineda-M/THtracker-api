using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Users;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class AddRoleToUserUseCaseTests
{
    private readonly Mock<IUserRoleRepository> _repositoryMock;
    private readonly AddRoleToUserUseCase _useCase;

    public AddRoleToUserUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRoleRepository>();
        _useCase = new AddRoleToUserUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAddRole_WhenNotAlreadyAssigned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.IsRoleAssignedAsync(userId, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _useCase.ExecuteAsync(userId, roleId);

        // Assert
        _repositoryMock.Verify(x => x.AddRoleToUserAsync(userId, roleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenRoleAlreadyAssigned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.IsRoleAssignedAsync(userId, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(userId, roleId);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Role is already assigned to this user.");
    }
}
