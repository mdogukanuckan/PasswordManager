using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.DTOs.Auth;
using PasswordManager.Application.Interfaces.Services;

namespace PasswordManager.API.Controllers;

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
    public async Task<IActionResult> Register( RegisterRequest request)
    {
        var  result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpGet("salt")]
    public async Task<IActionResult> GetSalt([FromQuery] string email)
    {
        var result = await _authService.GetSaltAsync(email);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken);
        return Ok(result);
    }
}