using StockFlow.API.Data.Repositories.Interfaces;
using StockFlow.API.Data.Repositories;
using StockFlow.API.Services.Interfaces;
using StockFlow.API.Services;
using StockFlow.API.ValidationService.Interfaces;
using StockFlow.API.ValidationService;

namespace StockFlow.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Services
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IAuthService, AuthService>();

        // Validators
        services.AddScoped<ICategoryValidation, CategoryValidation>();
        services.AddScoped<IProductValidation, ProductValidation>();
        services.AddScoped<IOrderValidation, OrderValidation>();

        return services;
    }
}