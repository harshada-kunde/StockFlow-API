
using System.Collections.Generic;
using System.Threading.Tasks;
using StockFlow.API.Entities;
using StockFlow.API.DTOs;

namespace StockFlow.API.Data.Repositories.Interfaces;

public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();
         Task<Category?> GetByIdAsync(int categoryId);

        Task<Category> GetByNameAsync(string name);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category?> UpdateCategoryAsync(int Id, Category category);
        Task<bool> DeleteByIdAsync(int categoryId);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<bool> HasProductsAsync(int categoryId);

    }