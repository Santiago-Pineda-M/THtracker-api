using FluentAssertions;
using Moq;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Tests.Helpers;
using Xunit;

namespace THtracker.Tests.Unit.Application.Users;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
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
        var userId = Guid.NewGuid();
        var user = UserTestBuilder.WithId("Original Name", "original@example.com", userId);
        var request = new UpdateUserRequest("Updated Name", "updated@example.com");
        var expectedDto = new UserDto(userId, "Updated Name", "updated@example.com");
 
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _repositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
 
        var result = await _useCase.ExecuteAsync(userId, request);
 
        result.Should().BeEquivalentTo(expectedDto);
    }
 
    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        var userId = Guid.NewGuid();
        var user = UserTestBuilder.WithId("Original Name", "original@example.com", userId);
        var request = new UpdateUserRequest("New Name", "existing@example.com");
 
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _repositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
 
        Func<Task> act = async () => await _useCase.ExecuteAsync(userId, request);
 
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User with this email already exists.");
    }
 
    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("Updated Name", "updated@example.com");
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);
 
        var result = await _useCase.ExecuteAsync(userId, request);
 
        result.Should().BeNull();
    }
}
