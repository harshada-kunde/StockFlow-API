using StockFlow.API.DTOs;
using StockFlow.API.Entities;
using StockFlow.API.Models;

namespace StockFlow.API.Services.Interfaces;

public interface IInventoryQueryService
{
    Task<ApiResponse<List<Product>>> QueryAsync(InventoryQueryDto dto);
}