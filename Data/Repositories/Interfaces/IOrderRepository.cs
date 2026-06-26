using StockFlow.API.Entities;

namespace StockFlow.API.Data.Repositories.Interfaces;

public interface IOrderRepository
{
    // Get single order with all its items
    Task<Order?> GetOrderByIdAsync(int orderId);

    // Get all orders
    Task<List<Order>> GetAllOrdersAsync();

    // Get order history with optional date filter
    Task<List<Order>> GetOrderHistoryAsync(DateTime? startDate, DateTime? endDate);

    // Create new order
    Task<Order> CreateOrderAsync(Order order);

    // Update order — status, total, updatedOn etc.
    Task UpdateOrderAsync(Order order);

    // Get specific order item
    Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId);

    // Add new item to existing order
    Task<OrderItem> AddOrderItemAsync(OrderItem orderItem);

    // Update order item quantity
    Task UpdateOrderItemAsync(OrderItem orderItem);

    // Delete order item — used when cancelling
    Task DeleteOrderItemAsync(OrderItem orderItem);
}