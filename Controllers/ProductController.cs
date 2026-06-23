using Microsoft.AspNetCore.Mvc;
using StockFlow.API.DTOs;
using StockFlow.API.Services.Interfaces;

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

    [HttpGet]
    public async Task<IActionResult> GetAllProducts([FromQuery] int pageNo, [FromQuery] int pageSize)
    {
        var response = await _productService.GetAllProductsAsync(pageNo, pageSize);
        if (!response.Success)
        return BadRequest(response);

        return Ok(response);
    }
    [HttpGet("{id}")]
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
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string name)
    {
        var response = await _productService.SearchByNameAsync(name);

        if (!response.Success)
        return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("category/{categoryName}")]
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

    [HttpGet("filter")]
    public async Task<IActionResult> GetByPriceRange([FromQuery] decimal minPrice,[FromQuery] decimal maxPrice,[FromQuery] int pageNo = 1,[FromQuery] int pageSize = 10)
    {
        var response = await _productService.GetByPriceRangeAsync(minPrice, maxPrice, pageNo, pageSize);

        if (!response.Success)
        return BadRequest(response);

        return Ok(response);
    }

     [HttpPost]
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
    
    [HttpPost("bulk")]
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductDto dto)
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

    [HttpDelete("{id}")]
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