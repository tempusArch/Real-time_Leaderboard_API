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
public class ScoreController : ControllerBase {
    private readonly ScoreService _scoreService;
    private readonly HttpContextService _httpContextService;
    public ScoreController(ScoreService scoreService, HttpContextService httpContextService) {
        _scoreService = scoreService;
        _httpContextService = httpContextService;
    }

    [HttpPost]
    public async Task<ActionResult<ReadScoreDto>> CreateScore(CreateScoreDto dto, CancellationToken cancellationToken) {
        var userId = _httpContextService.GetCurrentUserId();

        var result = await _scoreService.CreateScore(userId, dto, cancellationToken);

        return Created(string.Empty, result);
    }

    [HttpPut]
    public async Task<ActionResult<ReadScoreDto>> UpdateScore(UpdateScoreDto dto, CancellationToken cancellationToken) {
        var userId = _httpContextService.GetCurrentUserId();

        var result = await _scoreService.UpdateScore(userId, dto, cancellationToken);

        return Created(string.Empty, result);
    }

    [HttpGet]
    public async Task<ActionResult<ReadScoreDto>> GetOneUserScore(int gameId, int seasonId, CancellationToken cancellationToken) {
        var userId = _httpContextService.GetCurrentUserId();

        return Ok(await _scoreService.GetOneUserScore(userId, gameId, seasonId, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteScore(int userId, int gameId, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();
           
        await _scoreService.DeleteScore(userId, gameId, cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("archive/{gameId}")]
    public async Task<ActionResult<ReadScoreDto>> ArchiveFinishedSeasonLeaderboard(int gameId, int seasonId, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();

        await _scoreService.ArchiveFinishedSeasonLeaderboard(gameId, seasonId, cancellationToken);
        
        return Ok();
    }




    
}