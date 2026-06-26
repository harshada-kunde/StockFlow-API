
namespace StockFlow.API.Entities;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalQuantity { get; set; }
    public string Status { get; set; } = "Pending";
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }

    // Navigation property
    public List<OrderItem> OrderItems { get; set; } = new();
}
