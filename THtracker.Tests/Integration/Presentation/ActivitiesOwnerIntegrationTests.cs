using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using THtracker.Domain.Entities;
using THtracker.Tests.Integration.Presentation.Support;

namespace THtracker.Tests.Integration.Presentation;

[Trait("Category", "Integration")]
[Trait("Layer", "Presentation")]
public class ActivitiesOwnerIntegrationTests : IDisposable
{
    private readonly ApiWebApplicationFactory _factory = new();

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenAuthenticated()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, userId.ToString());
        var response = await client.GetAsync("/api/v1/activities");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenNoAuth()
    {
        var client = _factory.CreateClient();
        var body = new StringContent(
            "{\"categoryId\":\"00000000-0000-0000-0000-000000000001\",\"name\":\"Act 1\",\"allowOverlap\":false}",
            Encoding.UTF8,
            "application/json");
        var response = await client.PostAsync("/api/v1/activities", body);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var categoryRepo = _factory.Services.GetService(typeof(Domain.Interfaces.ICategoryRepository)) as Domain.Interfaces.ICategoryRepository;
        var category = new Category(userId, "Cat 1", "#FF0000");
        await categoryRepo!.AddAsync(category);

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, userId.ToString());
        var body = new StringContent(
            $"{{\"categoryId\":\"{category.Id}\",\"name\":\"Act 1\",\"allowOverlap\":false}}",
            Encoding.UTF8,
            "application/json");
        var response = await client.PostAsync("/api/v1/activities", body);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

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
        var activity = new Activity(ownerId, categoryId, "Act 1", "#FF0000", false);
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
        var activity = new Activity(ownerId, categoryId, "Act 1", "#FF0000", false);
        await repo.AddAsync(activity);

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, ownerId.ToString());
        var response = await client.GetAsync($"/api/v1/activities/{activity.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public void Dispose() => _factory.Dispose();
}
