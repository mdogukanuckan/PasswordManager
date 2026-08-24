using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.DTOs.VaultItem;
using PasswordManager.Application.Interfaces.Services;

namespace PasswordManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class VaultItemController : ControllerBase
{
    private readonly IVaultItemService _vaultItemService;
    public VaultItemController(IVaultItemService vaultItemService)
    {
        _vaultItemService = vaultItemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _vaultItemService.GetAllAsync(userId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _vaultItemService.GetByIdAsync(id, userId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVaultItemRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _vaultItemService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id,UpdateVaultItemRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _vaultItemService.UpdateAsync(id,userId,request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _vaultItemService.DeleteAsync(id,userId);
        return NoContent();
    }
}