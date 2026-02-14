namespace THtracker.Domain.Entities;

public class Permission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    private Permission() { }

    public Permission(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }
}
