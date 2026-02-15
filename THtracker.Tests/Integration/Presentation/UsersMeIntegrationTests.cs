using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using THtracker.Domain.Interfaces;
using THtracker.Tests.Integration.Presentation.Support;
using THtracker.Tests.Helpers;
using THtracker.Domain.Entities;

namespace THtracker.Tests.Integration.Presentation;

[Trait("Category", "Integration")]
[Trait("Layer", "Presentation")]
public class UsersMeIntegrationTests
{
    private readonly ApiWebApplicationFactory _factory = new();

    [Fact]
    public async Task GetMe_ShouldReturnUnauthorized_WhenNoAuth()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/users/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_ShouldReturnOk_WhenUserExists()
    {
        var client = _factory.CreateClient();

        var userId = Guid.NewGuid();
        var repo = (IUserRepository)_factory.Services.GetService(typeof(IUserRepository))!;
        var user = UserTestBuilder.WithId("Alice", "alice@example.com", userId);
        await repo.AddAsync(user);

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, userId.ToString());
        var response = await client.GetAsync("/api/v1/users/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateMe_ShouldReturnOk_WhenUserExists()
    {
        var client = _factory.CreateClient();

        var userId = Guid.NewGuid();
        var repo = (IUserRepository)_factory.Services.GetService(typeof(IUserRepository))!;
        var user = UserTestBuilder.WithId("Bob", "bob@example.com", userId);
        await repo.AddAsync(user);

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, userId.ToString());
        var content = new StringContent("{\"name\":\"Bob2\",\"email\":\"bob2@example.com\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PutAsync("/api/v1/users/me", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
