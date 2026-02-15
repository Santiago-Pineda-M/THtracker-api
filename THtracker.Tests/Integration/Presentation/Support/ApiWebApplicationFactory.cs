using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using THtracker.Application.Constants;
using THtracker.Application.Interfaces;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Tests.Integration.Presentation.Support;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace authentication with Test scheme
            services
                .AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    options => { }
                );

            // Remove default repositories and register in-memory stubs
            ReplaceService<IUserRepository>(services, new InMemoryUserRepository());
            ReplaceService<IActivityRepository>(services, new InMemoryActivityRepository());
            ReplaceService<ICategoryRepository>(services, new InMemoryCategoryRepository());
            ReplaceService<IRoleRepository>(services, new InMemoryRoleRepository());
            ReplaceService<IUserRoleRepository>(services, new InMemoryUserRoleRepository());
            ReplaceService<IActivityLogRepository>(services, new InMemoryActivityLogRepository());
            ReplaceService<IActivityValueDefinitionRepository>(
                services,
                new InMemoryActivityValueDefinitionRepository()
            );

            // Disable data seeding at startup
            ReplaceService<IDataSeeder>(services, new NoOpSeeder());
        });
    }

    private static void ReplaceService<TService>(IServiceCollection services, object instance)
        where TService : class
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(TService)).ToList();
        foreach (var d in descriptors)
            services.Remove(d);
        services.AddSingleton(typeof(TService), instance);
    }
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.AsEnumerable());

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

    public Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(_users.Any(u => u.Email == email));

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = _users.RemoveAll(u => u.Id == id) > 0;
        return Task.FromResult(removed);
    }
}

public class InMemoryActivityRepository : IActivityRepository
{
    private readonly Dictionary<Guid, Activity> _activities = new();

    public Task<IEnumerable<Activity>> GetAllByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(_activities.Values.Where(a => a.UserId == userId).AsEnumerable());

    public Task<Activity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_activities.TryGetValue(id, out var a) ? a : null);

    public Task AddAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        _activities[activity.Id] = activity;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        _activities[activity.Id] = activity;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_activities.Remove(id));
}

public class NoOpSeeder : IDataSeeder
{
    public Task SeedAsync(
        string defaultAdminEmail,
        string defaultAdminPassword,
        string defaultAdminName,
        CancellationToken cancellationToken = default
    ) => Task.CompletedTask;
}

public class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly Dictionary<Guid, Category> _categories = new();

    public Task<IEnumerable<Category>> GetAllByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(_categories.Values.Where(c => c.UserId == userId).AsEnumerable());

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_categories.TryGetValue(id, out var c) ? c : null);

    public Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        _categories[category.Id] = category;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _categories[category.Id] = category;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_categories.Remove(id));
}

public class InMemoryRoleRepository : IRoleRepository
{
    private readonly Dictionary<Guid, Role> _roles = new();

    public InMemoryRoleRepository()
    {
        var admin = new Role(DefaultRoles.Admin);
        _roles[admin.Id] = admin;
        var user = new Role("User");
        _roles[user.Id] = user;
    }

    public Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_roles.Values.AsEnumerable());

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_roles.TryGetValue(id, out var r) ? r : null);

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult(
            _roles.Values.FirstOrDefault(r =>
                r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            )
        );

    public Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        _roles[role.Id] = role;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        _roles[role.Id] = role;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_roles.Remove(id));
}

public class InMemoryUserRoleRepository : IUserRoleRepository
{
    private readonly Dictionary<Guid, HashSet<Guid>> _userRoles = new();
    private readonly InMemoryRoleRepository _roleRepo = new();

    public Task<IEnumerable<Role>> GetRolesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (_userRoles.TryGetValue(userId, out var set))
        {
            var roles = set.Select(id => _roleRepo.GetByIdAsync(id, cancellationToken).Result!)
                .ToList();
            return Task.FromResult<IEnumerable<Role>>(roles);
        }
        return Task.FromResult(Enumerable.Empty<Role>());
    }

    public Task<IEnumerable<User>> GetUsersByRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(Enumerable.Empty<User>());

    public Task AddRoleToUserAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        if (!_userRoles.TryGetValue(userId, out var set))
        {
            set = new HashSet<Guid>();
            _userRoles[userId] = set;
        }
        set.Add(roleId);
        return Task.CompletedTask;
    }

    public Task RemoveRoleFromUserAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        if (_userRoles.TryGetValue(userId, out var set))
            set.Remove(roleId);
        return Task.CompletedTask;
    }

    public Task<bool> IsRoleAssignedAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        var result = _userRoles.TryGetValue(userId, out var set) && set.Contains(roleId);
        return Task.FromResult(result);
    }
}

public class InMemoryActivityLogRepository : IActivityLogRepository
{
    private readonly Dictionary<Guid, ActivityLog> _logs = new();

    public Task<IEnumerable<ActivityLog>> GetAllByActivityAsync(
        Guid activityId,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(_logs.Values.Where(l => l.ActivityId == activityId).AsEnumerable());

    public Task<ActivityLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(_logs.TryGetValue(id, out var l) ? l : null);

    public Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        _logs[log.Id] = log;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        _logs[log.Id] = log;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ActivityLog>> GetActiveLogsByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(_logs.Values.Where(l => !l.EndedAt.HasValue).AsEnumerable());

    public Task<IEnumerable<ActivityLog>> GetOverlappingLogsAsync(
        Guid userId,
        DateTime start,
        DateTime end,
        Guid? excludeLogId = null,
        CancellationToken cancellationToken = default
    ) =>
        Task.FromResult(
            _logs
                .Values.Where(l =>
                    (excludeLogId == null || l.Id != excludeLogId)
                    && (l.EndedAt ?? DateTime.UtcNow) > start
                    && l.StartedAt < end
                )
                .AsEnumerable()
        );

    public Task<IEnumerable<ActivityLog>> GetLogsInPeriodWithDetailsAsync(
        Guid userId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default
    ) => GetOverlappingLogsAsync(userId, start, end, null, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_logs.Remove(id));
}

public class InMemoryActivityValueDefinitionRepository : IActivityValueDefinitionRepository
{
    private readonly Dictionary<Guid, ActivityValueDefinition> _defs = new();

    public Task<IEnumerable<ActivityValueDefinition>> GetAllByActivityAsync(
        Guid activityId,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(_defs.Values.Where(d => d.ActivityId == activityId).AsEnumerable());

    public Task<ActivityValueDefinition?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(_defs.TryGetValue(id, out var d) ? d : null);

    public Task AddAsync(
        ActivityValueDefinition valueDefinition,
        CancellationToken cancellationToken = default
    )
    {
        _defs[valueDefinition.Id] = valueDefinition;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        ActivityValueDefinition valueDefinition,
        CancellationToken cancellationToken = default
    )
    {
        _defs[valueDefinition.Id] = valueDefinition;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_defs.Remove(id));
}
