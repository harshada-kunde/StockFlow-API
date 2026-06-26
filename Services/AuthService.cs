using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockFlow.API.Data;
using StockFlow.API.DTOs;
using StockFlow.API.Entities;
using StockFlow.API.Models;
using StockFlow.API.Services.Interfaces;
using StockFlow.API.Enums;

namespace StockFlow.API.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ApiResponse<string>> RegisterAsync(RegisterDto dto)
    {
        // Step 1 — Validate input
        if (string.IsNullOrWhiteSpace(dto.Username))
            return ApiResponse<string>.ErrorResponse("Username is required.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            return ApiResponse<string>.ErrorResponse("Password is required.");

        if (string.IsNullOrWhiteSpace(dto.Role.ToString()))
            return ApiResponse<string>.ErrorResponse("Role is required.");

         // Step 2 — Validate role
        if (!Enum.IsDefined(typeof(UserRole), dto.Role))
            return ApiResponse<string>.ErrorResponse("Invalid role selected.");

        // Step 3 — Check username already exists
        bool usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == dto.Username.ToLower());
        if (usernameExists)
            return ApiResponse<string>.ErrorResponse($"Username '{dto.Username}' is already taken.");

        // Step 4 — Hash password with BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Step 5 — Create user
        var user = new User
        {
            Username = dto.Username.Trim(),
            PasswordHash = passwordHash,
            Role = dto.Role.ToString(),
            CreatedOn = DateTime.UtcNow
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse("Registered", $"User '{dto.Username}' registered successfully.");
    }

    public async Task<ApiResponse<string>> LoginAsync(LoginDto dto)
    {
        // Step 1 — Validate input
        if (string.IsNullOrWhiteSpace(dto.Username))
            return ApiResponse<string>.ErrorResponse("Username is required.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            return ApiResponse<string>.ErrorResponse("Password is required.");

        // Step 2 — Find user by username
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower());

        // Step 3 — Generic message for both wrong username AND wrong password
        if (user == null)
            return ApiResponse<string>.ErrorResponse("Invalid username or password.");

        // Step 4 — Verify password with BCrypt
        bool passwordMatch = BCrypt.Net.BCrypt.Verify( dto.Password, user.PasswordHash);

        if (!passwordMatch)
            return ApiResponse<string>.ErrorResponse("Invalid username or password.");

        // Step 5 — Generate JWT token
        string token = GenerateJwtToken(user);

        return ApiResponse<string>.SuccessResponse(token, "Login successful.");
    }

    private string GenerateJwtToken(User user)
    {
        // Step 1 — Get JWT settings from appsettings.json
        var secretKey = _configuration["JwtSettings:SecretKey"]!;
        var issuer = _configuration["JwtSettings:Issuer"]!;
        var audience = _configuration["JwtSettings:Audience"]!;
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"]!);

        // Step 2 — Create security key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        // Step 3 — Create signing credentials
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Step 4 — Define claims (payload data)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        // Step 5 — Create token
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        // Step 6 — Serialize token to string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task<ApiResponse<Dictionary<string, string>>> GetUserNamesAsync()
    {
        var usernames = await _context.Users.Select(u => u.Username).ToListAsync();
        var roles = await _context.Users.Select(u => u.Role).ToListAsync();
        var userDict = usernames.ToDictionary(u => u, u => roles[usernames.IndexOf(u)]);
        return ApiResponse<Dictionary<string, string>>.SuccessResponse(userDict, "Usernames retrieved successfully.");
    }
}