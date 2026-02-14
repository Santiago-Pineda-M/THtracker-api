using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using THtracker.API.Controllers;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Users;
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
        _createUserMock = new Mock<CreateUserUseCase>(null!);
        _updateUserMock = new Mock<UpdateUserUseCase>(null!);
        _deleteUserMock = new Mock<DeleteUserUseCase>(null!);
        _controller = new UsersController(
            _getAllUsersMock.Object,
            _getUserByIdMock.Object,
            _createUserMock.Object,
            _updateUserMock.Object,
            _deleteUserMock.Object);
    }

    [Fact]
    public async Task Get_ShouldReturnOk_WithListOfUsers()
    {
        var users = new List<UserDto>
        {
            new UserDto(Guid.NewGuid(), "User 1", "user1@example.com"),
            new UserDto(Guid.NewGuid(), "User 2", "user2@example.com")
        };
        _getAllUsersMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(users);

        var result = await _controller.Get();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = new UserDto(userId, "John Doe", "john@example.com");
        _getUserByIdMock.Setup(x => x.ExecuteAsync(userId)).ReturnsAsync(user);

        var result = await _controller.GetById(userId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _getUserByIdMock.Setup(x => x.ExecuteAsync(userId)).ReturnsAsync((UserDto?)null);

        var result = await _controller.GetById(userId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WithCreatedUser()
    {
        var request = new CreateUserRequest("Jane Smith", "jane@example.com");
        var createdUser = new UserDto(Guid.NewGuid(), "Jane Smith", "jane@example.com");
        _createUserMock.Setup(x => x.ExecuteAsync(request)).ReturnsAsync(createdUser);

        var result = await _controller.Create(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(UsersController.GetById));
        createdResult.RouteValues!["id"].Should().Be(createdUser.Id);
        createdResult.Value.Should().BeEquivalentTo(createdUser);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenUserIsUpdated()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("Updated Name", "updated@example.com");
        var updatedUser = new UserDto(userId, "Updated Name", "updated@example.com");
        _updateUserMock.Setup(x => x.ExecuteAsync(userId, request)).ReturnsAsync(updatedUser);

        var result = await _controller.Update(userId, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(updatedUser);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("Updated Name", "updated@example.com");
        _updateUserMock.Setup(x => x.ExecuteAsync(userId, request)).ReturnsAsync((UserDto?)null);

        var result = await _controller.Update(userId, request);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenUserIsDeleted()
    {
        var userId = Guid.NewGuid();
        _deleteUserMock.Setup(x => x.ExecuteAsync(userId)).ReturnsAsync(true);

        var result = await _controller.Delete(userId);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _deleteUserMock.Setup(x => x.ExecuteAsync(userId)).ReturnsAsync(false);

        var result = await _controller.Delete(userId);

        result.Should().BeOfType<NotFoundResult>();
    }
}
