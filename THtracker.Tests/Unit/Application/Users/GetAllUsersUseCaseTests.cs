using FluentAssertions;
using Moq;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Users;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
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
        var user1 = new User("User 1", "user1@example.com");
        var user2 = new User("User 2", "user2@example.com");
        var users = new List<User> { user1, user2 };
        var expectedDtos = new List<UserDto>
        {
            new UserDto(user1.Id, user1.Name, user1.Email),
            new UserDto(user2.Id, user2.Name, user2.Email)
        };
        _repositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(users);

        var result = await _useCase.ExecuteAsync();

        result.Should().BeEquivalentTo(expectedDtos);
        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        _repositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<User>());

        var result = await _useCase.ExecuteAsync();

        result.Should().BeEmpty();
        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }
}
