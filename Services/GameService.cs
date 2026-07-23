using RealtimeLeaderboardAPI.Domain;
using RealtimeLeaderboardAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace RealtimeLeaderboardAPI.Application;

public class GameService {
    private readonly RealtimeLeaderboardAPIDbContext _context;


    public GameService(RealtimeLeaderboardAPIDbContext context) {
        _context = context;
        
    }

    public async Task<Game> CreateGame(string title, CancellationToken cancellationToken) {
        var isExisted = await _context.GameTable.AnyAsync(g => g.Title == title, cancellationToken);

        if (isExisted)
            throw new InvalidOperationException("Game already existed");

        var newOne = new Game {
            Title = title
        };

        _context.GameTable.Add(newOne);
        await _context.SaveChangesAsync(cancellationToken);

        return newOne;
    }

    public async Task<Game> UpdateGame(int gameId, string title, CancellationToken cancellationToken) {
        var isExisted = await _context.GameTable
            .AnyAsync(g => g.Title == title, cancellationToken);

        if (isExisted)
            throw new InvalidOperationException("Game already existed");

        var theOne = await _context.GameTable
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (theOne == null)
            throw new NotFoundException("Game not found");

        theOne.Title = title;
        await _context.SaveChangesAsync(cancellationToken);

        return theOne;
    }

    public async Task DeleteGame(int gameId, CancellationToken cancellationToken) {
        var theOne = await _context.GameTable
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (theOne == null)
            throw new NotFoundException("Game not found");

        _context.GameTable.Remove(theOne);
        await _context.SaveChangesAsync(cancellationToken);

        return;
    }

    public async Task<Game> GetOneGame(int gameId, CancellationToken cancellationToken) {
        var theOne = await _context.GameTable
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId, cancellationToken);

        if (theOne == null)
            throw new NotFoundException("Game not found");

        return theOne;
    }

    public async Task<List<Game>> GetAllGames(int page, int limit, CancellationToken cancellationToken) {
        limit = Math.Min(limit, 100);
        
        var theOne = await _context.GameTable
            .AsNoTracking()
            .OrderBy(x => x.Title)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return theOne;
    }
}