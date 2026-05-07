using Moq;
using THtracker.Application.Features.Auth;
using THtracker.Application.Features.Auth.Login;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Application.Interfaces;
using FluentAssertions;

namespace THtracker.Tests.Unit.Application.Auth.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUserSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IClientIpProvider> _clientIpProviderMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _sessionRepositoryMock = new Mock<IUserSessionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _clientIpProviderMock = new Mock<IClientIpProvider>();
        _clientIpProviderMock.Setup(x => x.GetClientIpAddress()).Returns("127.0.0.1");

        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtProviderMock.Object,
            _refreshTokenRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _clientIpProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!", "Chrome");
        var user = new User("Test User", command.Email);
        user.SetPassword("hashed_password");
        
        var refreshToken = new THtracker.Domain.Entities.RefreshToken("refresh_token", DateTime.UtcNow.AddDays(7), "127.0.0.1", command.DeviceInfo, user.Id);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _passwordHasherMock.Setup(x => x.Verify(command.Password, user.PasswordHash!))
            .Returns(true);

        _jwtProviderMock.Setup(x => x.GenerateRefreshToken(user, "127.0.0.1", command.DeviceInfo))
            .Returns(refreshToken);

        _jwtProviderMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<Guid>()))
            .Returns("access_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
        
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<THtracker.Domain.Entities.RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordIsIncorrect()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "WrongPassword", "Chrome");
        var user = new User("Test User", command.Email);
        user.SetPassword("hashed_password");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _passwordHasherMock.Setup(x => x.Verify(command.Password, user.PasswordHash!))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Unauthorized");
    }
}
