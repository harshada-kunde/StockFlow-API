using StockFlow.API.DTOs;


namespace StockFlow.API.ValidationService.Interfaces;

public interface IProductValidation
{
    Task<List<string>> ValidateCreateAsync(ProductDto dto);
    Task<List<string>> ValidateUpdateAsync(int id, ProductDto dto);
    List<string> ValidateBulkForDuplicates(List<ProductDto> dtos);
}