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
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddRoleToUserUseCase _useCase;

    public AddRoleToUserUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRoleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new AddRoleToUserUseCase(_repositoryMock.Object, _unitOfWorkMock.Object);
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
        var result = await _useCase.ExecuteAsync(userId, roleId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.AddRoleToUserAsync(userId, roleId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenRoleAlreadyAssigned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.IsRoleAssignedAsync(userId, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(userId, roleId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conflict");
    }
}
