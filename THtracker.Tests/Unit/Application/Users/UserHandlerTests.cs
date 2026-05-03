using Moq;
using FluentAssertions;
using THtracker.Application.Features.Users.Commands.CreateUser;
using THtracker.Application.Features.Users.Commands.UpdateUser;
using THtracker.Application.Features.Users.Commands.DeleteUser;
using THtracker.Application.Features.Users.Queries.GetUserById;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Application.Interfaces;
using MediatR;

namespace THtracker.Tests.Unit.Application.Users;

public class UserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public UserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    // --- CREATE USER ---

    [Fact]
    public async Task CreateUser_ShouldReturnSuccess_WhenEmailIsUnique()
    {
        // Arrange
        var command = new CreateUserCommand("Test User", "test@example.com", "Password123!");
        var handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _passwordHasherMock.Object, _unitOfWorkMock.Object);

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(x => x.Hash(command.Password))
            .Returns("hashed_password");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(command.Email);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnFailure_WhenEmailExists()
    {
        // Arrange
        var command = new CreateUserCommand("Test User", "test@example.com", "Password123!");
        var handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _passwordHasherMock.Object, _unitOfWorkMock.Object);

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conflict");
    }

    // --- UPDATE USER ---

    [Fact]
    public async Task UpdateUser_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var user = new User("Old Name", "old@example.com");
        var command = new UpdateUserCommand(user.Id, "New Name", "new@example.com");
        var handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _unitOfWorkMock.Object);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        user.Name.Should().Be("New Name");
        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- DELETE USER ---

    [Fact]
    public async Task DeleteUser_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var handler = new DeleteUserCommandHandler(_userRepositoryMock.Object, _unitOfWorkMock.Object);

        _userRepositoryMock.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- QUERIES ---

    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = new User("Test User", "test@example.com");
        var query = new GetUserByIdQuery(user.Id);
        var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(user.Id);
    }
}
