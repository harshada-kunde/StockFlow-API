using Microsoft.AspNetCore.Mvc;
using StockFlow.API.DTOs;
using StockFlow.API.Services.Interfaces;

namespace StockFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var response = await _authService.RegisterAsync(dto);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);
        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }
    [HttpGet("profile")]
    public async Task<IActionResult> GetUserNames()
    {
        var response = await _authService.GetUserNamesAsync();
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}