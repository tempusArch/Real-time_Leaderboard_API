using System.ComponentModel.DataAnnotations;

namespace RealtimeLeaderboardAPI.Application;

public class CreateSeasonDto {
    [Required]
    public string Name {get; set;}

    [Required]
    public int GameId {get; set;}

    [Required]
    public DateTime StartAt {get; set;}
    
    [Required]
    public DateTime EndAt {get; set;}
}