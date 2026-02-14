using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Activities;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Activities;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class GetAllActivitiesUseCaseTests
{
    private readonly Mock<IActivityRepository> _repositoryMock;
    private readonly GetAllActivitiesUseCase _useCase;

    public GetAllActivitiesUseCaseTests()
    {
        _repositoryMock = new Mock<IActivityRepository>();
        _useCase = new GetAllActivitiesUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnActivities_ForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activities = new List<Activity>
        {
            new Activity(userId, Guid.NewGuid(), "Act 1", false),
            new Activity(userId, Guid.NewGuid(), "Act 2", true)
        };

        _repositoryMock.Setup(x => x.GetAllByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Name == "Act 1");
        result.Should().Contain(a => a.Name == "Act 2");
    }
}
