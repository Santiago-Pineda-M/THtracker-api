using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
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
    private readonly Mock<IValidator<RegisterUserRequest>> _validatorMock;
    private readonly RegisterUserUseCase _useCase;

    public RegisterUserUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IValidator<RegisterUserRequest>>();
        
        _useCase = new RegisterUserUseCase(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object,
            _validatorMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRegisterUser_WhenDataIsValid()
    {
        // Arrange
        var request = new RegisterUserRequest("John Doe", "john@example.com", "Password123!");
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(x => x.Hash(request.Password))
            .Returns("hashed_password");

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new RegisterUserRequest("John Doe", "john@example.com", "Password123!");
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conflict");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var request = new RegisterUserRequest("", "invalid", "short");
        var failures = new List<ValidationFailure> { new("Email", "Invalid email") };
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
