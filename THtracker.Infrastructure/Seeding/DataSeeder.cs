using Microsoft.EntityFrameworkCore;
using THtracker.Application.Constants;
using THtracker.Application.Interfaces;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Seeding;

/// <summary>
/// Implementación del puerto IDataSeeder: crea roles por defecto y cuenta de administrador.
/// Pertenece a Infrastructure (persistencia); no expone tipos de Infrastructure a capas externas.
/// </summary>
public class DataSeeder : IDataSeeder
{

    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DataSeeder(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc />
    public async Task SeedAsync(
        string defaultAdminEmail,
        string defaultAdminPassword,
        string defaultAdminName = "Administrator",
        CancellationToken cancellationToken = default
    )
    {
        await EnsureRolesAsync(cancellationToken);
        await EnsureAdminUserAsync(
            defaultAdminEmail,
            defaultAdminPassword,
            defaultAdminName,
            cancellationToken
        );
    }

    private async Task EnsureRolesAsync(CancellationToken cancellationToken)
    {
        var existingNames = await _context.Roles
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        if (!existingNames.Contains(DefaultRoles.Admin))
        {
            _context.Roles.Add(new Role(DefaultRoles.Admin));
        }

        if (!existingNames.Contains(DefaultRoles.User))
        {
            _context.Roles.Add(new Role(DefaultRoles.User));
        }

        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureAdminUserAsync(
        string email,
        string password,
        string name,
        CancellationToken cancellationToken
    )
    {
        var emailTrimmed = email.Trim();
        var existingUser = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == emailTrimmed, cancellationToken);

        if (existingUser != null)
        {
            // Admin ya existe: asegurar que tenga rol en user_roles (para que el JWT incluya roles)
            var existingAdminRole = await _context.Roles.FirstOrDefaultAsync(
                r => r.Name == DefaultRoles.Admin,
                cancellationToken
            );
            if (existingAdminRole != null && !existingUser.Roles.Any(r => r.Id == existingAdminRole.Id))
            {
                existingUser.AddRole(existingAdminRole);
                await _context.SaveChangesAsync(cancellationToken);
            }
            return;
        }

        var adminRole = await _context.Roles.FirstOrDefaultAsync(
            r => r.Name == DefaultRoles.Admin,
            cancellationToken
        );
        if (adminRole == null)
        {
            return;
        }

        var user = new User(name, emailTrimmed);
        user.SetPassword(_passwordHasher.Hash(password));
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Asignar rol Admin: UserRoles (API/GetUserRoles) y user_roles (User.Roles → JWT en login)
        user.AddRole(adminRole);
        _context.UserRoles.Add(new UserRole(user.Id, adminRole.Id));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
