
using System.Collections.Generic;
using System.Threading.Tasks;
using StockFlow.API.Data.Entities;

public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();
         Task<Category?> GetByIdAsync(int categoryId);

        //Task<Category> GetByNameAsync(string name);
        Task<Category> CreateCategoryAsync(CategoryDto category);
        Task<Category?> UpdateCategoryAsync(int Id, CategoryDto category);
        Task<bool> DeleteByIdAsync(int categoryId);

}