using Moq;
using FluentAssertions;
using THtracker.Application.Features.UserSessions.Commands.RevokeSession;
using THtracker.Application.Features.UserSessions.Queries.GetUserSessions;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using MediatR;

namespace THtracker.Tests.Unit.Application.UserSessions;

public class UserSessionHandlerTests
{
    private readonly Mock<IUserSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public UserSessionHandlerTests()
    {
        _sessionRepositoryMock = new Mock<IUserSessionRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    // --- REVOKE SESSION ---

    [Fact]
    public async Task RevokeSession_ShouldReturnSuccess_WhenSessionExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = new UserSession(userId, "token123", DateTime.UtcNow.AddDays(1), "Chrome", "127.0.0.1");
        var refreshToken = new RefreshToken("token123", DateTime.UtcNow.AddDays(1), "127.0.0.1", "Chrome", userId);
        var command = new RevokeSessionCommand(session.Id, userId);
        
        var handler = new RevokeSessionCommandHandler(
            _sessionRepositoryMock.Object, _refreshTokenRepositoryMock.Object, _unitOfWorkMock.Object);

        _sessionRepositoryMock.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(session.SessionToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.IsActive.Should().BeFalse();
        refreshToken.IsRevoked.Should().BeTrue();
        _sessionRepositoryMock.Verify(x => x.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.UpdateAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeSession_ShouldReturnFailure_WhenSessionDoesNotExist()
    {
        // Arrange
        var command = new RevokeSessionCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new RevokeSessionCommandHandler(
            _sessionRepositoryMock.Object, _refreshTokenRepositoryMock.Object, _unitOfWorkMock.Object);

        _sessionRepositoryMock.Setup(x => x.GetByIdAsync(command.SessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task RevokeSession_ShouldReturnFailure_WhenDifferentUser()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var session = new UserSession(ownerId, "token123", DateTime.UtcNow.AddDays(1), "Chrome", "127.0.0.1");
        var command = new RevokeSessionCommand(session.Id, attackerId);
        
        var handler = new RevokeSessionCommandHandler(
            _sessionRepositoryMock.Object, _refreshTokenRepositoryMock.Object, _unitOfWorkMock.Object);

        _sessionRepositoryMock.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    // --- QUERIES ---

    [Fact]
    public async Task GetUserSessions_ShouldReturnList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessions = new List<UserSession>
        {
            new UserSession(userId, "token123", DateTime.UtcNow.AddDays(1), "Chrome", "127.0.0.1")
        };
        var query = new GetUserSessionsQuery(userId);
        var handler = new GetUserSessionsQueryHandler(_sessionRepositoryMock.Object);

        _sessionRepositoryMock.Setup(x => x.GetActiveByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}
