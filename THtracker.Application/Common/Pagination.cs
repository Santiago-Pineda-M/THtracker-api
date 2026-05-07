namespace THtracker.Application.Common;

public static class Pagination
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int PageNumber, int PageSize) Normalize(int pageNumber, int pageSize)
    {
        var pn = pageNumber < 1 ? 1 : pageNumber;
        var ps = pageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize
        };
        return (pn, ps);
    }
}
