namespace RealtimeLeaderboardAPI.Domain;

public class Game {
    public int Id {get; set;}

    public string Title {get; set;}
    public List<Season> SeasonRisuto {get; set;} = new();
}