namespace ODataConsoleApp.Models;

public class PaginatedResult<T>
{
    public IEnumerable<T>? Items { get; set; }
    public long TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }

    public int TotalPages
    {
        get
        {
            if (PageSize == 0 || TotalCount == 0)
                return 0;

            return (int)Math.Ceiling((double)TotalCount / PageSize);
        }
    }

    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
}
