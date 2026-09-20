using System.Security.Claims;
using FinanSmart.Api.Modules.Auth.DTOs;
using FinanSmart.Api.Modules.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Auth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register-client")]
    public async Task<IActionResult> RegisterClient(RegisterClientRequestDto request)
    {
        try { return await authService.RegisterClientAsync(request) ? StatusCode(StatusCodes.Status201Created) : Conflict(new { message = "An account with this email already exists." }); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var response = await authService.LoginAsync(request);
        return response is null ? Unauthorized(new { message = "Invalid email or password." }) : Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponseDto>> Refresh(RefreshTokenRequestDto request)
    {
        var response = await authService.RefreshAsync(request.RefreshToken);
        return response is null ? Unauthorized(new { message = "Invalid or expired refresh token." }) : Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var id = GetUserId();
        if (id is null) return Unauthorized();
        await authService.LogoutAsync(id.Value);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> GetCurrentUser()
    {
        var id = GetUserId();
        if (id is null) return Unauthorized();
        var user = await authService.GetCurrentUserAsync(id.Value);
        return user is null ? Unauthorized() : Ok(user);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
