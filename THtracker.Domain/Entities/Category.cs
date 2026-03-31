namespace THtracker.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Color { get; private set; } = "#FFFFFF";
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Category() { }

    public Category(Guid userId, string name, string color = "#FFFFFF")
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        Color = color; 
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name, string color)
    {
        Name = name;
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }
}
