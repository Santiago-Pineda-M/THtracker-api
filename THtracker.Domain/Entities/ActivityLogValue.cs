namespace THtracker.Domain.Entities;

public class ActivityLogValue
{
    public Guid Id { get; private set; }
    public Guid ActivityLogId { get; private set; }
    public Guid ValueDefinitionId { get; private set; }
    public string Value { get; private set; }

    private ActivityLogValue() { }

    public ActivityLogValue(Guid activityLogId, Guid valueDefinitionId, string value)
    {
        Id = Guid.NewGuid();
        ActivityLogId = activityLogId;
        ValueDefinitionId = valueDefinitionId;
        Value = value;
    }
}
