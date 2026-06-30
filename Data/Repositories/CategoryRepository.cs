using Microsoft.EntityFrameworkCore;
using StockFlow.API.Data;
using StockFlow.API.Entities;
using StockFlow.API.DTOs;
using StockFlow.API.Data.Repositories.Interfaces;

namespace StockFlow.API.Data.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
            private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
                _context = context;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int categoryId)
        {
                return await _context.Categories.FindAsync(categoryId);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return category;
        }

        public async Task<Category?> UpdateCategoryAsync(int Id, Category category)
        {
                var existingCategory = await _context.Categories.FindAsync(Id);
                if (existingCategory == null) return null;

                existingCategory.Name = category.Name;
            // existingCategory.Description = category.Description;
                await _context.SaveChangesAsync();
                return existingCategory;
        }

        public async Task<bool> DeleteByIdAsync(int categoryId)
        {
                var category = await _context.Categories.FindAsync(categoryId);
                if (category == null) return false;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
        }
        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            return await _context.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.Id != excludeId);
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        
        }
    }

}