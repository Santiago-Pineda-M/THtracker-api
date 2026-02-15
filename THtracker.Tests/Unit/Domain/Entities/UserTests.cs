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
        user.SecurityStamp.Should().NotBeNullOrEmpty();
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Update_ShouldUpdateNameAndEmail_AndNotChangeId()
    {
        var user = new User("Original Name", "original@example.com");
        var originalId = user.Id;
        var newName = "Updated Name";
        var newEmail = "updated@example.com";

        user.Update(newName, newEmail);

        user.Name.Should().Be(newName);
        user.Email.Should().Be(newEmail);
        user.Id.Should().Be(originalId);
    }

    [Fact]
    public void SetPassword_ShouldUpdatePasswordHash_AndUpdateSecurityStamp()
    {
        var user = new User("User", "user@test.com");
        var originalStamp = user.SecurityStamp;
        var newHash = "hashed_password";

        user.SetPassword(newHash);

        user.PasswordHash.Should().Be(newHash);
        user.SecurityStamp.Should().NotBe(originalStamp);
    }

    [Fact]
    public void AddRole_ShouldAddRole_WhenRoleIsNotPresent()
    {
        var user = new User("User", "user@test.com");
        var role = new Role("Admin");

        user.AddRole(role);

        user.Roles.Should().ContainSingle(r => r.Id == role.Id);
    }

    [Fact]
    public void AddRole_ShouldNotAddRole_WhenRoleIsAlreadyPresent()
    {
        var user = new User("User", "user@test.com");
        var role = new Role("Admin");
        user.AddRole(role);

        user.AddRole(role);

        user.Roles.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveRole_ShouldRemoveRole_WhenRoleIsPresent()
    {
        var user = new User("User", "user@test.com");
        var role = new Role("Admin");
        user.AddRole(role);

        user.RemoveRole(role);

        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void AddLogin_ShouldAddLogin_WhenLoginIsNotPresent()
    {
        var user = new User("User", "user@test.com");
        var provider = "Google";
        var key = "google-key-123";

        user.AddLogin(provider, key, "Google User");

        user.Logins.Should().ContainSingle(l => l.LoginProvider == provider && l.ProviderKey == key);
    }
}
