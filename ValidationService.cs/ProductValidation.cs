using StockFlow.API.DTOs;
using StockFlow.API.Entities;
using StockFlow.API.ValidationService.Interfaces;
using StockFlow.API.Data.Repositories.Interfaces;

namespace StockFlow.API.ValidationService;

public class ProductValidation : IProductValidation
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductValidation(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }
    public async Task<List<string>> ValidateCreateAsync(ProductDto dto)
    {
        var errors = new List<string>();

        // ── Name validation 
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            errors.Add("Product name is required.");
            return errors; // no point checking further
        }

        if (dto.Name.Length < 2)
            errors.Add("Name must be at least 2 characters.");

        if (dto.Name.Length > 150)
            errors.Add("Name cannot exceed 150 characters.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Name, @"^[a-zA-Z0-9\s\-_]+$"))
            errors.Add("Name can only contain letters, numbers, spaces, hyphens and underscores.");

        // ── Brand validation ─────────
        if (string.IsNullOrWhiteSpace(dto.Brand))
        {
            errors.Add("Brand is required.");
        }
        else
        {
            if (dto.Brand.Length < 2)
                errors.Add("Brand must be at least 2 characters.");

            if (dto.Brand.Length > 50)
                errors.Add("Brand cannot exceed 50 characters.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Brand, @"^[a-zA-Z0-9\s\-_]+$"))
                errors.Add("Brand can only contain letters, numbers, spaces, hyphens and underscores.");
        }

        // ── Description validation ────────
        if (dto.Description != null)
        {
            dto.Description = dto.Description.Trim();

            if (dto.Description.Length > 500)
                errors.Add("Description cannot exceed 500 characters.");
        }

        // ── Price validation ─────────────
        if (dto.Price <= 0)
            errors.Add("Price must be greater than 0.");

        // ── Stock validation ─────────────────────────────────────
        if (dto.StockQuantity < 0)
            errors.Add("Stock quantity cannot be negative.");

        // ── CategoryId validation ────────────
        if (dto.CategoryId <= 0)
        {
            errors.Add("Category ID must be a positive number.");
        }
        else
        {
            // Only check DB if basic validation passed
            if (errors.Count == 0 || !errors.Any(e => e.Contains("Category")))
            {
                var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
                if (category == null)
                    errors.Add($"Category with ID {dto.CategoryId} " + $"does not exist.");
            }
        }
        // ── Duplicate check ────────────────
        // Only run if name and categoryId passed validation
        if (errors.Count == 0)
        {
            bool exists = await _productRepository.ProductExistsAsync(dto.Name, dto.CategoryId);
            if (exists)
                errors.Add($"A product named '{dto.Name}' already exists in this category.");
        }
        return errors;
    }
    public async Task<List<string>> ValidateUpdateAsync(int id, ProductDto dto)
    {
        var errors = new List<string>();

        // ── ID validation ────────────────
        if (id <= 0)
        {
            errors.Add("Invalid product ID.");
            return errors;
        }

        // ── Name validation ───────
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            errors.Add("Product name is required.");
            return errors;
        }

        dto.Name = dto.Name.Trim();

        if (dto.Name.Length < 2)
            errors.Add("Name must be at least 2 characters.");

        if (dto.Name.Length > 150)
            errors.Add("Name cannot exceed 150 characters.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Name, @"^[a-zA-Z0-9\s\-_]+$"))
            errors.Add("Name can only contain letters, numbers, spaces, hyphens and underscores.");

        // ── Brand validation ──────────────────
        if (string.IsNullOrWhiteSpace(dto.Brand))
        {
            errors.Add("Brand is required.");
        }
        else
        {
            if (dto.Brand.Length < 2)
                errors.Add("Brand must be at least 2 characters.");

            if (dto.Brand.Length > 50)
                errors.Add("Brand cannot exceed 50 characters.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Brand, @"^[a-zA-Z0-9\s\-_]+$"))
                errors.Add("Brand can only contain letters, numbers, spaces, hyphens and underscores.");
        }

        // ── Description validation ──────────────
        if (dto.Description != null)
        {
            dto.Description = dto.Description.Trim();
            if (dto.Description.Length > 500)
            errors.Add("Description cannot exceed 500 characters.");
        }

        // ── Price validation ───────────────
        if (dto.Price <= 0)
            errors.Add("Price must be greater than 0.");

        // ── Stock validation ────────────────
        if (dto.StockQuantity < 0)
            errors.Add("Stock quantity cannot be negative.");

        // ── Duplicate check excluding current record ─────
        if (errors.Count == 0)
        {
            bool exists = await _productRepository.ProductExistsAsync(
                          dto.Name, dto.CategoryId, excludeId: id);
            if (exists)
                errors.Add($"Another product named '{dto.Name}' already exists in this category.");
        }

        return errors;
    }
}