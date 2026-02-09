using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.DTOs;
using THtracker.Domain.Interfaces;

namespace THtracker.Tests.Unit.Application;

public class GetAllUsersUseCaseTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly GetAllUsersUseCase _useCase;

    public GetAllUsersUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _useCase = new GetAllUsersUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAllUsers_FromRepository()
    {
        // Arrange
        var expectedUsers = new List<UserDto>
        {
            new UserDto(Guid.NewGuid(), "User 1", "user1@example.com"),
            new UserDto(Guid.NewGuid(), "User 2", "user2@example.com")
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedUsers);
        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<UserDto>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }
}
