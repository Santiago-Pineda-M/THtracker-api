using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;

namespace THtracker.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserLogin> UserLogins => Set<UserLogin>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<ActivityLogValue> ActivityLogValues => Set<ActivityLogValue>();
    public DbSet<ActivityValueDefinition> ActivityValueDefinitions =>
        Set<ActivityValueDefinition>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<TaskList> TaskLists => Set<TaskList>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
