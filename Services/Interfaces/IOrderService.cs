using StockFlow.API.DTOs;
using StockFlow.API.Entities;
using StockFlow.API.Models;

namespace StockFlow.API.Services.Interfaces;

public interface IOrderService
{
    // Create new order with items
    Task<ApiResponse<Order>> CreateOrderAsync(OrderDto dto);

    // Get single order by id
    Task<ApiResponse<Order>> GetOrderByIdAsync(int orderId);

    // Get order history with optional date filter
    Task<ApiResponse<List<Order>>> GetOrderHistoryAsync(DateTime? startDate, DateTime? endDate);

    // Add new item to existing pending order
    Task<ApiResponse<Order>> AddOrderItemAsync(int orderId, OrderItemDto dto);

    // Update quantity of existing order item
    Task<ApiResponse<Order>> UpdateOrderItemAsync(int orderId, int orderItemId, int newQuantity);

    // Confirm a pending order
    Task<ApiResponse<Order>> ConfirmOrderAsync(int orderId);

    // Cancel a pending order and restore stock
    Task<ApiResponse<string>> CancelOrderAsync(int orderId);
}