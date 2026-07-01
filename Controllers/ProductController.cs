using Microsoft.AspNetCore.Mvc;
using StockFlow.API.DTOs;
using StockFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace StockFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Retrieves all products. Requires User role.
    /// </summary>
    /// <param name="pageNo">The page number for pagination (default is 1).</param>
    /// <param name="pageSize">The number of products per page (default is 10).</param>
    /// <returns>List of products.</returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllProducts([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _productService.GetAllProductsAsync(pageNo, pageSize);
        if (!response.Success)
        return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Retrieves a single product by Product ID. Requires User role.
    /// </summary>
    /// <param name="id">The Product Id.</param>
    /// <returns>Product details.</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _productService.GetByProductIdAsync(id);
        if (!response.Success)
        {
            if (response.Message.Contains("does not exist") || response.Message.Contains("Invalid"))
            return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// search a new product. Requires Admin role.
    /// </summary>
    /// <param name="name">The product name.</param>
    /// <returns>Products details.</returns>
    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> Search([FromQuery] string name)
    {
        var response = await _productService.SearchByNameAsync(name);

        if (!response.Success)
        return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Get Products based on Category Name
    /// </summary>
    /// <param name="categoryName">The order details.</param>
    /// <param name="pageNo">The page number for pagination (default is 1).</param>
    /// <param name="pageSize">The number of products per page (default is 10).</param> 
    /// <returns>Product details.</returns>
    [HttpGet("category/{categoryName}")]
    [Authorize]
    public async Task<IActionResult> GetByCategoryName(string categoryName,[FromQuery] int pageNo = 1,[FromQuery] int pageSize = 10)
    {
        var response = await _productService.GetByCategoryNameAsync(categoryName, pageNo, pageSize);

        if (!response.Success)
        {
            if (response.Message.Contains("does not exist"))
            return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get Products based on Price Range
    /// </summary>
    /// <param name="minPrice">Minimum Price</param>
    /// <param name="maxPrice">Maximum Price.</param>
    /// <param name="pageNo">The page number for pagination (default is 1).</param>
    /// <param name="pageSize">The number of products per page (default is 10).</param> 
    /// <returns>Product details.</returns>
    [HttpGet("filter")]
    [Authorize]
    public async Task<IActionResult> GetByPriceRange([FromQuery] decimal minPrice,[FromQuery] decimal maxPrice,[FromQuery] int pageNo = 1,[FromQuery] int pageSize = 10)
    {
        var response = await _productService.GetByPriceRangeAsync(minPrice, maxPrice, pageNo, pageSize);

        if (!response.Success)
        return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Create Products
    /// </summary>
    /// <param name="dto">The product details.</param>
    /// <returns>Product details.</returns>
     [HttpPost]
     [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ProductDto dto)
    {
        var response = await _productService.CreateProductAsync(dto);

        if (!response.Success)
        {
            if (response.Errors != null)
            return UnprocessableEntity(response);

            return BadRequest(response);
        }

        return CreatedAtAction(nameof(GetById),new { id = response.Data!.Id }, response);
    }
    
    /// <summary>
    /// Create multiple products
    /// </summary>
    /// <param name="categoryName">The order details.</param>
    /// <param name="pageNo">The page number for pagination (default is 1).</param>
    /// <param name="pageSize">The number of products per page (default is 10).</param> 
    /// <returns>Product details.</returns>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateMultiple([FromBody] List<ProductDto> dtos)
    {
        var response = await _productService.CreateMultipleProductsAsync(dtos);

        if (!response.Success)
        {
            if (response.Errors != null)
            return UnprocessableEntity(response);

            return BadRequest(response);
        }

        return Ok(response);
    }


    /// <summary>
    /// Updates an existing products. Requires Admin role.
    /// </summary>
    /// <param name="id">The product ID to update.</param>
    /// <param name="dto">Updated product details.</param>
    /// <returns>updated product details.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
    {
        var response = await _productService.UpdateProductAsync(id, dto);
        if (!response.Success)
        {
            if (response.Message.Contains("does not exist") || response.Message.Contains("Invalid"))
            return NotFound(response);

            if (response.Errors != null)
            return UnprocessableEntity(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

     /// <summary>
    /// Delete a category. Requires Admin role.
    /// </summary>
    /// <param name="id">The category ID to delete.</param>
    /// <returns>Confirmation Message</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _productService.DeleteProductAsync(id);
        if (!response.Success)
        {
            if (response.Message.Contains("does not exist") || response.Message.Contains("Invalid"))
            return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

}