using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using THtracker.API.Controllers.v1;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Common;
using Xunit;

namespace THtracker.Tests.Unit.Presentation.Controllers;

[Trait("Category", "Unit")]
[Trait("Layer", "Presentation")]
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
        _createUserMock = new Mock<CreateUserUseCase>(null!, null!, null!);
        _updateUserMock = new Mock<UpdateUserUseCase>(null!, null!, null!);
        _deleteUserMock = new Mock<DeleteUserUseCase>(null!, null!);
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
            new UserDto(Guid.NewGuid(), "User 2", "user2@example.com"),
        };
        _getAllUsersMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(Result.Success<IEnumerable<UserDto>>(users));

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
        _getUserByIdMock.Setup(x => x.ExecuteAsync(userId)).ReturnsAsync(Result.Success(user));

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
        _getUserByIdMock.Setup(x => x.ExecuteAsync(userId)).ReturnsAsync(Result.Failure<UserDto>(new Error("NotFound", "User not found")));

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WithCreatedUser()
    {
        // Arrange
        var request = new CreateUserRequest("Jane Smith", "jane@example.com");
        var createdUser = new UserDto(Guid.NewGuid(), "Jane Smith", "jane@example.com");
        _createUserMock.Setup(x => x.ExecuteAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(createdUser));

        // Act
        var result = await _controller.Create(request);

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
        var request = new UpdateUserRequest("Updated Name", "updated@example.com");
        var updatedUser = new UserDto(userId, "Updated Name", "updated@example.com");
        _updateUserMock.Setup(x => x.ExecuteAsync(userId, request, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(updatedUser));

        // Act
        var result = await _controller.Update(userId, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(updatedUser);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("Updated Name", "updated@example.com");
        _updateUserMock.Setup(x => x.ExecuteAsync(userId, request, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure<UserDto>(new Error("NotFound", "User not found")));

        // Act
        var result = await _controller.Update(userId, request);

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenUserIsDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _deleteUserMock.Setup(x => x.ExecuteAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

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
        _deleteUserMock.Setup(x => x.ExecuteAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure(new Error("NotFound", "User not found")));

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
