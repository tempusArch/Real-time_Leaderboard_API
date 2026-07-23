using System.ComponentModel.DataAnnotations;

namespace RealtimeLeaderboardAPI.Application;

public class UpdateScoreDto {
    [Required]
    public int GameId {get; set;}

    [Required]
    public double ValueChanged {get; set;}

    [Required]
    public int SeasonId {get; set;}

}