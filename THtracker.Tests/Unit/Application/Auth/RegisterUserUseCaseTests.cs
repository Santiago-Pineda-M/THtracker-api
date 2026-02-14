using FluentAssertions;
using Moq;
using THtracker.Application.DTOs.Auth;
using THtracker.Application.Interfaces;
using THtracker.Application.UseCases.Auth;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Auth;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class RegisterUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterUserUseCase _useCase;

    public RegisterUserUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new RegisterUserUseCase(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRegisterUser_WhenDataIsValid()
    {
        // Arrange
        var request = new RegisterUserRequest("John Doe", "john@example.com", "Password123!");
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(x => x.Hash(request.Password))
            .Returns("hashed_password");

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeEmpty();
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new RegisterUserRequest("John Doe", "john@example.com", "Password123!");
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User with this email already exists.");
    }
}
