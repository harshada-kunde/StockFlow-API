using Microsoft.AspNetCore.Mvc;
using StockFlow.API.DTOs;
using StockFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace StockFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Creates a new order. Requires Admin role.
    /// </summary>
    /// <param name="dto">The order details.</param>
    /// <returns>Order details.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderDto dto)
    {
        var response = await _orderService.CreateOrderAsync(dto);
        if (!response.Success)
        {
            if (response.Errors != null)
                return UnprocessableEntity(response);

            return BadRequest(response);
        }

        return CreatedAtAction(nameof(GetOrderById),
               new { orderId = response.Data!.Id }, response);
    }

    /// <summary>
    /// Retrieves an order by its ID. Requires Admin role.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <returns>Order details.</returns>
    [HttpGet("{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(int orderId)
    {
        var response = await _orderService.GetOrderByIdAsync(orderId);
        if (!response.Success)
        {
            if (response.Message.Contains("does not exist") ||
                response.Message.Contains("Invalid"))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }
    /// <summary>
    /// Retrieves the order history.
    /// </summary>
    /// <param name="startDate">The start date for the order history.</param>
    /// <param name="endDate">The end date for the order history.</param>
    /// <returns>Order details.</returns>
    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> GetOrderHistory([FromQuery] DateOnly? startDate,[FromQuery] DateOnly? endDate)
    {
        DateTime? start = startDate.HasValue ? startDate.Value.ToDateTime(TimeOnly.MinValue) : null;
        DateTime? end = endDate.HasValue ? endDate.Value.ToDateTime(TimeOnly.MaxValue) : null;

        var response = await _orderService.GetOrderHistoryAsync(start, end);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Adds an item to an existing order. Requires Admin role.
    /// </summary>
    /// <param name="dto">The order item details.</param>
    /// <returns>Order details.</returns>
    [HttpPut("{orderId}/additem")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddOrderItem( int orderId, [FromBody] OrderItemDto dto)
    {
        var response = await _orderService.AddOrderItemAsync(orderId, dto);
        if (!response.Success)
        {
            if (response.Message.Contains("does not exist") ||
                response.Message.Contains("Invalid"))
                return NotFound(response);

            if (response.Errors != null)
                return UnprocessableEntity(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Updates an item in an existing order. Requires Admin role.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="newQuantity">The new quantity for the order item.</param>
    /// <param name="orderItemId">The order item ID.</param>
    /// <returns>Order details.</returns>
    [HttpPut("{orderId}/updateitem/{orderItemId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateOrderItem(int orderId,int orderItemId,[FromQuery] int newQuantity)
    {
        var response = await _orderService.UpdateOrderItemAsync(
                       orderId, orderItemId, newQuantity);
        if (!response.Success)
        {
            if (response.Message.Contains("does not exist") ||
                response.Message.Contains("Invalid"))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Confirms an existing order. Requires Admin role.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <returns>Confirmation details.</returns>
    [HttpPut("{orderId}/confirm")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ConfirmOrder(int orderId)
    {
        var response = await _orderService.ConfirmOrderAsync(orderId);
        if (!response.Success)
        {
            if (response.Message.Contains("does not exist") ||
                response.Message.Contains("Invalid"))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Cancels an existing order. Requires Admin role.
    /// </summary>
    /// <param name="orderId">The order ID .</param>
    /// <returns>Cancellation details.</returns>
    [HttpPut("{orderId}/cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var response = await _orderService.CancelOrderAsync(orderId);
        if (!response.Success)
        {
            if (response.Message.Contains("does not exist") ||
                response.Message.Contains("Invalid"))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }
}