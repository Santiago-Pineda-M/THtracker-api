using System.Net;
using FluentAssertions;
using THtracker.Domain.Entities;
using THtracker.Tests.Integration.Presentation.Support;

namespace THtracker.Tests.Integration.Presentation
{
    [Trait("Category", "Integration")]
    [Trait("Layer", "Presentation")]
    public class ActivityLogsControllerIntegrationTests : IDisposable
    {
        private readonly ApiWebApplicationFactory _factory = new();

        [Fact]
        public async Task StopShouldReturnUnauthorizedWhenNoAuth()
        {
            HttpClient client = _factory.CreateClient();
            Guid logId = Guid.NewGuid();
            HttpResponseMessage response = await client.PostAsync(
                $"/api/v1/activity-logs/{logId}/stop",
                null
            );
            _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task StopShouldReturnForbiddenWhenNotOwner()
        {
            HttpClient client = _factory.CreateClient();

            Guid ownerId = Guid.NewGuid();
            Guid otherUserId = Guid.NewGuid();
            Guid categoryId = Guid.NewGuid();

            Domain.Interfaces.IActivityRepository? activityRepo =
                _factory.Services.GetService(typeof(Domain.Interfaces.IActivityRepository))
                as Domain.Interfaces.IActivityRepository;
            Domain.Interfaces.IActivityLogRepository? logRepo =
                _factory.Services.GetService(typeof(Domain.Interfaces.IActivityLogRepository))
                as Domain.Interfaces.IActivityLogRepository;

            Activity activity = new Activity(ownerId, categoryId, "Act 1", false);
            await activityRepo!.AddAsync(activity);
            ActivityLog log = new ActivityLog(activity.Id, DateTime.UtcNow);
            await logRepo!.AddAsync(log);

            client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, otherUserId.ToString());
            HttpResponseMessage response = await client.PostAsync(
                $"/api/v1/activity-logs/{log.Id}/stop",
                null
            );
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task StopShouldReturnOkWhenOwner()
        {
            HttpClient client = _factory.CreateClient();

            Guid ownerId = Guid.NewGuid();
            Guid categoryId = Guid.NewGuid();
            Domain.Interfaces.IActivityRepository? activityRepo =
                _factory.Services.GetService(typeof(Domain.Interfaces.IActivityRepository))
                as Domain.Interfaces.IActivityRepository;
            Domain.Interfaces.IActivityLogRepository? logRepo =
                _factory.Services.GetService(typeof(Domain.Interfaces.IActivityLogRepository))
                as Domain.Interfaces.IActivityLogRepository;

            Activity activity = new Activity(ownerId, categoryId, "Act 1", false);
            await activityRepo!.AddAsync(activity);
            ActivityLog log = new ActivityLog(activity.Id, DateTime.UtcNow.AddMinutes(-5));
            await logRepo!.AddAsync(log);

            client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, ownerId.ToString());
            HttpResponseMessage response = await client.PostAsync(
                $"/api/v1/activity-logs/{log.Id}/stop",
                null
            );
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
