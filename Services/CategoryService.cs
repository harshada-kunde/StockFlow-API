using StockFlow.API.DTOs;
using StockFlow.API.Entities;
using StockFlow.API.Models;
using StockFlow.API.Repositories.Interfaces;
using StockFlow.API.Services.Interfaces;
using StockFlow.API.ValidationService.Interfaces;

namespace StockFlow.API.Services;
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryValidation _categoryValidation;

    public CategoryService(ICategoryRepository categoryRepository, ICategoryValidation categoryValidation)
    {
        _categoryRepository = categoryRepository;
        _categoryValidation = categoryValidation;
    }

    public async Task<ApiResponse<List<Category>>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return new ApiResponse<List<Category>>(categories);
    }

    public async Task<ApiResponse<Category>> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if(category == null)
        {
            return new ApiResponse<Category>(null, false, $"Category with Id {id} does not exist.");
        }
        return new ApiResponse<Category>(category);
    }

    public async Task<ApiResponse<Category>> CreateAsync(CategoryDto dto)
    {
        var errors = await _categoryValidation.ValidateCreateAsync(dto);

        if(errors.Count > 0)
        {
            return ApiResponse<Category>.ValidationErrorResponse(errors);
        }

        var category = new Category
        {
            Name = dto.Name,
            CreatedOn = DateTime.UtcNow, //DateTime.Now = local timezone , DateTime.UtcNow= universal time , so if we use local time and api get used in different country, createdon will be confusing hence always use UtcNow
            CreatedBy = "System"
        };
        var createdCategory = await _categoryRepository.CreateCategoryAsync(category);
        return ApiResponse<Category>.SuccessResponse(createdCategory, "Category created Successfully");
    }

    public async Task<ApiResponse<Category>> UpdateAsync(int id, CategoryDto dto)
    {
        var errors = await _categoryValidation.ValidateUpdateAsync(id, dto);

        if (errors.Count > 0)
        {
            return ApiResponse<Category>.ValidationErrorResponse(errors);
        }
        var newCategory = new Category
        {
            Id = id,
            Name = dto.Name,
           // Description = dto.Description
        };
        var updatedCategory = await _categoryRepository.UpdateCategoryAsync(id, newCategory);
        return ApiResponse<Category>.SuccessResponse(updatedCategory, "Category updated successfully");
    }

    public async Task<ApiResponse<string>> DeleteAsync(int id)
    {
        var result = await _categoryRepository.DeleteByIdAsync(id);
        return new ApiResponse<string>(default, true, $"Category with ID {id} deleted successfully");
    }
}
