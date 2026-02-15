using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Tests.Helpers;
using Xunit;

namespace THtracker.Tests.Unit.Application.Users;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class UpdateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidator<UpdateUserRequest>> _validatorMock;
    private readonly UpdateUserUseCase _useCase;

    public UpdateUserUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IValidator<UpdateUserRequest>>();
        
        _useCase = new UpdateUserUseCase(
            _repositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = UserTestBuilder.WithId("Original Name", "original@example.com", userId);
        var request = new UpdateUserRequest("Updated Name", "updated@example.com");

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _repositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");
        result.Value.Email.Should().Be("updated@example.com");
        
        _repositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = UserTestBuilder.WithId("Original Name", "original@example.com", userId);
        var request = new UpdateUserRequest("New Name", "existing@example.com");

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _repositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conflict");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("Updated Name", "updated@example.com");
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
            
        _repositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var request = new UpdateUserRequest("", "invalid");
        var failures = new List<ValidationFailure> { new("Name", "Name is required") };
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var result = await _useCase.ExecuteAsync(Guid.NewGuid(), request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
