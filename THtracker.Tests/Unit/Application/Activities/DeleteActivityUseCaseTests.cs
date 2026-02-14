using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Activities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Activities;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class DeleteActivityUseCaseTests
{
    private readonly Mock<IActivityRepository> _repositoryMock;
    private readonly DeleteActivityUseCase _useCase;

    public DeleteActivityUseCaseTests()
    {
        _repositoryMock = new Mock<IActivityRepository>();
        _useCase = new DeleteActivityUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTrue_WhenDeleted()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.DeleteAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(activityId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFalse_WhenNotDeleted()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.DeleteAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _useCase.ExecuteAsync(activityId);

        // Assert
        result.Should().BeFalse();
    }
}
