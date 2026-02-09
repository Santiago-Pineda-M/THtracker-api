using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.DTOs;
using THtracker.Domain.Interfaces;

namespace THtracker.Tests.Unit.Application;

public class UpdateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly UpdateUserUseCase _useCase;

    public UpdateUserUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _useCase = new UpdateUserUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto("Updated Name", "updated@example.com");
        var expectedUser = new UserDto(userId, "Updated Name", "updated@example.com");

        _repositoryMock
            .Setup(x => x.UpdateAsync(userId, updateDto))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _useCase.ExecuteAsync(userId, updateDto);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);
        _repositoryMock.Verify(x => x.UpdateAsync(userId, updateDto), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto("Updated Name", "updated@example.com");

        _repositoryMock
            .Setup(x => x.UpdateAsync(userId, updateDto))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId, updateDto);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(x => x.UpdateAsync(userId, updateDto), Times.Once);
    }
}
