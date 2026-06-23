using System.Net;
using System.Text.Json;
using StockFlow.API.Models;

namespace StockFlow.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env; 
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        ApiResponse<string> response;

        if (_env.IsDevelopment())
        {    
            // Development — show full error details for debugging
            response = new ApiResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = new List<string> { ex.StackTrace ?? string.Empty }
            };
    
        }
        else
        {
           //Producion - hide internal details for security
           response = new ApiResponse<string>
           {
               Success = false,
               Message = "An unexpected error occurred. Please try again later.",
               Data = null,
               Errors = null
           };
        }

        //Serialize to JSON and write to response
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        await context.Response.WriteAsync(json);
    }

}