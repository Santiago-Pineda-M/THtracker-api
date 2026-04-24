namespace THtracker.Tests.Unit.Domain.Entities;

using FluentAssertions;
using THtracker.Domain.Entities;
using Xunit;

public class TaskItemTests
{
    [Fact]
    public void Constructor_ShouldEnsureDueDateIsUtc()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var localDate = new DateTime(2023, 10, 27, 10, 0, 0, DateTimeKind.Unspecified);

        // Act
        var task = new TaskItem(taskListId, userId, "Test task", localDate);

        // Assert
        task.DueDate.Should().NotBeNull();
        task.DueDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
        task.DueDate.Value.Year.Should().Be(2023);
    }

    [Fact]
    public void UpdateContent_ShouldEnsureDueDateIsUtc()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var task = new TaskItem(taskListId, userId, "Original content", null);
        var localDate = new DateTime(2023, 10, 27, 10, 0, 0, DateTimeKind.Unspecified);

        // Act
        task.UpdateContent("Updated content", localDate);

        // Assert
        task.DueDate.Should().NotBeNull();
        task.DueDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }
}
