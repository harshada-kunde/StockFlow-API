using StockFlow.API.DTOs;
using StockFlow.API.Entities;
using StockFlow.API.Models;
using StockFlow.API.Data.Repositories.Interfaces;
using StockFlow.API.Services.Interfaces;
using StockFlow.API.ValidationService.Interfaces;

namespace StockFlow.API.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderValidation _validator;

    public OrderService(IOrderRepository orderRepository,IProductRepository productRepository,IOrderValidation validator)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _validator = validator;
    }

    public async Task<ApiResponse<Order>> CreateOrderAsync(OrderDto dto)
    {
        // Step 1 — Validate
        var errors = await _validator.ValidateCreateAsync(dto);
        if (errors.Count > 0)
            return ApiResponse<Order>.ValidationErrorResponse(errors);

        // Step 2 — Build OrderItems and calculate totals
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;
        int totalQuantity = 0;

        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);

            var unitPrice = product!.Price;
            var subTotal = unitPrice * itemDto.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice,
                SubTotal = subTotal
            });

            totalAmount += subTotal;
            totalQuantity += itemDto.Quantity;
        }

        // Step 3 — Create Order
        var order = new Order
        {
            CustomerName = dto.CustomerName.Trim(),
            OrderDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            TotalQuantity = totalQuantity,
            Status = "Pending",
            CreatedBy = "system",
            CreatedOn = DateTime.UtcNow,
            OrderItems = orderItems
        };

        // Step 4 — Deduct stock from each product
        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
            product!.StockQuantity -= itemDto.Quantity;
            await _productRepository.UpdateAsync(product);
        }

        // Step 5 — Save order
        var created = await _orderRepository.CreateOrderAsync(order);
        return ApiResponse<Order>.SuccessResponse(created, "Order created successfully.");
    }

    public async Task<ApiResponse<Order>> GetOrderByIdAsync(int orderId)
    {
        if (orderId <= 0)
            return ApiResponse<Order>.ErrorResponse("Invalid order ID.");

        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return ApiResponse<Order>.ErrorResponse(
                   $"Order with ID {orderId} does not exist.");

        return ApiResponse<Order>.SuccessResponse(order, "Order retrieved successfully.");
    }

    public async Task<ApiResponse<List<Order>>> GetOrderHistoryAsync(DateTime? startDate, DateTime? endDate)
    {
        // Validate date range
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            return ApiResponse<List<Order>>.ErrorResponse("Start date cannot be greater than end date.");

        var orders = await _orderRepository.GetOrderHistoryAsync(startDate, endDate);

        if (orders.Count == 0)
            return ApiResponse<List<Order>>.SuccessResponse(orders,"No orders found for the specified period.");

        return ApiResponse<List<Order>>.SuccessResponse(orders,$"{orders.Count} order(s) found.");
    }

    public async Task<ApiResponse<Order>> AddOrderItemAsync(int orderId, OrderItemDto dto)
    {
        // Step 1 — Check order exists and is Pending
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return ApiResponse<Order>.ErrorResponse($"Order with ID {orderId} does not exist.");

        if (order.Status != "Pending")
            return ApiResponse<Order>.ErrorResponse($"Cannot add items to a {order.Status} order.");

        // Step 2 — Validate new item
        var errors = await _validator.ValidateOrderItemAsync(dto);
        if (errors.Count > 0)
            return ApiResponse<Order>.ValidationErrorResponse(errors);

        // Step 3 — Check product exists
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
            return ApiResponse<Order>.ErrorResponse($"Product with ID {dto.ProductId} does not exist.");

        // Step 4 — Check if product already in order
        bool alreadyExists = order.OrderItems.Any(oi => oi.ProductId == dto.ProductId);
        if (alreadyExists)
            return ApiResponse<Order>.ErrorResponse($"Product '{product.Name}' is already in this order. Use update quantity instead.");

        // Step 5 — Check sufficient stock
        if (product.StockQuantity < dto.Quantity)
            return ApiResponse<Order>.ErrorResponse($"Insufficient stock for '{product.Name}'. Requested: {dto.Quantity}, Available: {product.StockQuantity}.");

        // Step 6 — Create new OrderItem
        var orderItem = new OrderItem
        {
            OrderId = orderId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitPrice = product.Price,
            SubTotal = product.Price * dto.Quantity
        };

        // Step 7 — Deduct stock
        product.StockQuantity -= dto.Quantity;
        await _productRepository.UpdateAsync(product);

        // Step 8 — Add item and recalculate totals
        await _orderRepository.AddOrderItemAsync(orderItem);
        order.TotalAmount += orderItem.SubTotal;
        order.TotalQuantity += dto.Quantity;
        order.UpdatedOn = DateTime.UtcNow;
        order.UpdatedBy = "system";
        await _orderRepository.UpdateOrderAsync(order);

        // Step 9 — Return fresh order with all items
        var updatedOrder = await _orderRepository.GetOrderByIdAsync(orderId);
        return ApiResponse<Order>.SuccessResponse(updatedOrder!,"Item added to order successfully.");
    }

    public async Task<ApiResponse<Order>> UpdateOrderItemAsync(int orderId, int orderItemId, int newQuantity)
    {
        // Step 1 — Check order exists and is Pending
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return ApiResponse<Order>.ErrorResponse($"Order with ID {orderId} does not exist.");

        if (order.Status != "Pending")
            return ApiResponse<Order>.ErrorResponse($"Cannot update items in a {order.Status} order.");

        // Step 2 — Validate new quantity
        if (newQuantity <= 0)
            return ApiResponse<Order>.ErrorResponse("Quantity must be greater than 0.");

        // Step 3 — Check OrderItem exists
        var orderItem = await _orderRepository.GetOrderItemByIdAsync(orderItemId);
        if (orderItem == null)
            return ApiResponse<Order>.ErrorResponse($"Order item with ID {orderItemId} does not exist.");

        // Step 4 — Check item belongs to this order
        if (orderItem.OrderId != orderId)
            return ApiResponse<Order>.ErrorResponse("Order item does not belong to this order.");

        // Step 5 — Calculate stock difference
        var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
        int diff = newQuantity - orderItem.Quantity;

        if (diff > 0)
        {
            // Needs MORE stock — check available
            if (product!.StockQuantity < diff)
                return ApiResponse<Order>.ErrorResponse($"Insufficient stock for '{product.Name}'. Additional needed: {diff}, Available: {product.StockQuantity}.");

            // Deduct extra stock
            product.StockQuantity -= diff;
        }
        else if (diff < 0)
        {
            // Returning stock — restore to product
            product!.StockQuantity += Math.Abs(diff);
        }

        await _productRepository.UpdateAsync(product!);

        // Step 6 — Update OrderItem
        var oldSubTotal = orderItem.SubTotal;
        orderItem.Quantity = newQuantity;
        orderItem.SubTotal = orderItem.UnitPrice * newQuantity;
        await _orderRepository.UpdateOrderItemAsync(orderItem);

        // Step 7 — Recalculate order totals
        order.TotalAmount = order.TotalAmount - oldSubTotal + orderItem.SubTotal;
        order.TotalQuantity = order.TotalQuantity - (newQuantity < orderItem.Quantity ? orderItem.Quantity - newQuantity : newQuantity - orderItem.Quantity);

        order.UpdatedOn = DateTime.UtcNow;
        order.UpdatedBy = "system";
        await _orderRepository.UpdateOrderAsync(order);

        var updatedOrder = await _orderRepository.GetOrderByIdAsync(orderId);
        return ApiResponse<Order>.SuccessResponse(updatedOrder!,"Order item updated successfully.");
    }

    public async Task<ApiResponse<Order>> ConfirmOrderAsync(int orderId)
    {
        if (orderId <= 0)
            return ApiResponse<Order>.ErrorResponse("Invalid order ID.");

        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return ApiResponse<Order>.ErrorResponse($"Order with ID {orderId} does not exist.");

        // Check status
        if (order.Status == "Confirmed")
            return ApiResponse<Order>.ErrorResponse("Order is already confirmed.");

        if (order.Status == "Cancelled")
            return ApiResponse<Order>.ErrorResponse("Cannot confirm a cancelled order.");

        // Confirm order
        order.Status = "Confirmed";
        order.UpdatedOn = DateTime.UtcNow;
        order.UpdatedBy = "system";
        await _orderRepository.UpdateOrderAsync(order);

        return ApiResponse<Order>.SuccessResponse(order, "Order confirmed successfully.");
    }

    public async Task<ApiResponse<string>> CancelOrderAsync(int orderId)
    {
        if (orderId <= 0)
            return ApiResponse<string>.ErrorResponse("Invalid order ID.");

        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return ApiResponse<string>.ErrorResponse($"Order with ID {orderId} does not exist.");

        // Check status
        if (order.Status == "Cancelled")
            return ApiResponse<string>.ErrorResponse("Order is already cancelled.");

        if (order.Status == "Confirmed")
            return ApiResponse<string>.ErrorResponse("Cannot cancel a confirmed order. Please contact administrator.");

        // Restore stock for each order item
        foreach (var item in order.OrderItems)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                await _productRepository.UpdateAsync(product);
            }
        }

        // Cancel order
        order.Status = "Cancelled";
        order.UpdatedOn = DateTime.UtcNow;
        order.UpdatedBy = "system";
        await _orderRepository.UpdateOrderAsync(order);

        return ApiResponse<string>.SuccessResponse("Cancelled", $"Order #{orderId} has been cancelled and stock has been restored.");
    }
}