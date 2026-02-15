namespace THtracker.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; private set; }
    public Guid ActivityId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }

    private ActivityLog() { }

    public ActivityLog(Guid activityId, DateTime startedAt)
    {
        Id = Guid.NewGuid();
        ActivityId = activityId;
        StartedAt = startedAt;
    }

    public void UpdatePeriod(DateTime startedAt, DateTime? endedAt)
    {
        if (endedAt.HasValue && endedAt.Value < startedAt)
            throw new ArgumentException("La fecha de fin no puede ser anterior a la de inicio.", nameof(endedAt));
            
        StartedAt = startedAt;
        EndedAt = endedAt;
    }

    public void Stop(DateTime endedAt)
    {
        if (endedAt < StartedAt)
            throw new ArgumentException("La fecha de fin no puede ser anterior a la de inicio.", nameof(endedAt));
            
        EndedAt = endedAt;
    }

    /// <summary>
    /// Calcula la duración del log dentro de un intervalo específico.
    /// Útil para reportes diarios donde el log cruza la medianoche.
    /// </summary>
    public TimeSpan GetDurationInInterval(DateTime intervalStart, DateTime intervalEnd)
    {
        var effectiveEnd = EndedAt ?? DateTime.UtcNow;

        // Si el log terminó antes del intervalo o empezó después, duración 0
        if (effectiveEnd <= intervalStart || StartedAt >= intervalEnd)
            return TimeSpan.Zero;

        // El inicio real es el máximo entre cuando empezó el log y cuando empieza el intervalo
        var start = StartedAt > intervalStart ? StartedAt : intervalStart;
        
        // El fin real es el mínimo entre cuando terminó el log y cuando termina el intervalo
        var end = effectiveEnd < intervalEnd ? effectiveEnd : intervalEnd;

        return end - start;
    }

    public ICollection<ActivityLogValue> LogValues { get; private set; } = new List<ActivityLogValue>();
}
