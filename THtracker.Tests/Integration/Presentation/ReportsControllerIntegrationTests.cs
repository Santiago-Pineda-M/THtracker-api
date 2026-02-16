using System.Net;
using FluentAssertions;
using THtracker.Domain.Entities;
using THtracker.Tests.Integration.Presentation.Support;

namespace THtracker.Tests.Integration.Presentation;

[Trait("Category", "Integration")]
[Trait("Layer", "Presentation")]
public class ReportsControllerIntegrationTests : IDisposable
{
    private readonly ApiWebApplicationFactory _factory = new();

    [Fact]
    public async Task GetActivityReport_ShouldReturnUnauthorized_WhenNoAuth()
    {
        var client = _factory.CreateClient();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var response = await client.GetAsync(
            $"/api/v1/reports/activities?startDate={startDate:O}&endDate={endDate:O}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetActivityReport_ShouldReturnOk_WhenAuthenticated()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, userId.ToString());

        var response = await client.GetAsync(
            $"/api/v1/reports/activities?startDate={startDate:O}&endDate={endDate:O}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetActivityReport_ShouldReturnBadRequest_WhenInvalidDateRange()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-7);

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, userId.ToString());

        var response = await client.GetAsync(
            $"/api/v1/reports/activities?startDate={startDate:O}&endDate={endDate:O}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
