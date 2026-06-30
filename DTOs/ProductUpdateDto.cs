
namespace StockFlow.API.DTOs;

public class ProductUpdateDto
{
    public string Brand { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    
}