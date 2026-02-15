using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using THtracker.Domain.Entities;
using THtracker.Tests.Integration.Presentation.Support;

namespace THtracker.Tests.Integration.Presentation
{
    [Trait("Category", "Integration")]
    [Trait("Layer", "Presentation")]
    public class ActivityValueDefinitionsIntegrationTests : IDisposable
    {
        private readonly ApiWebApplicationFactory _factory = new();

        [Fact]
        public async Task GetAllShouldReturnUnauthorizedWhenNoAuth()
        {
            HttpClient client = _factory.CreateClient();
            Guid activityId = Guid.NewGuid();
            HttpResponseMessage response = await client.GetAsync(
                $"/api/v1/activities/{activityId}/definitions"
            );
            _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllShouldReturnOkWhenOwner()
        {
            HttpClient client = _factory.CreateClient();
            Guid ownerId = Guid.NewGuid();
            Guid activityId = Guid.NewGuid();

            using var scope = _factory.Services.CreateScope();
            Domain.Interfaces.IActivityRepository activityRepo =
                (Domain.Interfaces.IActivityRepository)
                    scope.ServiceProvider.GetService(
                        typeof(Domain.Interfaces.IActivityRepository)
                    )!;

            Activity activity = new(ownerId, Guid.NewGuid(), "Act X", false);
            typeof(Activity).GetProperty("Id")!.SetValue(activity, activityId);
            await activityRepo.AddAsync(activity);

            // No definiciones añadidas: la respuesta debe ser 200 con lista vacía

            client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, ownerId.ToString());
            HttpResponseMessage response = await client.GetAsync(
                $"/api/v1/activities/{activityId}/definitions"
            );
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
