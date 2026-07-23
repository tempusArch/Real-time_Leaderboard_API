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
public class SeasonController : ControllerBase {
    private readonly SeasonService _seasonService;
    private readonly HttpContextService _httpContextService;
    public SeasonController(SeasonService seasonService, HttpContextService httpContextService) {
        _seasonService = seasonService;
        _httpContextService = httpContextService;
    }

    [HttpPost]
    public async Task<ActionResult<Season>> Createseason(CreateSeasonDto dto, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();

        var result = await _seasonService.CreateSeason(dto, cancellationToken);

        return Created(string.Empty, result);
    }

    [HttpPut("{seasonId}")]
    public async Task<ActionResult<Season>> Updateseason(int seasonId, UpdateSeasonDto dto, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();
        
        return Ok(await _seasonService.UpdateSeason(seasonId, dto, cancellationToken));
    }

    [HttpDelete("{seasonId}")]
    public async Task<IActionResult> DeleteSeason(int seasonId, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();
                    
        await _seasonService.DeleteSeason(seasonId, cancellationToken);

        return NoContent();
    }

    [HttpPost("active/{seasonId}")]
    public async Task<IActionResult> ActiveSeanson(int seasonId, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();
                    
        await _seasonService.ActiveSeason(seasonId, cancellationToken);

        return Ok();
    }

    [HttpPost("finish/{seasonId}")]
    public async Task<IActionResult> FinishSeanson(int seasonId, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();
                    
        await _seasonService.FinishSeason(seasonId, cancellationToken);

        return Ok();
    }

    [HttpGet("{seasonId}")]
    public async Task<ActionResult<Season>> GetOneSeason(int seasonId, CancellationToken cancellationToken) {
        _httpContextService.CheckUserIdClaim();

        return Ok(await _seasonService.GetOneSeason(seasonId, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<Season>>> GetAllseasons(
        int gameId,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10
    ) {
        _httpContextService.CheckUserIdClaim();
        
        return Ok(await _seasonService.GetAllSeasons(gameId, page, limit,cancellationToken));
    }



    
}