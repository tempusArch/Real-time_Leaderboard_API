using RealtimeLeaderboardAPI.Domain;
using RealtimeLeaderboardAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace RealtimeLeaderboardAPI.Application;

public class ScoreService {
    private readonly RealtimeLeaderboardAPIDbContext _context;
    private readonly IDatabase _redisDB;


    public ScoreService(IConnectionMultiplexer redisConnectionMultiplexer, RealtimeLeaderboardAPIDbContext context) {
        _redisDB = redisConnectionMultiplexer.GetDatabase();
        _context = context;
        
    }
    
    public async Task<ReadScoreDto> CreateScore(int userId, CreateScoreDto dto, CancellationToken cancellationToken) {
        var theSeason = await _context.SeasonTable
            .AsNoTracking()
            .Where(x => x.Id == dto.SeasonId)
            .Select(x => (SeasonStatus?)x.SeasonStatus)
            .FirstOrDefaultAsync(cancellationToken);

        if (theSeason == null)
            throw new NotFoundException("Season not found");

        if (theSeason != SeasonStatus.Active)
            throw new InvalidOperationException("Season non-active");
        
        var theGame = await _context.GameTable
            .AnyAsync(x => x.Id == dto.GameId);

        if (!theGame)
            throw new NotFoundException("Game not found");
        
        var theUser = await _context.UserTable
            .AnyAsync(x => x.Id == userId);

        if (!theUser)
            throw new NotFoundException("User not found");

        var newOne = new Score {
            GameId = dto.GameId,
            SeasonId = dto.SeasonId,
            UserId = userId,
            CurrentRankingScore = dto.Value,
            HighestRankingScore = dto.Value,
            
        };

        _context.ScoreTable.Add(newOne);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.ScoreTable
            .AsNoTracking()
            .Where(x => x.Id == newOne.Id && x.UserId == userId)
            .Select(x => new ReadScoreDto {
                ScoreId = newOne.Id,

                GameTitle = x.Season.Game.Title,
                SeasonName = x.Season.Name,
                UserName = x.User.Name,

                CurrentRankingScore = newOne.CurrentRankingScore,
                HighestRankingScore = newOne.HighestRankingScore,
                CreatedAt = newOne.CreatedAt,
                UpdatedAt = newOne.UpdatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            throw new InvalidOperationException("Score creation failed");

        await _redisDB.SortedSetAddAsync($"gameId:{dto.GameId}", result.UserName, dto.Value);

        return result;
    }

    public async Task<ReadScoreDto> UpdateScore(int userId, UpdateScoreDto dto, CancellationToken cancellationToken) {
        var theSeason = await _context.SeasonTable
            .AsNoTracking()
            .Where(x => x.Id == dto.SeasonId)
            .Select(x => (SeasonStatus?)x.SeasonStatus)
            .FirstOrDefaultAsync(cancellationToken);

        if (theSeason == null)
            throw new NotFoundException("Season not found");

        if (theSeason != SeasonStatus.Active)
            throw new InvalidOperationException("Season non-active");      
        
        var theOne = await _context.ScoreTable
            .FirstOrDefaultAsync(x => 
                x.UserId == userId && 
                x.GameId == dto.GameId &&
                x.SeasonId == dto.SeasonId, 
                cancellationToken
            );

        if (theOne == null)
            throw new NotFoundException("Score not found");

        theOne.CurrentRankingScore += dto.ValueChanged;

        if (theOne.CurrentRankingScore < 0)
            theOne.CurrentRankingScore = 0;

        theOne.HighestRankingScore = Math.Max(theOne.HighestRankingScore, theOne.CurrentRankingScore);
        theOne.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.ScoreTable
            .AsNoTracking()
            .Where(x => x.Id == theOne.Id && x.UserId == userId)
            .Select(x => new ReadScoreDto {
                ScoreId = x.Id,

                GameTitle = x.Season.Game.Title,
                SeasonName = x.Season.Name,
                UserName = x.User.Name,

                CurrentRankingScore = theOne.CurrentRankingScore,
                HighestRankingScore = theOne.HighestRankingScore,
                CreatedAt = theOne.CreatedAt,
                UpdatedAt = theOne.UpdatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            throw new InvalidOperationException("Score Update failed");

        await _redisDB.SortedSetAddAsync($"gameId:{dto.GameId}", result.UserName, result.CurrentRankingScore);

        return result;
    }

    public async Task<ReadScoreDto> GetOneUserScore(int userId, int gameId, int seasonId, CancellationToken cancellationToken) {
        var result = await _context.ScoreTable
            .AsNoTracking()
            .Where(x => 
                x.UserId == userId &&
                x.GameId == gameId &&
                x.SeasonId == seasonId)
            .Select(x => new ReadScoreDto {
                ScoreId = x.Id,

                GameTitle = x.Season.Game.Title,
                SeasonName = x.Season.Name,
                UserName = x.User.Name,

                CurrentRankingScore = x.CurrentRankingScore,
                HighestRankingScore = x.HighestRankingScore,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);
            
        if (result == null)
            throw new NotFoundException("Score not found");

        return result;
    }

    public async Task DeleteScore(int userId, int gameId, CancellationToken cancellationToken) {       
        var theOne = await _context.ScoreTable
            .Where(x => x.UserId == userId && x.GameId == gameId)
            .ToListAsync(cancellationToken);

        if (!theOne.Any())
            throw new NotFoundException("Score not found");

        var userName = await _context.UserTable
            .Where(x => x.Id == userId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);
        
        _context.ScoreTable.RemoveRange(theOne);
        await _context.SaveChangesAsync(cancellationToken);

        await _redisDB.SortedSetRemoveAsync($"gameId:{gameId}", userName);

        return;
    }

    public async Task ArchiveFinishedSeasonLeaderboard(int gameId, int seasonId, CancellationToken cancellationToken) {
        var theSeason = await _context.SeasonTable
            .AsNoTracking()
            .Where(x => x.Id == seasonId)
            .Select(x => x.SeasonStatus)
            .FirstOrDefaultAsync(cancellationToken);

        if (theSeason != SeasonStatus.Finished)
            throw new InvalidOperationException("Season non-finished");

        var data = await _context.ScoreTable
            .AsNoTracking()
            .Where(x => x.GameId == gameId && x.SeasonId == seasonId)
            .OrderByDescending(x => x.CurrentRankingScore)
            .ThenBy(x => x.UserId)
            .Select(x => new ArchivedLeaderboard {
                GameId = gameId,
                GameTitle = x.Season.Game.Title,

                SeasonId = seasonId,
                SeasonName = x.Season.Name,

                UserId = x.UserId,
                UserName = x.User.Name,

                FinalRankingScore = x.CurrentRankingScore
            })
            .ToListAsync(cancellationToken);

        var result = data
            .Select((x, index) => {
                x.Place = index + 1;
                return x;
            })
            .ToList();

        _context.ArchivedLeaderboardTable.AddRange(result);
        await _context.SaveChangesAsync(cancellationToken);

        return;
        
    }
}