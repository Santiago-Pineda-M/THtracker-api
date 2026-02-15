using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using THtracker.Domain.Entities;
using THtracker.Tests.Integration.Presentation.Support;

namespace THtracker.Tests.Integration.Presentation;

[Trait("Category", "Integration")]
[Trait("Layer", "Presentation")]
public class ActivitiesOwnerIntegrationTests
{
    private readonly ApiWebApplicationFactory _factory = new();

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WhenNoAuth()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/activities");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_ShouldReturnForbidden_WhenNotOwner()
    {
        var client = _factory.CreateClient();

        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var service = _factory.Services.GetService(typeof(THtracker.Domain.Interfaces.IActivityRepository));
        var repo = (THtracker.Domain.Interfaces.IActivityRepository)service!;
        var activity = new Activity(ownerId, categoryId, "Act 1", false);
        await repo.AddAsync(activity);

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, otherUserId.ToString());
        var response = await client.GetAsync($"/api/v1/activities/{activity.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOwner()
    {
        var client = _factory.CreateClient();

        var ownerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var service = _factory.Services.GetService(typeof(THtracker.Domain.Interfaces.IActivityRepository));
        var repo = (THtracker.Domain.Interfaces.IActivityRepository)service!;
        var activity = new Activity(ownerId, categoryId, "Act 1", false);
        await repo.AddAsync(activity);

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, ownerId.ToString());
        var response = await client.GetAsync($"/api/v1/activities/{activity.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
