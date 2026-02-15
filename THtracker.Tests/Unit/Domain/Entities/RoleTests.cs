using FluentAssertions;
using THtracker.Domain.Entities;
using Xunit;

namespace THtracker.Tests.Unit.Domain.Entities;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
public class RoleTests
{
    [Fact]
    public void Constructor_ShouldSetIdAndName()
    {
        var name = "Admin";
        var role = new Role(name);

        role.Id.Should().NotBeEmpty();
        role.Name.Should().Be(name);
        role.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void AddPermission_ShouldAddPermission_WhenNotPresent()
    {
        var role = new Role("Admin");
        var permission = new Permission("CanEdit", "Can edit content");

        role.AddPermission(permission);

        role.Permissions.Should().Contain(permission);
    }

    [Fact]
    public void AddPermission_ShouldNotAddPermission_WhenAlreadyPresent()
    {
        var role = new Role("Admin");
        var permission = new Permission("CanEdit", "Can edit content");
        role.AddPermission(permission);

        role.AddPermission(permission);

        role.Permissions.Count.Should().Be(1);
    }

    [Fact]
    public void RemovePermission_ShouldRemovePermission_WhenPresent()
    {
        var role = new Role("Admin");
        var permission = new Permission("CanEdit", "Can edit content");
        role.AddPermission(permission);

        role.RemovePermission(permission);

        role.Permissions.Should().BeEmpty();
    }
}
