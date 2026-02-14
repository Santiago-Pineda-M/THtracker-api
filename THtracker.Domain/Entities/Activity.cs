namespace THtracker.Domain.Entities;

public class Activity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public bool AllowOverlap { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Activity() { }

    public Activity(Guid userId, Guid categoryId, string name, bool allowOverlap = false)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CategoryId = categoryId;
        Name = name;
        AllowOverlap = allowOverlap;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, bool allowOverlap)
    {
        Name = name;
        AllowOverlap = allowOverlap;
        UpdatedAt = DateTime.UtcNow;
    }
}
