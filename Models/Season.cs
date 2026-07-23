using RealtimeLeaderboardAPI.Application;

namespace RealtimeLeaderboardAPI.Domain;

public class Season {
    public int Id {get; set;}
    public string Name {get; set;}

    public int GameId {get; set;}
    public Game Game {get; set;}

    public SeasonStatus SeasonStatus {get; set;} = SeasonStatus.Upcoming;

    public List<Score> ScoreRisuto {get; set;} = new();

    public DateTime StartAt {get; set;}
    public DateTime EndAt {get; set;}


}