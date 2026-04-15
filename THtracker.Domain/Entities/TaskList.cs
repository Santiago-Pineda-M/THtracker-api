namespace THtracker.Domain.Entities;

public class TaskList
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = null!;

    public string Color { get; private set; } = "#FFFFFF";

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private readonly List<TaskItem> _tasks = new();

    public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();

    private TaskList() { }

    public TaskList(Guid userId, string name, string color = "#FFFFFF")
    {
        this.Id = Guid.NewGuid();
        this.UserId = userId;
        this.Name = name;
        this.Color = color;
        this.CreatedAt = DateTime.UtcNow;
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string color)
    {
        this.Name = name;
        this.Color = color;
        this.UpdatedAt = DateTime.UtcNow;
    }
}
