namespace AcilEvrak.Application.Models;

public sealed class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public long TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    private PagedResponse(IReadOnlyList<T> items, int page, int pageSize, long totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PagedResponse<T> Create(IReadOnlyList<T> items, int page, int pageSize, long totalCount)
        => new(items, page, pageSize, totalCount);
}
