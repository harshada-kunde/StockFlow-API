using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.API.DTOs;
using StockFlow.API.Services.Interfaces;

namespace StockFlow.API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryQueryController : ControllerBase
{
    private readonly IInventoryQueryService _service;

    public InventoryQueryController(IInventoryQueryService service)
    {
        _service = service;
    }

    /// <summary>
    /// Query inventory using natural language.
    /// Example: "show electronics under €500 with low stock"
    /// </summary>
    /// <param name="dto">Natural language query string.</param>
    /// <returns>List of matching products.</returns>
    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] InventoryQueryDto dto)
    {
        var response = await _service.QueryAsync(dto);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}