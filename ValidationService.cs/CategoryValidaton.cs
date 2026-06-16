using StockFlow.API.DTOs;
using StockFlow.API.Repositories.Interfaces;
using StockFlow.API.ValidationService.Interfaces;

namespace StockFlow.API.ValidationService;

public class CategoryValidation : ICategoryValidation
{
    private readonly ICategoryRepository _repository;
     
     public CategoryValidation(ICategoryRepository repository)
     {
         _repository = repository;
     }

     public async Task<List<string>> ValidateCreateAsync(CategoryDto category)
     {
         var errors = new List<string>();

         if (string.IsNullOrWhiteSpace(category.Name))
         {
             errors.Add("Category name is required.");
         }
         if(category.Name.Length > 100)
         {
             errors.Add("Category name must be less than 100 characters.");
         }
         if(category.Name.Length < 2)
         {
             errors.Add("Category name must be at least 2 characters.");
         }
         if (!System.Text.RegularExpressions.Regex.IsMatch(category.Name, @"^[a-zA-Z0-9\s\-_]+$"))
         {
             errors.Add("Category name can only contain letters, numbers, spaces, hyphens and underscores.");
         }
         if(!string.IsNullOrWhiteSpace(category.Name))
         {
            var existingCategory = await _repository.GetByNameAsync(category.Name);
            if(existingCategory != null)
            {
                errors.Add($"Category with Id {existingCategory.Id} and name {existingCategory.Name} already exists.");
            }
         }
         return errors;
     }

     public async Task<List<string>> ValidateUpdateAsync(int id, CategoryDto category)
     {
         var errors = new List<string>();

         if(id <= 0)
         {
            errors.Add("Invalid category Id.");
         }
         if (string.IsNullOrWhiteSpace(category.Name))
         {
             errors.Add("Category name is required.");
         }
         if (category.Name.Length > 100)
         {
             errors.Add("Category name must be less than 100 characters.");
         }
         if (category.Name.Length < 2)
         {
             errors.Add("Category name must be at least 2 characters.");
         }
         if (!System.Text.RegularExpressions.Regex.IsMatch(category.Name, @"^[a-zA-Z0-9\s\-_]+$"))
         {
             errors.Add("Category name can only contain letters, numbers, spaces, hyphens and underscores.");
         }
         if (!string.IsNullOrWhiteSpace(category.Name))
         {
             var existingCategory = await _repository.GetByNameAsync(category.Name);
             if (existingCategory != null && existingCategory.Id != id)
             {
                 errors.Add($"Category with Id {existingCategory.Id} and name {existingCategory.Name} already exists.");
             }
         }
         if(id > 0)
         {
            var existingCategory = await _repository.GetByIdAsync(id);
            if(existingCategory == null)
            {
                errors.Add($"Category with Id {id} does not exist.");
            }
         }
         return errors;
     }

    public async Task<List<string>> ValidateDeleteAsync(int id)
    {
        var errors = new List<string>();

        if (id <= 0)
        {
            errors.Add("Invalid category Id.");
        }
        else
        {
            var existingCategory = await _repository.GetByIdAsync(id);
            if (existingCategory == null)
            {
                errors.Add($"Category with Id {id} does not exist.");
            }
             bool hasProducts = await _repository.HasProductsAsync(id);
             if (hasProducts)
             {
                errors.Add($"Cannot delete '{existingCategory.Name}' because it has " +
                   $"products assigned to it.");
             }

        }

        return errors;
    }
}