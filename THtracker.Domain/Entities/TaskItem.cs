namespace THtracker.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }

    public Guid TaskListId { get; private set; }

    public Guid UserId { get; private set; }

    public string Content { get; private set; } = null!;

    public bool IsCompleted { get; private set; }

    public DateTime? DueDate { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public TaskList? TaskList { get; private set; }

    private TaskItem() { }

    public TaskItem(Guid taskListId, Guid userId, string content, DateTime? dueDate = null)
    {
        this.Id = Guid.NewGuid();
        this.TaskListId = taskListId;
        this.UserId = userId;
        this.Content = content;
        this.IsCompleted = false;
        this.DueDate = dueDate;
        this.CreatedAt = DateTime.UtcNow;
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        this.IsCompleted = true;
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPending()
    {
        this.IsCompleted = false;
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string content, DateTime? dueDate)
    {
        this.Content = content;
        this.DueDate = dueDate;
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleCompletion()
    {
        this.IsCompleted = !this.IsCompleted;
        this.UpdatedAt = DateTime.UtcNow;
    }
}
