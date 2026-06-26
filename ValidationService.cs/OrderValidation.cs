using StockFlow.API.DTOs;
using StockFlow.API.Data.Repositories.Interfaces;
using StockFlow.API.ValidationService.Interfaces;

namespace StockFlow.API.ValidationService;

public class OrderValidation : IOrderValidation
{
    private readonly IProductRepository _productRepository;

    public OrderValidation(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<string>> ValidateCreateAsync(OrderDto dto)
    {
        var errors = new List<string>();

        // CustomerName validation 
        if (string.IsNullOrWhiteSpace(dto.CustomerName))
        {
            errors.Add("Customer name is required.");
            return errors;
        }

        dto.CustomerName = dto.CustomerName.Trim();

        if (dto.CustomerName.Length < 2)
            errors.Add("Customer name must be at least 2 characters.");

        if (dto.CustomerName.Length > 100)
            errors.Add("Customer name cannot exceed 100 characters.");

        //  Items list validation 
        if (dto.Items == null || dto.Items.Count == 0)
        {
            errors.Add("Order must contain at least one item.");
            return errors;
        }

        //  Check for duplicate ProductIds 
        var duplicates = dto.Items.GroupBy(i => i.ProductId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        if (duplicates.Count > 0)
        {
            errors.Add($"Duplicate products found in order. Please use update quantity instead of adding same product twice.");
            return errors;
        }

        // Validate each item
        for (int i = 0; i < dto.Items.Count; i++)
        {
            var item = dto.Items[i];

            // ProductId check
            if (item.ProductId <= 0)
            {
                errors.Add($"Item {i + 1}: Product ID must be a positive number.");
                continue; // skip DB check for invalid id
            }

            // Quantity check
            if (item.Quantity <= 0)
            {
                errors.Add($"Item {i + 1}: Quantity must be greater than 0.");
                continue;
            }

            // Product exists check
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                errors.Add($"Item {i + 1}: Product with ID {item.ProductId} does not exist.");
                continue;
            }

            // Sufficient stock check
            if (product.StockQuantity < item.Quantity)
            {
                errors.Add($"Item {i + 1} ('{product.Name}'): Insufficient stock. Requested: {item.Quantity}, " +
                           $"Available: {product.StockQuantity}.");
            }
        }

        return errors;
    }

    public async Task<List<string>> ValidateOrderItemAsync(OrderItemDto dto)
    {
        var errors = new List<string>();

        // ProductId check
        if (dto.ProductId <= 0)
            errors.Add("Product ID must be a positive number.");

        // Quantity check
        if (dto.Quantity <= 0)
            errors.Add("Quantity must be greater than 0.");

        return errors;
    }
}