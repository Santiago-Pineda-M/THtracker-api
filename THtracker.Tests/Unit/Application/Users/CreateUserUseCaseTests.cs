using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Users;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class CreateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidator<CreateUserRequest>> _validatorMock;
    private readonly CreateUserUseCase _useCase;

    public CreateUserUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IValidator<CreateUserRequest>>();
        
        _useCase = new CreateUserUseCase(
            _repositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateUser_WhenValid()
    {
        // Arrange
        var request = new CreateUserRequest("John Doe", "john@example.com");
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Name.Should().Be(request.Name);
        result.Value.Email.Should().Be(request.Email);
        
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new CreateUserRequest("John Doe", "john@example.com");
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
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
        var request = new CreateUserRequest("", "invalid-email");
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
