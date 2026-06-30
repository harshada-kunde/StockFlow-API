using StockFlow.API.DTOs;
using StockFlow.API.Entities;

namespace StockFlow.API.ValidationService.Interfaces
{
    public interface IOrderValidation
    {
        // Same pattern as ICategoryValidation and IProductValidation
            Task<List<string>> ValidateCreateAsync(OrderDto dto);
            Task<List<string>> ValidateOrderItemAsync(OrderItemDto dto);
            Task<List<string>> ValidateUpdateOrderItemAsync(int orderId, int orderItemId, int newQuantity, Order order);
            Task<List<string>> ValidateConfirmOrderAsync(Order order);
    }
}
