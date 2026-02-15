using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using THtracker.Domain.Entities;
using THtracker.Tests.Integration.Presentation.Support;

namespace THtracker.Tests.Integration.Presentation;

[Trait("Category", "Integration")]
[Trait("Layer", "Presentation")]
public class CategoriesControllerIntegrationTests
{
    private readonly ApiWebApplicationFactory _factory = new();

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WhenNoAuth()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/categories");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_ShouldReturnForbidden_WhenNotOwner()
    {
        var client = _factory.CreateClient();

        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var repo = _factory.Services.GetService(typeof(THtracker.Domain.Interfaces.ICategoryRepository)) as THtracker.Domain.Interfaces.ICategoryRepository;
        var category = new Category(ownerId, "Cat 1");
        await repo!.AddAsync(category);

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, otherUserId.ToString());
        var response = await client.GetAsync($"/api/v1/categories/{category.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOwner()
    {
        var client = _factory.CreateClient();

        var ownerId = Guid.NewGuid();
        var repo = _factory.Services.GetService(typeof(THtracker.Domain.Interfaces.ICategoryRepository)) as THtracker.Domain.Interfaces.ICategoryRepository;
        var category = new Category(ownerId, "Cat 1");
        await repo!.AddAsync(category);

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, ownerId.ToString());
        var response = await client.GetAsync($"/api/v1/categories/{category.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
