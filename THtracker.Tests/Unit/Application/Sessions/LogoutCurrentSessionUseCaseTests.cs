namespace THtracker.Tests.Unit.Application.Sessions;

using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Sessions;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class LogoutCurrentSessionUseCaseTests
{
    private readonly Mock<IUserSessionRepository> sessionRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly LogoutCurrentSessionUseCase useCase;

    public LogoutCurrentSessionUseCaseTests()
    {
        this.sessionRepositoryMock = new Mock<IUserSessionRepository>();
        this.refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.useCase = new LogoutCurrentSessionUseCase(
            this.sessionRepositoryMock.Object,
            this.refreshTokenRepositoryMock.Object,
            this.unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogout_WhenSessionExistsAndUserOwnsIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = new UserSession(
            userId,
            "token123",
            DateTime.UtcNow.AddDays(7),
            "Chrome on Windows",
            "192.168.1.1"
        );
        this.sessionRepositoryMock.Setup(x =>
                x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(session);
        this.refreshTokenRepositoryMock.Setup(x =>
                x.GetByTokenAsync(session.SessionToken, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, session.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.IsActive.Should().BeFalse();
        this.unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRevokeRefreshToken_WhenSessionHasActiveRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = new UserSession(
            userId,
            "token123",
            DateTime.UtcNow.AddDays(7),
            "Chrome on Windows",
            "192.168.1.1"
        );
        var refreshToken = new RefreshToken(
            session.SessionToken,
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1",
            "Test Device",
            session.UserId
        );
        this.sessionRepositoryMock.Setup(x =>
                x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(session);
        this.refreshTokenRepositoryMock.Setup(x =>
                x.GetByTokenAsync(session.SessionToken, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(refreshToken);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, session.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenSessionNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        this.sessionRepositoryMock.Setup(x =>
                x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((UserSession?)null);

        // Act
        var result = await this.useCase.ExecuteAsync(userId, nonExistentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenUserDoesNotOwnSession()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var session = new UserSession(
            ownerId,
            "token123",
            DateTime.UtcNow.AddDays(7),
            "Chrome on Windows",
            "192.168.1.1"
        );
        this.sessionRepositoryMock.Setup(x =>
                x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(session);

        // Act
        var result = await this.useCase.ExecuteAsync(otherUserId, session.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden");
    }
}
