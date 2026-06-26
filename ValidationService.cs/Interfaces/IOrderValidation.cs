using StockFlow.API.DTOs;

namespace StockFlow.API.ValidationService.Interfaces
{
    public interface IOrderValidation
    {
        // Same pattern as ICategoryValidation and IProductValidation
            Task<List<string>> ValidateCreateAsync(OrderDto dto);
            Task<List<string>> ValidateOrderItemAsync(OrderItemDto dto);
    }
}
