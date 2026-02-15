using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Users;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class DeleteUserUseCaseTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteUserUseCase _useCase;

    public DeleteUserUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new DeleteUserUseCase(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenUserIsDeleted()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }
}
