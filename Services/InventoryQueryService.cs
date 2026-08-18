using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StockFlow.API.Data;
using StockFlow.API.DTOs;
using StockFlow.API.Entities;
using StockFlow.API.Models;
using StockFlow.API.Services.Interfaces;

namespace StockFlow.API.Services;

public class InventoryQueryService : IInventoryQueryService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HttpClient _httpClient;

    public InventoryQueryService(IConfiguration configuration, IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<ApiResponse<List<Product>>> QueryAsync( InventoryQueryDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Query))
                return ApiResponse<List<Product>>.ErrorResponse(
                       "Query cannot be empty.");

            if (dto.Query.Length > 500)
                return ApiResponse<List<Product>>.ErrorResponse(
                       "Query cannot exceed 500 characters.");

            // Step 1 — Call Groq first (no DB involved here)
            var filter = await CallGroqAsync(dto.Query);
            if (filter == null)
                return ApiResponse<List<Product>>.ErrorResponse(
                       "Could not understand your query. " +
                       "Please try rephrasing it.");

            // Step 2 — Create fresh scope for DB query
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                              .GetRequiredService<ApplicationDbContext>();

            var products = await BuildQueryAsync(filter, context);

            if (products.Count == 0)
                return ApiResponse<List<Product>>.SuccessResponse(products,
                       "No products found matching your query.");

            return ApiResponse<List<Product>>.SuccessResponse(products,
                   $"{products.Count} product(s) found.");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<Product>>.ErrorResponse(
                   $"Unexpected error in " +
                   $"{nameof(InventoryQueryService)}." +
                   $"{nameof(QueryAsync)}: {ex.Message}");
        }
    }

    private async Task<InventoryFilterDto?> CallGroqAsync(string userQuery)
    {
        try
        {
            var apiKey = _configuration["GroqSettings:ApiKey"];
            var model = _configuration["GroqSettings:Model"];

            var systemPrompt = """
                You are an inventory query assistant.
                Convert the user's natural language query into a JSON filter.
                
                Return ONLY a valid JSON object with these exact fields 
                (use null for fields not mentioned):
                {
                  "categoryName": "string or null",
                  "minPrice": number or null,
                  "maxPrice": number or null,
                  "minStock": number or null,
                  "maxStock": number or null,
                  "productName": "string or null",
                  "brand": "string or null"
                }
                
                Rules:
                - Return ONLY the JSON object, no explanation, no markdown
                - If user says "low stock" use maxStock: 10
                - If user says "out of stock" use maxStock: 0
                - If user says "in stock" use minStock: 1
                - All string values should be in proper case
                """;

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userQuery }
                },
                temperature = 0.1,
                max_tokens = 200
            };

            var json = JsonSerializer.Serialize(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post,"https://api.groq.com/openai/v1/chat/completions");

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseBody = await response.Content.ReadAsStringAsync();
            var groqResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            var aiMessage = groqResponse
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(aiMessage))
                return null;

            aiMessage = aiMessage
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            return JsonSerializer.Deserialize<InventoryFilterDto>(
                   aiMessage,
                   new JsonSerializerOptions
                   {
                       PropertyNameCaseInsensitive = true
                   });
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<Product>> BuildQueryAsync(InventoryFilterDto filter, ApplicationDbContext context)
    {
        // Fresh context — no conflicts ✅
        var query = context.Products
                           .Include(p => p.Category)
                           .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.CategoryName))
            query = query.Where(p => p.Category != null &&
                     p.Category.Name.ToLower()
                     .Contains(filter.CategoryName.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.ProductName))
            query = query.Where(p => p.Name.ToLower()
                     .Contains(filter.ProductName.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            query = query.Where(p => p.Brand.ToLower()
                     .Contains(filter.Brand.ToLower()));

        if (filter.MinPrice.HasValue)
            query = query.Where(p =>
                    p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p =>
                    p.Price <= filter.MaxPrice.Value);

        if (filter.MinStock.HasValue)
            query = query.Where(p =>
                    p.StockQuantity >= filter.MinStock.Value);

        if (filter.MaxStock.HasValue)
            query = query.Where(p =>
                    p.StockQuantity <= filter.MaxStock.Value);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }
}