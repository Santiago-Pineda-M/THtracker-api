namespace THtracker.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    private readonly List<Permission> _permissions = new();
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    private Role() { }

    public Role(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    public void AddPermission(Permission permission)
    {
        if (!_permissions.Contains(permission))
        {
            _permissions.Add(permission);
        }
    }

    public void RemovePermission(Permission permission)
    {
        _permissions.Remove(permission);
    }
}
