using System.Net;
using System.Text;
using FluentAssertions;
using THtracker.Application.Constants;
using THtracker.Domain.Entities;
using THtracker.Tests.Integration.Presentation.Support;

namespace THtracker.Tests.Integration.Presentation;

[Trait("Category", "Integration")]
[Trait("Layer", "Presentation")]
public class UserRolesControllerIntegrationTests : IDisposable
{
    private readonly ApiWebApplicationFactory _factory = new();

    [Fact]
    public async Task GetRoles_ShouldReturnUnauthorized_WhenNoAuth()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/v1/users/{userId}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRoles_ShouldReturnForbidden_WhenNotAdmin()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var regularUserId = Guid.NewGuid();

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, regularUserId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderRoles, "User");

        var response = await client.GetAsync($"/api/v1/users/{userId}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRoles_ShouldReturnOk_WhenAdmin()
    {
        var client = _factory.CreateClient();
        var adminId = Guid.NewGuid();

        var userRepo = _factory.Services.GetService(typeof(Domain.Interfaces.IUserRepository))
            as Domain.Interfaces.IUserRepository;
        var userRoleRepo = _factory.Services.GetService(typeof(Domain.Interfaces.IUserRoleRepository))
            as Domain.Interfaces.IUserRoleRepository;
        var roleRepo = _factory.Services.GetService(typeof(Domain.Interfaces.IRoleRepository))
            as Domain.Interfaces.IRoleRepository;
        var user = new User("Test User", "test@example.com");
        await userRepo!.AddAsync(user);

        var adminRole = (await roleRepo!.GetAllAsync()).First(r => r.Name == DefaultRoles.Admin);
        await userRoleRepo!.AddRoleToUserAsync(user.Id, adminRole.Id);

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, adminId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderRoles, "Admin");

        var response = await client.GetAsync($"/api/v1/users/{user.Id}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddRole_ShouldReturnUnauthorized_WhenNoAuth()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var response = await client.PostAsync($"/api/v1/users/{userId}/roles/{roleId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddRole_ShouldReturnForbidden_WhenNotAdmin()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var regularUserId = Guid.NewGuid();

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, regularUserId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderRoles, "User");

        var response = await client.PostAsync($"/api/v1/users/{userId}/roles/{roleId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveRole_ShouldReturnUnauthorized_WhenNoAuth()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var response = await client.DeleteAsync($"/api/v1/users/{userId}/roles/{roleId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveRole_ShouldReturnForbidden_WhenNotAdmin()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var regularUserId = Guid.NewGuid();

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, regularUserId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderRoles, "User");

        var response = await client.DeleteAsync($"/api/v1/users/{userId}/roles/{roleId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetRoles_ShouldReturnUnauthorized_WhenNoAuth()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var content = new StringContent(
            "{\"roleNames\":[\"Admin\"]}",
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PutAsync($"/api/v1/users/{userId}/roles", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetRoles_ShouldReturnForbidden_WhenNotAdmin()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var regularUserId = Guid.NewGuid();
        var content = new StringContent(
            "{\"roleNames\":[\"Admin\"]}",
            Encoding.UTF8,
            "application/json"
        );

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, regularUserId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderRoles, "User");

        var response = await client.PutAsync($"/api/v1/users/{userId}/roles", content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
