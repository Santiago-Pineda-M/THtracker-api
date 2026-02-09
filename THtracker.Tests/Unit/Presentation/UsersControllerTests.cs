using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using THtracker.API.Controllers;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.DTOs;

namespace THtracker.Tests.Unit.Presentation;

public class UsersControllerTests
{
    private readonly Mock<GetAllUsersUseCase> _getAllUsersMock;
    private readonly Mock<GetUserByIdUseCase> _getUserByIdMock;
    private readonly Mock<CreateUserUseCase> _createUserMock;
    private readonly Mock<UpdateUserUseCase> _updateUserMock;
    private readonly Mock<DeleteUserUseCase> _deleteUserMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _getAllUsersMock = new Mock<GetAllUsersUseCase>(null!);
        _getUserByIdMock = new Mock<GetUserByIdUseCase>(null!);
        _createUserMock = new Mock<CreateUserUseCase>(null!);
        _updateUserMock = new Mock<UpdateUserUseCase>(null!);
        _deleteUserMock = new Mock<DeleteUserUseCase>(null!);

        _controller = new UsersController(
            _getAllUsersMock.Object,
            _getUserByIdMock.Object,
            _createUserMock.Object,
            _updateUserMock.Object,
            _deleteUserMock.Object
        );
    }

    [Fact]
    public async Task Get_ShouldReturnOk_WithListOfUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto(Guid.NewGuid(), "User 1", "user1@example.com"),
            new UserDto(Guid.NewGuid(), "User 2", "user2@example.com")
        };

        _getAllUsersMock
            .Setup(x => x.ExecuteAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserDto(userId, "John Doe", "john@example.com");

        _getUserByIdMock
            .Setup(x => x.ExecuteAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _getUserByIdMock
            .Setup(x => x.ExecuteAsync(userId))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WithCreatedUser()
    {
        // Arrange
        var createDto = new CreateUserDto("Jane Smith", "jane@example.com");
        var createdUser = new UserDto(Guid.NewGuid(), "Jane Smith", "jane@example.com");

        _createUserMock
            .Setup(x => x.ExecuteAsync(createDto))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(UsersController.GetById));
        createdResult.RouteValues!["id"].Should().Be(createdUser.Id);
        createdResult.Value.Should().BeEquivalentTo(createdUser);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenUserIsUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto("Updated Name", "updated@example.com");
        var updatedUser = new UserDto(userId, "Updated Name", "updated@example.com");

        _updateUserMock
            .Setup(x => x.ExecuteAsync(userId, updateDto))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.Update(userId, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(updatedUser);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto("Updated Name", "updated@example.com");

        _updateUserMock
            .Setup(x => x.ExecuteAsync(userId, updateDto))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.Update(userId, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenUserIsDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _deleteUserMock
            .Setup(x => x.ExecuteAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _deleteUserMock
            .Setup(x => x.ExecuteAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
