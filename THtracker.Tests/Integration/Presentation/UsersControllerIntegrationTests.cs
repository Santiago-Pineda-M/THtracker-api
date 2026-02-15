using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using THtracker.Application.Constants;
using THtracker.Tests.Integration.Presentation.Support;

namespace THtracker.Tests.Integration.Presentation;

[Trait("Category", "Integration")]
[Trait("Layer", "Presentation")]
public class UsersControllerIntegrationTests
{
    private readonly ApiWebApplicationFactory _factory = new();

    [Fact]
    public async Task Get_ShouldReturnUnauthorized_WhenNoAuth()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/users");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_ShouldReturnForbidden_WhenAuthenticatedWithoutAdminRole()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderRoles, "User");
        var response = await client.GetAsync("/api/v1/users");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Get_ShouldReturnOk_WhenAuthenticatedWithAdminRole()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderRoles, DefaultRoles.Admin);
        var response = await client.GetAsync("/api/v1/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
