using AutoMapper;
using RealtimeLeaderboardAPI.Domain;
using RealtimeLeaderboardAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace RealtimeLeaderboardAPI.Application;

public class SeasonService {
    private readonly IMapper _mapper;
    private readonly RealtimeLeaderboardAPIDbContext _context;

    public SeasonService(IMapper mapper, RealtimeLeaderboardAPIDbContext context) {
        _mapper = mapper;
        _context = context;
    }

    public async Task<Season> CreateSeason(CreateSeasonDto dto, CancellationToken cancellationToken) {
        var isExisted = await _context.SeasonTable
            .AnyAsync(g => 
                g.Name == dto.Name &&
                g.GameId == dto.GameId, 
                cancellationToken
            );

        if (isExisted)
            throw new InvalidOperationException("Season already existed");

        var theGame = await _context.GameTable
            .AnyAsync(x => x.Id == dto.GameId);

        if (!theGame)
            throw new NotFoundException("Game not found");

        var newOne = _mapper.Map<Season>(dto);

        _context.SeasonTable.Add(newOne);
        await _context.SaveChangesAsync(cancellationToken);

        return newOne;
    }

    public async Task<Season> UpdateSeason(int seasonId, UpdateSeasonDto dto, CancellationToken cancellationToken) {
        var theOne = await _context.SeasonTable
            .FirstOrDefaultAsync(g => g.Id == seasonId);

        if (theOne == null)
            throw new NotFoundException("Season not found");

        _mapper.Map(dto, theOne);
        await _context.SaveChangesAsync(cancellationToken);

        return theOne;
    }

    public async Task DeleteSeason(int SeasonId, CancellationToken cancellationToken) {
        var theOne = await _context.SeasonTable
            .FirstOrDefaultAsync(g => g.Id == SeasonId);

        if (theOne == null)
            throw new NotFoundException("Season not found");

        _context.SeasonTable.Remove(theOne);
        await _context.SaveChangesAsync(cancellationToken);

        return;
    }

    public async Task ActiveSeason(int SeasonId, CancellationToken cancellationToken) {
        var theOne = await _context.SeasonTable
            .FirstOrDefaultAsync(x => 
                x.Id == SeasonId && 
                x.SeasonStatus == SeasonStatus.Upcoming,
                cancellationToken
            );

        if (theOne == null)
            throw new NotFoundException("Season not found");

        if (DateTime.UtcNow >= theOne.StartAt && (DateTime.UtcNow < theOne.EndAt))
            theOne.SeasonStatus = SeasonStatus.Active;
        else
            throw new InvalidOperationException("Season start time not yet");

        await _context.SaveChangesAsync(cancellationToken);

        return;
        
    }

    public async Task FinishSeason(int SeasonId, CancellationToken cancellationToken) {
        var theOne = await _context.SeasonTable
            .FirstOrDefaultAsync(x => 
                x.Id == SeasonId && 
                x.SeasonStatus == SeasonStatus.Active,
                cancellationToken
            );

        if (theOne == null)
            throw new NotFoundException("Season not found");

        if (DateTime.UtcNow >= theOne.EndAt)
            theOne.SeasonStatus = SeasonStatus.Finished;
        else
            throw new InvalidOperationException("Season end time not yet");

        await _context.SaveChangesAsync(cancellationToken);

        return;

        
    }

    public async Task<Season> GetOneSeason(int seasonId, CancellationToken cancellationToken) {
        var theOne = await _context.SeasonTable
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == seasonId, cancellationToken);

        if (theOne == null)
            throw new NotFoundException("Season not found");

        return theOne;
    }

    public async Task<List<Season>> GetAllSeasons(int gameId, int page, int limit, CancellationToken cancellationToken) {
        limit = Math.Min(limit, 100);
        
        var theOne = await _context.SeasonTable
            .AsNoTracking()
            .Where(x => x.GameId == gameId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return theOne;
    }
}