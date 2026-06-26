using StockFlow.API.DTOs;
using StockFlow.API.Models;
namespace StockFlow.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> RegisterAsync(RegisterDto dto);
        Task<ApiResponse<string>> LoginAsync(LoginDto dto);
        Task<ApiResponse<Dictionary<string, string>>> GetUserNamesAsync();
    }
}
