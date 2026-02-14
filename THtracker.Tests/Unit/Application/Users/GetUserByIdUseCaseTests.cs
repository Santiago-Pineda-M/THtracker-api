using FluentAssertions;
using Moq;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Interfaces;
using THtracker.Tests.Helpers;
using Xunit;

namespace THtracker.Tests.Unit.Application.Users;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
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
        var userId = Guid.NewGuid();
        var user = UserTestBuilder.WithId("John Doe", "john@example.com", userId);
        var expectedDto = new UserDto(userId, "John Doe", "john@example.com");
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _useCase.ExecuteAsync(userId);

        result.Should().BeEquivalentTo(expectedDto);
        _repositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((THtracker.Domain.Entities.User?)null);

        var result = await _useCase.ExecuteAsync(userId);

        result.Should().BeNull();
        _repositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }
}
