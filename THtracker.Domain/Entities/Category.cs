namespace THtracker.Domain.Entities;

public sealed class Category
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Color { get; private set; } = "#FFFFFF";
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Category() { }

    public Category(Guid userId, string name, string? description, string color = "#FFFFFF")
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        Description = description;
        Color = color; 
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description, string color)
    {
        Name = name;
        Description = description;
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }
}
