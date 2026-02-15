using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using THtracker.Tests.Integration.Presentation.Support;
using Xunit;

namespace THtracker.Tests.Integration.Presentation;

[Trait("Category", "Integration")]
[Trait("Layer", "Presentation")]
public class AuthControllerIntegrationTests
{
    private readonly ApiWebApplicationFactory _factory = new();

    [Fact]
    public async Task Login_InvalidCredentials_ShouldReturnUnauthorizedOrBadRequest()
    {
        var client = _factory.CreateClient();
        var json = "{\"email\":\"noone@example.com\",\"password\":\"bad\",\"deviceInfo\":\"x\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/auth/login", content);
        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }
}
