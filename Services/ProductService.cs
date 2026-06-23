using StockFlow.API.DTOs;
using StockFlow.API.Entities;
using StockFlow.API.Models;
using StockFlow.API.Data.Repositories.Interfaces;
using StockFlow.API.Services.Interfaces;
using StockFlow.API.ValidationService.Interfaces;

namespace StockFlow.API.Services;
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductValidation _productValidation;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, IProductValidation productValidation, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _productValidation = productValidation;
        _categoryRepository = categoryRepository;
    }

    public async Task<ApiResponse<PagedResponse<Product>>> GetAllProductsAsync(int pageNo, int pageSize)
    {
        var pagedResponse = new PagedResponse<Product>();
        // Validate pagination
        if (pageNo <= 0 || pageSize <= 0)
        {
            return ApiResponse<PagedResponse<Product>>.ErrorResponse("Page number and Page Size must be greater than 0.");
        }

        var (products, totalCount) = await _productRepository.GetAllAsync(pageNo, pageSize);
        if (products.Count == 0)
        {
            return ApiResponse<PagedResponse<Product>>.SuccessResponse(pagedResponse, "No products found.");
        }
        else
        {
            pagedResponse.Items = products;
            pagedResponse.TotalCount = totalCount;
            pagedResponse.PageNo = pageNo;
            pagedResponse.PageSize = pageSize;

        }
        return ApiResponse<PagedResponse<Product>>.SuccessResponse(pagedResponse, "Products retrieved successfully.");
    }
    public async Task<ApiResponse<Product>> GetByProductIdAsync(int id)
    {
        if (id <= 0)
            return ApiResponse<Product>.ErrorResponse("Invalid product ID.");

        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
            return ApiResponse<Product>.SuccessResponse(product , $"Product with ID {id} does not exist.");

        return ApiResponse<Product>.SuccessResponse(product, "Product retrieved successfully.");
    }
    public async Task<ApiResponse<List<Product>>> SearchByNameAsync(string name)
    {
            name = name.Trim();

            var products = await _productRepository.SearchByNameAsync(name);

        if (products.Count == 0)
            return ApiResponse<List<Product>>.SuccessResponse(products, $"No products found matching '{name}'.");

        return ApiResponse<List<Product>>.SuccessResponse(products,$"{products.Count} product(s) found matching '{name}'.");
    }
    public async Task<ApiResponse<List<Product>>> GetByCategoryNameAsync(string categoryName, int pageNo, int pageSize)
    {
      if (pageNo <= 0 || pageSize <= 0)
        {
            return ApiResponse<List<Product>>.ErrorResponse("Page number and Page Size must be greater than 0.");
        }

        // Check if category exists
        var category = await _categoryRepository.GetByNameAsync(categoryName);
        if (category == null)
            return ApiResponse<List<Product>>.ErrorResponse($"Category '{categoryName}' does not exist.");

        var products = await _productRepository.GetByCategoryNameAsync(categoryName, pageNo, pageSize);

        if (products.Count == 0)
            return ApiResponse<List<Product>>.SuccessResponse(products, $"No products found in category '{categoryName}'.");

        return ApiResponse<List<Product>>.SuccessResponse(products, $"{products.Count} product(s) found in category '{categoryName}'.");
    }
    public async Task<ApiResponse<List<Product>>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, int pageNo, int pageSize)
    {
       /* if (minPrice < 0)
            return ApiResponse<List<Product>>.Fail(
                   "Minimum price cannot be negative.");

        if (maxPrice <= 0)
            return ApiResponse<List<Product>>.Fail(
                   "Maximum price must be greater than 0.");

        if (minPrice >= maxPrice)
            return ApiResponse<List<Product>>.Fail(
                   "Minimum price must be less than maximum price.");

        if (pageNo <= 0)
            return ApiResponse<List<Product>>.Fail(
                   "Page number must be greater than 0.");

        if (pageSize <= 0 || pageSize > 50)
            return ApiResponse<List<Product>>.Fail(
                   "Page size must be between 1 and 50.");*/ // put all this code in vallidation service

        var products = await _productRepository.GetByPriceRangeAsync(minPrice, maxPrice, pageNo, pageSize);

        if (products.Count == 0)
            return ApiResponse<List<Product>>.SuccessResponse(products,$"No products found between " + $"€{minPrice} and €{maxPrice}.");

            return ApiResponse<List<Product>>.SuccessResponse(products,$"{products.Count} product(s) found between " + $"€{minPrice} and €{maxPrice}.");
    }
    public async Task<ApiResponse<Product>> CreateProductAsync(ProductDto dto)
    {
        // Validate — returns all errors at once
        var errors = await _productValidation.ValidateCreateAsync(dto);
        if (errors.Count > 0)
            return ApiResponse<Product>.ValidationErrorResponse(errors);

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Brand = dto.Brand.Trim(),
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };
        var productCreated = await _productRepository.CreateAsync(product);
        return ApiResponse<Product>.SuccessResponse(productCreated,"Product created successfully.");
    }
    public async Task<ApiResponse<List<Product>>> CreateMultipleProductsAsync(List<ProductDto> dtos)
    {
        if (dtos == null || dtos.Count == 0)
            return ApiResponse<List<Product>>.ErrorResponse("No products provided.");

        // Validate ALL products first — collect all errors
        var allErrors = new List<string>();

        foreach (var prod in dtos)
        {
            var errors = await _productValidation.ValidateCreateAsync(prod);
            if (errors.Count > 0)
            {
                allErrors.AddRange(errors.Select(e => $"Product {prod.Name}: {e}"));
            }
        }

        // If ANY product failed — reject ALL
        if (allErrors.Count > 0)
            return ApiResponse<List<Product>>.ValidationErrorResponse(allErrors);

        // All passed — save all
        var products = dtos.Select(dto => new Product
        {
            Name = dto.Name.Trim(),
            Brand = dto.Brand.Trim(),
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        }).ToList();

        var created = await _productRepository.CreateMultipleAsync(products);
        return ApiResponse<List<Product>>.SuccessResponse(created,$"{created.Count} products created successfully.");
    }
    public async Task<ApiResponse<Product>> UpdateProductAsync(int id, ProductDto dto)
    {
        if (id <= 0)
        return ApiResponse<Product>.ErrorResponse("Invalid product ID.");

        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
            return ApiResponse<Product>.ErrorResponse($"Product with ID {id} does not exist.");

        // Validate — pass id for exclude check
        var errors = await _productValidation.ValidateUpdateAsync(id, dto);

        if (errors.Count > 0)
        return ApiResponse<Product>.ValidationErrorResponse(errors);

        // Update only allowed fields — CategoryId cannot change
        product.Name = dto.Name.Trim();
        product.Brand = dto.Brand.Trim();
        product.Description = dto.Description?.Trim();
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;

        await _productRepository.UpdateAsync(product);

        return ApiResponse<Product>.SuccessResponse(product,"Product updated successfully.");
    }

    public async Task<ApiResponse<string>> DeleteProductAsync(int id)
    {
        if (id <= 0)
            return ApiResponse<string>.ErrorResponse("Invalid product ID.");

        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
            return ApiResponse<string>.ErrorResponse($"Product with ID {id} does not exist.");

        await _productRepository.DeleteAsync(product);

        return ApiResponse<string>.SuccessResponse(default,$"Product '{product.Name}' deleted successfully.");
    }

}