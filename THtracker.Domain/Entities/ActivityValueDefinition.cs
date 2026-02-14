namespace THtracker.Domain.Entities;

public class ActivityValueDefinition
{
    public Guid Id { get; private set; }
    public Guid ActivityId { get; private set; }
    public string Name { get; private set; }
    public string ValueType { get; private set; }
    public bool IsRequired { get; private set; }
    public string? Unit { get; private set; }
    public string? MinValue { get; private set; }
    public string? MaxValue { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ActivityValueDefinition() { }

    public ActivityValueDefinition(
        Guid activityId,
        string name,
        string valueType,
        bool isRequired = false,
        string? unit = null,
        string? minValue = null,
        string? maxValue = null
    )
    {
        Id = Guid.NewGuid();
        ActivityId = activityId;
        Name = name;
        ValueType = valueType;
        IsRequired = isRequired;
        Unit = unit;
        MinValue = minValue;
        MaxValue = maxValue;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string valueType,
        bool isRequired,
        string? unit,
        string? minValue,
        string? maxValue
    )
    {
        Name = name;
        ValueType = valueType;
        IsRequired = isRequired;
        Unit = unit;
        MinValue = minValue;
        MaxValue = maxValue;
        UpdatedAt = DateTime.UtcNow;
    }
}
