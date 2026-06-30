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
        try
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
        catch (Exception ex)
        {
            return ApiResponse<PagedResponse<Product>>.ErrorResponse($"Unexpected error in {nameof(ProductService)}.{nameof(GetAllProductsAsync)}: {ex.Message}");
        }
    }

    public async Task<ApiResponse<Product>> GetByProductIdAsync(int id)
    {
        try
        {   
            if (id <= 0)
                return ApiResponse<Product>.ErrorResponse("Invalid product ID.");

            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return ApiResponse<Product>.SuccessResponse(product , $"Product with ID {id} does not exist.");

            return ApiResponse<Product>.SuccessResponse(product, "Product retrieved successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<Product>.ErrorResponse($"Unexpected error in {nameof(ProductService)}.{nameof(GetByProductIdAsync)}: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<Product>>> SearchByNameAsync(string name)
    {
        try
        {
            name = name.Trim();

            var products = await _productRepository.SearchByNameAsync(name);

            if (products.Count == 0)
                return ApiResponse<List<Product>>.SuccessResponse(products, $"No products found matching '{name}'.");

            return ApiResponse<List<Product>>.SuccessResponse(products,$"{products.Count} product(s) found matching '{name}'.");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<Product>>.ErrorResponse($"Unexpected error in {nameof(ProductService)}.{nameof(SearchByNameAsync)}: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<Product>>> GetByCategoryNameAsync(string categoryName, int pageNo, int pageSize)
    {
        try
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
        catch (Exception ex)
        {
            return ApiResponse<List<Product>>.ErrorResponse($"Unexpected error in {nameof(ProductService)}.{nameof(GetByCategoryNameAsync)}: {ex.Message}");
        }
    }

    
    public async Task<ApiResponse<List<Product>>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, int pageNo, int pageSize)
    {
        try
        {
            if (minPrice < 0 || maxPrice < 0)
                return ApiResponse<List<Product>>.ErrorResponse("Invalid price range.");

            if (pageNo <= 0 || pageSize <= 0)
                return ApiResponse<List<Product>>.ErrorResponse("Page number and Page Size must be greater than 0.");

            var products = await _productRepository.GetByPriceRangeAsync(minPrice, maxPrice, pageNo, pageSize);

            if (products.Count == 0)
                return ApiResponse<List<Product>>.SuccessResponse(products,$"No products found between " + $"€{minPrice} and €{maxPrice}.");

            return ApiResponse<List<Product>>.SuccessResponse(products,$"{products.Count} product(s) found between " + $"€{minPrice} and €{maxPrice}.");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<Product>>.ErrorResponse($"Unexpected error in {nameof(ProductService)}.{nameof(GetByPriceRangeAsync)}: {ex.Message}");
        }
    }

    public async Task<ApiResponse<Product>> CreateProductAsync(ProductDto dto)
    {
        try
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
        catch (Exception ex)
        {
            return ApiResponse<Product>.ErrorResponse($"Unexpected error in {nameof(ProductService)}.{nameof(CreateProductAsync)}: {ex.Message}");
        }
    }
    public async Task<ApiResponse<List<Product>>> CreateMultipleProductsAsync(List<ProductDto> dtos)
    {
        try
        {
            if (dtos == null || dtos.Count == 0)
                return ApiResponse<List<Product>>.ErrorResponse("No products provided.");

            // Validate ALL products first — collect all errors
            var allErrors = new List<string>();
            var batchErrors = new List<string>();

            //check duplicate records in single request
            batchErrors = _productValidation.ValidateBulkForDuplicates(dtos);

            if (batchErrors.Count > 0)
                return ApiResponse<List<Product>>.ValidationErrorResponse(batchErrors);

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
        catch (Exception ex)
        {
            return ApiResponse<List<Product>>.ErrorResponse($"Unexpected error in {nameof(ProductService)}.{nameof(CreateMultipleProductsAsync)}: {ex.Message}");
        }
    }
    public async Task<ApiResponse<Product>> UpdateProductAsync(int id, ProductUpdateDto dto)
    {
        try
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
            product.Brand = dto.Brand.Trim();
            product.Description = dto.Description?.Trim();
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;

            await _productRepository.UpdateAsync(product);

            return ApiResponse<Product>.SuccessResponse(product,"Product updated successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<Product>.ErrorResponse($"Unexpected error in {nameof(ProductService)}.{nameof(UpdateProductAsync)}: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteProductAsync(int id)
    {
        try
        {
            if (id <= 0)
                return ApiResponse<string>.ErrorResponse("Invalid product ID.");

            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return ApiResponse<string>.ErrorResponse($"Product with ID {id} does not exist.");

            await _productRepository.DeleteAsync(product);

            return ApiResponse<string>.SuccessResponse(default,$"Product '{product.Name}' deleted successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResponse($"Unexpected error in {nameof(ProductService)}.{nameof(DeleteProductAsync)}: {ex.Message}");
        }
    }

}