using Microsoft.AspNetCore.Mvc;
using StockFlow.API.DTOs;
using StockFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace StockFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Retrieves all categories.
    /// </summary>
    /// <returns>List of all categories.</returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var response = await _categoryService.GetAllAsync();
        return Ok(response);
    }

    /// <summary>
    /// Retrieves a single category by Category ID.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>The matching category if found.</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _categoryService.GetByIdAsync(id);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// Creates a new category. Requires Admin role.
    /// </summary>
    /// <param name="dto">Category details including name.</param>
    /// <returns>The newly created category.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _categoryService.CreateAsync(dto);
        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), 
               new { id = response.Data!.Id }, response);
    }

    /// <summary>
    /// Updates an existing category. Requires Admin role.
    /// </summary>
    /// <param name="id">The category ID to update.</param>
    /// <param name="dto">Updated category details.</param>
    /// <returns>The updated category.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _categoryService.UpdateAsync(id, dto);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// Deletes a category. Requires Admin role. 
    /// Cannot delete categories that have associated products.
    /// </summary>
    /// <param name="id">The category ID to delete.</param>
    /// <returns>Confirmation message.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _categoryService.DeleteAsync(id);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}