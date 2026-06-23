namespace StockFlow.API.DTOs;

public class ProductDto
{
    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }

    public string? Description {get;set;} = string.Empty;

}