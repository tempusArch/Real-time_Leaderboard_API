using RealtimeLeaderboardAPI.Domain;
using RealtimeLeaderboardAPI.Application;
using RealtimeLeaderboardAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RealtimeLeaderboardAPI.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase {
    private readonly UserService _userService;
    private readonly HttpContextService _httpContextService;
    public UserController(UserService userService, HttpContextService httpContextService) {
        _userService = userService;
        _httpContextService = httpContextService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> RegisterUser(RegisterUserDto dto, CancellationToken cancellationToken) {
        return Created(string.Empty, await _userService.RegisterUser(dto, cancellationToken));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginUser(LoginUserDto dto, CancellationToken cancellationToken) {
        var accessToken = await _userService.LoginUser(dto, cancellationToken);

        return Ok(new {Token = accessToken});
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();

        var newAccessToken = await _userService.RefreshUser(cancellationToken);

        return Ok(new { Token = newAccessToken });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();

        await _userService.LogoutUser(cancellationToken);
        
        return NoContent();
    }
    
}