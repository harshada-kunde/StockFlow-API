using StockFlow.API.DTOs;

namespace StockFlow.API.ValidationService.Interfaces;

public interface ICategoryValidation
{
    Task<List<string>> ValidateCreateAsync(CategoryDto category);
    Task<List<string>> ValidateUpdateAsync(int id, CategoryDto category);
    Task<List<string>> ValidateDeleteAsync(int id);
}