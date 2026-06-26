using StockFlow.API.DTOs;

namespace StockFlow.API.DTOs;

public class OrderDto
{
    public string CustomerName{get;set;}
    public List<OrderItemDto> Items {get;set;}

}