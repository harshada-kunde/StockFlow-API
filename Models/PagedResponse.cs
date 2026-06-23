namespace StockFlow.API.Models;

//We can combine this class and PaginationDto.cs but then one class is doing two jobs — receiving input AND formatting output. That breaks Single Responsibility
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNo { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(
                             (double)TotalCount / PageSize);
    public bool HasNextPage => PageNo < TotalPages;
    public bool HasPreviousPage => PageNo > 1;
}