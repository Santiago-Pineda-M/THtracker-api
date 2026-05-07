using Moq;
using THtracker.Application.Features.Auth;
using THtracker.Application.Features.Auth.RefreshToken;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Application.Interfaces;
using FluentAssertions;

namespace THtracker.Tests.Unit.Application.Auth.RefreshToken;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUserSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IClientIpProvider> _clientIpProviderMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _sessionRepositoryMock = new Mock<IUserSessionRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _clientIpProviderMock = new Mock<IClientIpProvider>();
        _clientIpProviderMock.Setup(x => x.GetClientIpAddress()).Returns("127.0.0.1");

        _handler = new RefreshTokenCommandHandler(
            _refreshTokenRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _jwtProviderMock.Object,
            _unitOfWorkMock.Object,
            _clientIpProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid_old_token", "Chrome");
        var userId = Guid.NewGuid();
        var user = new User("Test User", "test@example.com");
        
        var oldRefreshToken = new THtracker.Domain.Entities.RefreshToken(command.RefreshToken, DateTime.UtcNow.AddDays(1), "127.0.0.1", "Chrome", userId);
        var session = new UserSession(userId, command.RefreshToken, oldRefreshToken.ExpiryDate, "Chrome", "127.0.0.1");
        
        var newRefreshToken = new THtracker.Domain.Entities.RefreshToken("new_token", DateTime.UtcNow.AddDays(7), "127.0.0.1", command.DeviceInfo, userId);

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldRefreshToken);

        _sessionRepositoryMock.Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtProviderMock.Setup(x => x.GenerateRefreshToken(user, "127.0.0.1", command.DeviceInfo))
            .Returns(newRefreshToken);

        _jwtProviderMock.Setup(x => x.GenerateAccessToken(user, session.Id))
            .Returns("new_access_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RefreshToken.Should().Be("new_token");
        result.Value.AccessToken.Should().Be("new_access_token");

        // Verificar Rotación: el viejo se revoca
        oldRefreshToken.IsRevoked.Should().BeTrue();
        
        // Verificar Actualización de Sesión: se actualiza el token en la misma sesión
        session.SessionToken.Should().Be("new_token");

        _refreshTokenRepositoryMock.Verify(x => x.UpdateAsync(oldRefreshToken, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(newRefreshToken, It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepositoryMock.Verify(x => x.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTokenIsExpired()
    {
        // Arrange
        var command = new RefreshTokenCommand("expired_token", "Chrome");
        // Creamos un token que expira en el pasado
        var oldRefreshToken = new THtracker.Domain.Entities.RefreshToken(command.RefreshToken, DateTime.UtcNow.AddDays(-1), "127.0.0.1", "Chrome", Guid.NewGuid());

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Unauthorized");
    }
}
