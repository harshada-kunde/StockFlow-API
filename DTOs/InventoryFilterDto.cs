namespace StockFlow.API.DTOs;

public class InventoryFilterDto
{
    public string? CategoryName { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinStock { get; set; }
    public int? MaxStock { get; set; }
    public string? ProductName { get; set; }
    public string? Brand { get; set; }
}