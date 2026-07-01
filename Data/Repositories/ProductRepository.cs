
using Microsoft.EntityFrameworkCore;
using StockFlow.API.Data;
using StockFlow.API.Entities;
using StockFlow.API.DTOs;
using StockFlow.API.Data.Repositories.Interfaces;

namespace StockFlow.API.Data.Repositories;

public class ProductRepository : IProductRepository
{
    public readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    } 
    public async Task<(List<Product> products, int totalCount)> GetAllAsync(
        int pageNo, int pageSize)
   {
    var query = _context.Products.Include(p => p.Category).OrderBy(p => p.Name);      //Does NOT hit database — just builds the query in memory

    var countTask = query.CountAsync();     //THIS hits the database — CountAsync() executes the query
           
    var productsTask = query.Skip((pageNo - 1) * pageSize).Take(pageSize).ToListAsync();               //THIS hits the database — ToListAsync() executes the query

    await Task.WhenAll(countTask, productsTask);  //Task.WhenAll runs both database calls simultaneously instead of sequentially: instead of hitting DB twice, it hits it once for both calls.

    return (await productsTask, await countTask);
}

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> SearchByNameAsync(string name)
    {
        return await _context.Products
                             .Include(p => p.Category)
                             .Where(p => p.Name.ToLower()
                             .Contains(name.ToLower()))
                             .OrderBy(p => p.Name)
                             .ToListAsync();
    }

    public async Task<List<Product>> GetByCategoryNameAsync(
        string categoryName, int pageNo, int pageSize)
    {
        return await _context.Products
                             .Include(p => p.Category)
                             .Where(p => p.Category != null &&
                              p.Category.Name.ToLower() == 
                              categoryName.ToLower())
                             .OrderBy(p => p.Name)
                             .Skip((pageNo - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync();
    }

    public async Task<List<Product>> GetByPriceRangeAsync(
        decimal minPrice, decimal maxPrice,
        int pageNo, int pageSize)
    {
        return await _context.Products
                             .Include(p => p.Category)
                             .Where(p => p.Price >= minPrice && 
                                    p.Price <= maxPrice)
                             .OrderBy(p =>p.Price)
                             .Skip((pageNo - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync();
    }

    public async Task<bool> ProductExistsAsync(
        string name, int categoryId, int? excludeId = null)
    {
        return await _context.Products
                             .AnyAsync(p =>
                              p.Name.ToLower() == name.ToLower() &&
                              p.CategoryId == categoryId &&
                              p.Id != excludeId);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<List<Product>> CreateMultipleAsync(
        List<Product> products)
    {
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();
        return products;
    }

    public async Task UpdateAsync(Product product)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }


}