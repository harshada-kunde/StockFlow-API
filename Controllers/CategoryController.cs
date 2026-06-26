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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var response = await _categoryService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _categoryService.GetByIdAsync(id);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

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