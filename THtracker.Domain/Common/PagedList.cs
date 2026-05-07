namespace THtracker.Domain.Common;

public sealed record PagedList<T>(IReadOnlyList<T> Items, int TotalCount);
