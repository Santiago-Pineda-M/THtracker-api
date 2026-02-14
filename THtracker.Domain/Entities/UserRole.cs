using System;

namespace THtracker.Domain.Entities;

public class UserRole
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    public User? User { get; set; }
    public Role? Role { get; set; }

    private UserRole() { }

    public UserRole(Guid userId, Guid roleId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RoleId = roleId;
    }
}
