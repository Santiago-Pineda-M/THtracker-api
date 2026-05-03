namespace THtracker.Domain.Entities;

public sealed class Activity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Color { get; private set; } = "#FFFFFF";
    public bool AllowOverlap { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Activity() { }

    public Activity(Guid userId, Guid categoryId, string name, string color = "#FFFFFF", bool allowOverlap = false)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CategoryId = categoryId;
        Name = name;
        Color = color;
        AllowOverlap = allowOverlap;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(Guid categoryId, string name, string color, bool allowOverlap)
    {
        CategoryId = categoryId;
        Name = name;
        Color = color;
        AllowOverlap = allowOverlap;
        UpdatedAt = DateTime.UtcNow;
    }
}
