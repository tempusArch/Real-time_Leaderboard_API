namespace RealtimeLeaderboardAPI.Application;

public class ReadLeaderboardDto {
    public int GameId {get; set;}
    public string UserName {get; set;}

    public double RankingScore {get; set;}
    public int? Place {get; set;}
}