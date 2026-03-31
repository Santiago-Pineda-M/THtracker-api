using FluentAssertions;
using Moq;
using THtracker.Application.DTOs.Reports;
using THtracker.Application.UseCases.Reports;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using Xunit;

namespace THtracker.Tests.Unit.Application.Reports;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class GetUserActivityReportUseCaseTests
{
    private readonly Mock<IActivityLogRepository> _logRepositoryMock;
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly GetUserActivityReportUseCase _useCase;

    public GetUserActivityReportUseCaseTests()
    {
        _logRepositoryMock = new Mock<IActivityLogRepository>();
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _useCase = new GetUserActivityReportUseCase(_logRepositoryMock.Object, _activityRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateDurationsCorrectly_WithinInterval()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateTime(2024, 2, 1, 0, 0, 0); // Feb 1st
        var endDate = new DateTime(2024, 2, 1, 23, 59, 59); // End of Feb 1st
        var request = new ActivityReportRequest(startDate, endDate);

        var activityId = Guid.NewGuid();
        var activity = new Activity(userId, Guid.NewGuid(), "Work", "#FF0000", false);
        _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId, It.IsAny<CancellationToken>())).ReturnsAsync(activity);

        // Log that started before and ended during the interval
        var log1 = new ActivityLog(activityId, startDate.AddHours(-1));
        log1.Stop(startDate.AddHours(1)); // 1 hour outside, 1 hour inside

        // Log completely inside
        var log2 = new ActivityLog(activityId, startDate.AddHours(5));
        log2.Stop(startDate.AddHours(6)); // 1 hour inside

        // Log that started during and ended after
        var log3 = new ActivityLog(activityId, endDate.AddHours(-1));
        log3.Stop(endDate.AddHours(1)); // 1 hour inside, 1 hour outside

        var logs = new List<ActivityLog> { log1, log2, log3 };
        _logRepositoryMock.Setup(x => x.GetLogsInPeriodWithDetailsAsync(userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalDurationMinutes.Should().Be(180); // 60 + 60 + 60
        result.Value.Activities.Should().HaveCount(1);
        result.Value.Activities.First().TotalDurationMinutes.Should().Be(180);
    }
}
