using StockFlow.API.DTOs;
using StockFlow.API.Entities;
using StockFlow.API.Models;


namespace StockFlow.API.Services.Interfaces;

public interface IProductService
{
    // Get all with pagination
    Task<ApiResponse<PagedResponse<Product>>> GetAllProductsAsync(
        int pageNo, int pageSize);

    // Get single
    Task<ApiResponse<Product>> GetByProductIdAsync(int id);

    // Search by name
    Task<ApiResponse<List<Product>>> SearchByNameAsync(string name);

    // Filter by category name
    Task<ApiResponse<List<Product>>> GetByCategoryNameAsync(
        string categoryName, int pageNo, int pageSize);

    // Filter by price range
    Task<ApiResponse<List<Product>>> GetByPriceRangeAsync(
        decimal minPrice, decimal maxPrice,
        int pageNo, int pageSize);

    // Create single
    Task<ApiResponse<Product>> CreateProductAsync(ProductDto dto);

    // Create multiple
    Task<ApiResponse<List<Product>>> CreateMultipleProductsAsync(
        List<ProductDto> dtos);

    // Update
    Task<ApiResponse<Product>> UpdateProductAsync(int id, ProductDto dto);

    // Delete
    Task<ApiResponse<string>> DeleteProductAsync(int id);
}