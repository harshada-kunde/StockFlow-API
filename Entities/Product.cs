namespace StockFlow.API.Entities;
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }

    public DateTime CreatedOn { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    //Navigation Property
    public Category? Category { get; set; } 


}