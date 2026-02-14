using FluentAssertions;
using THtracker.Domain.Entities;
using Xunit;

namespace THtracker.Tests.Unit.Domain.Entities;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
public class UserTests
{
    [Fact]
    public void Constructor_ShouldCreateUser_WithValidData()
    {
        var name = "John Doe";
        var email = "john@example.com";

        var user = new User(name, email);

        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueId_ForEachUser()
    {
        var user1 = new User("User 1", "user1@example.com");
        var user2 = new User("User 2", "user2@example.com");

        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void Update_ShouldUpdateNameAndEmail_WithValidData()
    {
        var user = new User("Original Name", "original@example.com");
        var newName = "Updated Name";
        var newEmail = "updated@example.com";

        user.Update(newName, newEmail);

        user.Name.Should().Be(newName);
        user.Email.Should().Be(newEmail);
    }

    [Fact]
    public void Update_ShouldNotChangeId_WhenUpdatingUser()
    {
        var user = new User("Original Name", "original@example.com");
        var originalId = user.Id;

        user.Update("New Name", "new@example.com");

        user.Id.Should().Be(originalId);
    }
}
