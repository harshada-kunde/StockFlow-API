using StockFlow.API.DTOs;
using StockFlow.API.Entities;
using StockFlow.API.Models;
using StockFlow.API.Data.Repositories.Interfaces;
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
        try
        {
            var categories = await _categoryRepository.GetAllAsync();
            return new ApiResponse<List<Category>>(categories);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<Category>>(null, false, $"Unexpected error in {nameof(CategoryService)}.{nameof(GetAllAsync)}: {ex.Message}");
        }
    }

    public async Task<ApiResponse<Category>> GetByIdAsync(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if(category == null)
            {
                return new ApiResponse<Category>(null, false, $"Category with Id {id} does not exist.");
            }
            return new ApiResponse<Category>(category);
        }
        catch (Exception ex)
        {
            return new ApiResponse<Category>(null, false, $"Unexpected error in {nameof(CategoryService)}.{nameof(GetByIdAsync)}: {ex.Message}");
        }
    }

    public async Task<ApiResponse<Category>> CreateAsync(CategoryDto dto)
    {
        try
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
        catch (Exception ex)
        {
            return new ApiResponse<Category>(null, false, $"Unexpected error in {nameof(CategoryService)}.{nameof(CreateAsync)}: {ex.Message}");
        }
    }

    public async Task<ApiResponse<Category>> UpdateAsync(int id, CategoryDto dto)
    {
        try
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
        catch (Exception ex)
        {
            return new ApiResponse<Category>(null, false, $"Unexpected error in {nameof(CategoryService)}.{nameof(UpdateAsync)}: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(int id)
    {
        try
        {
            if (id <= 0)
                return ApiResponse<string>.ErrorResponse("Invalid category ID.");

            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return ApiResponse<string>.ErrorResponse($"Category with ID {id} does not exist.");

            bool hasProducts = await _categoryRepository.HasProductsAsync(id);
            if (hasProducts)
                return ApiResponse<string>.ErrorResponse($"Cannot delete '{category.Name}' because it has products assigned to it.");

            await _categoryRepository.DeleteByIdAsync(id);

            return ApiResponse<string>.SuccessResponse("Deleted", $"Category '{category.Name}' deleted successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResponse($"Unexpected error in {nameof(CategoryService)}.{nameof(DeleteAsync)}: {ex.Message}");
        }   
    }
}

