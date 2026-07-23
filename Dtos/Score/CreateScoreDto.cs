using System.ComponentModel.DataAnnotations;

namespace RealtimeLeaderboardAPI.Application;

public class CreateScoreDto {
    [Required]
    public int GameId {get; set;}

    [Required]
    public double Value {get; set;}
    
    [Required]
    public int SeasonId {get; set;}
}