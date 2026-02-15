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
        // Arrange
        var userId = Guid.NewGuid();
        var user = UserTestBuilder.WithId("John Doe", "john@example.com", userId);
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(userId);
        result.Value.Name.Should().Be("John Doe");
        _repositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((THtracker.Domain.Entities.User?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
        _repositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }
}
