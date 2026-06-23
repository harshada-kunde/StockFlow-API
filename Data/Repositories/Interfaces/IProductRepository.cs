using System.Collections.Generic;
using System.Threading.Tasks;
using StockFlow.API.Entities;
using StockFlow.API.DTOs;

namespace StockFlow.API.Data.Repositories.Interfaces;
public interface IProductRepository
{
    // Get all with pagination
    Task<(List<Product> products, int totalCount)> GetAllAsync(
        int pageNo, int pageSize);

    // Get single by id
    Task<Product?> GetByIdAsync(int id);

    // Search by name
    Task<List<Product>> SearchByNameAsync(string name);

    // Filter by category name with pagination
    Task<List<Product>> GetByCategoryNameAsync(
        string categoryName, int pageNo, int pageSize);

    // Filter by price range with pagination
    Task<List<Product>> GetByPriceRangeAsync(
        decimal minPrice, decimal maxPrice, 
        int pageNo, int pageSize);

    // Composite duplicate check
    Task<bool> ProductExistsAsync(
        string name, int categoryId, int? excludeId = null);

    // Create single
    Task<Product> CreateAsync(Product product);

    // Create multiple — implement after single create works
    Task<List<Product>> CreateMultipleAsync(List<Product> products);

    // Update — Product object contains Id inside it
    Task UpdateAsync(Product product);

    // Delete — pass whole object to avoid extra DB call
    Task DeleteAsync(Product product);
}
