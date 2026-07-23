using RealtimeLeaderboardAPI.Domain;
using RealtimeLeaderboardAPI.Application;
using RealtimeLeaderboardAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RealtimeLeaderboardAPI.Controller;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class GameController : ControllerBase {
    private readonly GameService _gameService;
    private readonly HttpContextService _httpContextService;
    public GameController(GameService gameService, HttpContextService httpContextService) {
        _gameService = gameService;
        _httpContextService = httpContextService;
    }

    [HttpPost]
    public async Task<ActionResult<Game>> CreateGame([FromQuery] string gameName, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();

        var result = await _gameService.CreateGame(gameName, cancellationToken);

        return Created(string.Empty, result);
    }

    [HttpPut("{gameId}")]
    public async Task<ActionResult<Game>> UpdateGame(int gameId, [FromQuery] string gameName, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();
        
        return Ok(await _gameService.UpdateGame(gameId, gameName, cancellationToken));
    }

    [HttpDelete("{gameId}")]
    public async Task<IActionResult> DeleteGame(int gameId, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();
                    
        await _gameService.DeleteGame(gameId, cancellationToken);

        return NoContent();
    }

    [HttpGet("{gameId}")]
    public async Task<ActionResult<Game>> GetOneGame(int gameId, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();

        return Ok(await _gameService.GetOneGame(gameId, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<Game>>> GetAllGames(
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10
    ) {
        _httpContextService.CheckUserIdClaim();
        
        return Ok(await _gameService.GetAllGames(page, limit,cancellationToken));
    }



    
}