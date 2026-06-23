namespace StockFlow.API.DTOs;

public class PaginationDto
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}