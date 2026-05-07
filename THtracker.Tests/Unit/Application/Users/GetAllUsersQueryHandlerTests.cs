using FluentAssertions;
using Moq;
using THtracker.Application.Common;
using THtracker.Application.Features.Users.Queries.GetAllUsers;
using THtracker.Application.Features.Users.Queries.GetUserById;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Users;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class GetAllUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _handler = new GetAllUsersQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedUsers_FromRepository()
    {
        var user1 = new User("User 1", "user1@example.com");
        var user2 = new User("User 2", "user2@example.com");
        var page = new PagedList<User>(new List<User> { user1, user2 }, 2);
        _repositoryMock
            .Setup(x => x.GetPageAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _handler.Handle(new GetAllUsersQuery(1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items[0].Should().BeEquivalentTo(new UserResponse(user1.Id, user1.Name, user1.Email));
        result.Value.Items[1].Should().BeEquivalentTo(new UserResponse(user2.Id, user2.Name, user2.Email));
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        _repositoryMock.Verify(x => x.GetPageAsync(1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldClampPageSize_ToMax()
    {
        _repositoryMock
            .Setup(x => x.GetPageAsync(1, Pagination.MaxPageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<User>(new List<User>(), 0));

        var result = await _handler.Handle(
            new GetAllUsersQuery(1, Pagination.MaxPageSize + 50),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PageSize.Should().Be(Pagination.MaxPageSize);
        _repositoryMock.Verify(x => x.GetPageAsync(1, Pagination.MaxPageSize, It.IsAny<CancellationToken>()), Times.Once);
    }
}
