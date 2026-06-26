using Microsoft.EntityFrameworkCore;
using StockFlow.API.Data;
using StockFlow.API.Entities;
using StockFlow.API.Data.Repositories.Interfaces;

namespace StockFlow.API.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).OrderByDescending(o => o.OrderDate).ToListAsync();
    }

    public async Task<List<Order>> GetOrderHistoryAsync(
        DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).AsQueryable();

        if (startDate.HasValue)
            query = query.Where(o => o.OrderDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.OrderDate <= endDate.Value);

        return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateOrderAsync(Order order)
    {
        await _context.SaveChangesAsync();
    }

    public async Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId)
    {
        return await _context.OrderItems.Include(oi => oi.Product).FirstOrDefaultAsync(oi => oi.Id == orderItemId);
    }

    public async Task<OrderItem> AddOrderItemAsync(OrderItem orderItem)
    {
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();
        return orderItem;
    }

    public async Task UpdateOrderItemAsync(OrderItem orderItem)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOrderItemAsync(OrderItem orderItem)
    {
        _context.OrderItems.Remove(orderItem);
        await _context.SaveChangesAsync();
    }
}