using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.DTOs;
using THtracker.Domain.Interfaces;

namespace THtracker.Tests.Unit.Application;

public class GetUserByIdUseCaseTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly GetUserByIdUseCase _useCase;

    public GetUserByIdUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _useCase = new GetUserByIdUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new UserDto(userId, "John Doe", "john@example.com");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);
        _repositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }
}
