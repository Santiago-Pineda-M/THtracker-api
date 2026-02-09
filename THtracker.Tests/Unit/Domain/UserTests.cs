using FluentAssertions;
using THtracker.Domain.Entities;

namespace THtracker.Tests.Unit.Domain;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldCreateUser_WithValidData()
    {
        // Arrange
        var name = "John Doe";
        var email = "john@example.com";

        // Act
        var user = new User(name, email);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueId_ForEachUser()
    {
        // Arrange & Act
        var user1 = new User("User 1", "user1@example.com");
        var user2 = new User("User 2", "user2@example.com");

        // Assert
        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void Update_ShouldUpdateNameAndEmail_WithValidData()
    {
        // Arrange
        var user = new User("Original Name", "original@example.com");
        var newName = "Updated Name";
        var newEmail = "updated@example.com";

        // Act
        user.Update(newName, newEmail);

        // Assert
        user.Name.Should().Be(newName);
        user.Email.Should().Be(newEmail);
    }

    [Fact]
    public void Update_ShouldNotChangeId_WhenUpdatingUser()
    {
        // Arrange
        var user = new User("Original Name", "original@example.com");
        var originalId = user.Id;

        // Act
        user.Update("New Name", "new@example.com");

        // Assert
        user.Id.Should().Be(originalId);
    }
}
