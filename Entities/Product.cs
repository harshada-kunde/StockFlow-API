namespace StockFlow.API.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }

    public string? Description {get;set;} = string.Empty;

    public DateTime CreatedOn { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    //Navigation Property
    public Category? Category { get; set; } 


}
