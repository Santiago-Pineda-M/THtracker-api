using FluentAssertions;
using THtracker.Domain.Entities;
using Xunit;

namespace THtracker.Tests.Unit.Domain;

public class ActivityLogTests
{
    [Fact]
    public void GetDurationInInterval_ShouldReturnCorrectDuration_WhenLogIsWithinInterval()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var start = new DateTime(2024, 1, 1, 10, 0, 0);
        var end = new DateTime(2024, 1, 1, 12, 0, 0); // 2 hours
        var log = new ActivityLog(activityId, start);
        log.Stop(end);

        var intervalStart = new DateTime(2024, 1, 1, 0, 0, 0);
        var intervalEnd = new DateTime(2024, 1, 1, 23, 59, 59);

        // Act
        var duration = log.GetDurationInInterval(intervalStart, intervalEnd);

        // Assert
        duration.TotalHours.Should().Be(2);
    }

    [Fact]
    public void GetDurationInInterval_ShouldReturnCorrectDuration_WhenLogCrossesMidnight()
    {
        // Arrange: Log from 10 PM to 2 AM next day (4 hours total)
        var activityId = Guid.NewGuid();
        var logStart = new DateTime(2024, 1, 1, 22, 0, 0);
        var logEnd = new DateTime(2024, 1, 2, 2, 0, 0);
        var log = new ActivityLog(activityId, logStart);
        log.Stop(logEnd);

        // Day 1 Interval
        var day1Start = new DateTime(2024, 1, 1, 0, 0, 0);
        var day1End = new DateTime(2024, 1, 2, 0, 0, 0);

        // Day 2 Interval
        var day2Start = new DateTime(2024, 1, 2, 0, 0, 0);
        var day2End = new DateTime(2024, 1, 3, 0, 0, 0);

        // Act
        var durationDay1 = log.GetDurationInInterval(day1Start, day1End);
        var durationDay2 = log.GetDurationInInterval(day2Start, day2End);

        // Assert
        durationDay1.TotalHours.Should().Be(2); // 10 PM to Midnight
        durationDay2.TotalHours.Should().Be(2); // Midnight to 2 AM
    }

    [Fact]
    public void GetDurationInInterval_ShouldReturnZeroMinutes_WhenLogIsOutsideInterval()
    {
        // Arrange
        var log = new ActivityLog(Guid.NewGuid(), new DateTime(2024, 1, 1, 10, 0, 0));
        log.Stop(new DateTime(2024, 1, 1, 11, 0, 0));

        var intervalStart = new DateTime(2024, 1, 2, 0, 0, 0);
        var intervalEnd = new DateTime(2024, 1, 2, 23, 59, 59);

        // Act
        var duration = log.GetDurationInInterval(intervalStart, intervalEnd);

        // Assert
        duration.TotalMinutes.Should().Be(0);
    }

    [Fact]
    public void Stop_ShouldThrowException_WhenEndBeforeStart()
    {
        // Arrange
        var log = new ActivityLog(Guid.NewGuid(), new DateTime(2024, 1, 1, 12, 0, 0));
        var invalidEnd = new DateTime(2024, 1, 1, 10, 0, 0);

        // Act
        Action act = () => log.Stop(invalidEnd);

        // Assert
        act.Should().Throw<Exception>().WithMessage("La fecha de fin no puede ser anterior a la de inicio.");
    }
}
