using StockFlow.API.DTOs;
using StockFlow.API.Entities;
using StockFlow.API.Models;

namespace StockFlow.API.Services.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<List<Category>>> GetAllAsync();
    Task<ApiResponse<Category>> GetByIdAsync(int id);
    Task<ApiResponse<Category>> CreateAsync(CategoryDto dto);
    Task<ApiResponse<Category>> UpdateAsync(int id, CategoryDto dto);
    Task<ApiResponse<string>> DeleteAsync(int id);
}