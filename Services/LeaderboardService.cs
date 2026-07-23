using RealtimeLeaderboardAPI.Domain;
using RealtimeLeaderboardAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Formats.Asn1;

namespace RealtimeLeaderboardAPI.Application;

public class LeaderboardService {
    private readonly RealtimeLeaderboardAPIDbContext _context;
    private readonly IDatabase _redisDB;

    public LeaderboardService(IConnectionMultiplexer redisConnectionMultiplexer, RealtimeLeaderboardAPIDbContext context) {
        _redisDB = redisConnectionMultiplexer.GetDatabase();
        _context = context;
    }

    public async Task ResetLeaderboardDueToNewSeason(int gameId) {
        await _redisDB.KeyDeleteAsync($"gameId:{gameId}");
    }  

    public async Task SeedLeaderboardDueToRedisRestart(int gameId, int seasonId, CancellationToken cancellationToken) {
        var theSeason = await _context.SeasonTable
            .AsNoTracking()
            .Where(x => x.Id == seasonId)
            .Select(x => x.SeasonStatus)
            .FirstOrDefaultAsync(cancellationToken);

        if (theSeason != SeasonStatus.Active)
            throw new InvalidOperationException("Season non-active");
        
        var data = await _context.ScoreTable
            .AsNoTracking()
            .Where(x => x.GameId == gameId && x.SeasonId == seasonId)
            .Select(x => new {
                x.User.Name,
                x.CurrentRankingScore,
            })
            .ToListAsync(cancellationToken);

        if (!data.Any())
            throw new NotFoundException("Score not found");

        var entries = data
            .Select(x => new SortedSetEntry(x.Name, x.CurrentRankingScore))
            .ToArray();

        await _redisDB.SortedSetAddAsync($"gameId:{gameId}", entries);
    }

    public async Task<LeaderboardListResponse> GetTopPlayers(int gameId, int page, int limit, CancellationToken cancellationToken) {
        limit = Math.Min(limit, 100);
        
        var entries = await _redisDB.SortedSetRangeByRankWithScoresAsync(
            $"gameId:{gameId}",
            (page - 1) * limit,
            page * limit - 1,
            Order.Descending
        );

        var result = entries
            .Select((x, index) => 
                new ReadLeaderboardDto {
                    GameId = gameId,
                    UserName = x.Element.ToString(),
                    RankingScore = x.Score,
                    Place = index + 1
                })
            .ToList();

        return new LeaderboardListResponse {Items = result};

    }

    public async Task<ReadLeaderboardDto> GetOnePlayer(int gameId, int userId, CancellationToken cancellationToken) {
        var theUser = await _context.UserTable
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (theUser == null)
            throw new NotFoundException("User not found");

        var scoreTask = _redisDB.SortedSetScoreAsync($"gameId:{gameId}", theUser);
        var rankTask = _redisDB.SortedSetRankAsync($"gameId:{gameId}", theUser, Order.Descending);

        await Task.WhenAll(scoreTask, rankTask);

        var score = await scoreTask;
        var rank = await rankTask;

        return new ReadLeaderboardDto {
            GameId = gameId,
            UserName = theUser,
            RankingScore = score ?? 0.0,
            Place = rank.HasValue ? (int)rank.Value + 1 : null,
        };
    }

    public async Task<LeaderboardListResponse> GetTopPlayersFromPastSeasons(int gameId, int seasonId, int page, int limit, CancellationToken cancellationToken) {
        var theSeason = await _context.SeasonTable
            .AsNoTracking()
            .Where(x => x.Id == seasonId)
            .Select(x => x.SeasonStatus)
            .FirstOrDefaultAsync(cancellationToken);

        if (theSeason != SeasonStatus.Finished)
            throw new InvalidOperationException("Season non-finished");

        limit = Math.Min(limit, 100);

        var result = await _context.ArchivedLeaderboardTable
            .AsNoTracking()
            .Where(x => x.GameId == gameId && x.SeasonId == seasonId)
            .OrderBy(x => x.Place)
            .ThenBy(x => x.UserName)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(x => new ReadLeaderboardDto {
                GameId = gameId,
                UserName = x.UserName,
                RankingScore = x.FinalRankingScore,
                Place = x.Place
            })
            .ToListAsync(cancellationToken);

        return new LeaderboardListResponse {Items = result};

    }
}

