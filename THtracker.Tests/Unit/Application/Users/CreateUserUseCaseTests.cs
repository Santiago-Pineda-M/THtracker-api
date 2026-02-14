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
public class CreateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly CreateUserUseCase _useCase;

    public CreateUserUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _useCase = new CreateUserUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateUser_AndReturnUserDto_WithNameAndEmailFromRequest()
    {
        var request = new CreateUserRequest("John Doe", "john@example.com");
        _repositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be(request.Name);
        result.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        var request = new CreateUserRequest("John Doe", "john@example.com");
        _repositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Func<Task> act = async () => await _useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User with this email already exists.");
    }
}
