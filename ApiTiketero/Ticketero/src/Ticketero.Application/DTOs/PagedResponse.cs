namespace Ticketero.Application.DTOs;

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; }
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "asc";
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
