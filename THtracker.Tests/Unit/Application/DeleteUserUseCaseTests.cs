using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Interfaces;

namespace THtracker.Tests.Unit.Application;

public class DeleteUserUseCaseTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly DeleteUserUseCase _useCase;

    public DeleteUserUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _useCase = new DeleteUserUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTrue_WhenUserIsDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.DeleteAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(x => x.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.DeleteAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(x => x.DeleteAsync(userId), Times.Once);
    }
}
