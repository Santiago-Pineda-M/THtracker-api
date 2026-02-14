using FluentAssertions;
using Moq;
using THtracker.Application.DTOs.Activities;
using THtracker.Application.UseCases.Activities;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Activities;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class UpdateActivityUseCaseTests
{
    private readonly Mock<IActivityRepository> _repositoryMock;
    private readonly UpdateActivityUseCase _useCase;

    public UpdateActivityUseCaseTests()
    {
        _repositoryMock = new Mock<IActivityRepository>();
        _useCase = new UpdateActivityUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateActivity_WhenExists()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), "Old Name", false);
        var request = new UpdateActivityRequest("Updated Name", true);

        _repositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await _useCase.ExecuteAsync(activityId, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.AllowOverlap.Should().BeTrue();
        _repositoryMock.Verify(x => x.UpdateAsync(activity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var request = new UpdateActivityRequest("Updated Name", true);

        _repositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        // Act
        var result = await _useCase.ExecuteAsync(activityId, request);

        // Assert
        result.Should().BeNull();
    }
}
