using Microsoft.EntityFrameworkCore;
using StockFlow.API.Entities;

namespace StockFlow.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tables
        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }
    }
}