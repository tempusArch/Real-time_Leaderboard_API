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
public class LeaderboardController : ControllerBase {
    private readonly LeaderboardService _leaderboardService;
    private readonly HttpContextService _httpContextService;
    public LeaderboardController(LeaderboardService leaderboardService, HttpContextService httpContextService) {
        _leaderboardService = leaderboardService;
        _httpContextService = httpContextService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{gameId}")]
    public async Task<IActionResult> ResetLeaderboardDueToNewSeason(int gameId) {
        _httpContextService.CheckUserIdClaim();
        
        await _leaderboardService.ResetLeaderboardDueToNewSeason(gameId);

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{gameId}")]
    public async Task<IActionResult> SeedLeaderboardDueToRedisRestart(int gameId, int seasonId, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();

        await _leaderboardService.SeedLeaderboardDueToRedisRestart(gameId, seasonId, cancellationToken);

        return Ok();
    }

    [HttpGet("top/{gameId}")]
    public async Task<ActionResult<LeaderboardListResponse>> GetTopPlayers(
        int gameId, 
        CancellationToken cancellationToken,        
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10
    ) {
        _httpContextService.CheckUserIdClaim();

        return Ok(await _leaderboardService.GetTopPlayers(gameId, page, limit, cancellationToken));
    }

    [HttpGet("{gameId}")]
    public async Task<ActionResult<ReadLeaderboardDto>> GetOnePlayer(int gameId, CancellationToken cancellationToken) {
        var userId = _httpContextService.GetCurrentUserId();

        return Ok(await _leaderboardService.GetOnePlayer(gameId, userId, cancellationToken));
    }

    [HttpGet("top/pastSeason/{gameId}")]
    public async Task<ActionResult<LeaderboardListResponse>> GetTopPlayersFromPastSeasons(
        int gameId, 
        int seasonId,
        CancellationToken cancellationToken,        
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10
    ) {
        _httpContextService.CheckUserIdClaim();

        return Ok(await _leaderboardService.GetTopPlayersFromPastSeasons(gameId, seasonId, page, limit, cancellationToken));
    }




    
}