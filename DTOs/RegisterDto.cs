using StockFlow.API.Enums;

namespace StockFlow.API.DTOs;
public class RegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}