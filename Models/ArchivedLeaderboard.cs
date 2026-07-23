namespace RealtimeLeaderboardAPI.Domain;

public class ArchivedLeaderboard {
    public int Id {get; set;}

    public int GameId {get; set;}
    public string GameTitle {get; set;}

    public int SeasonId {get; set;}
    public string SeasonName {get; set;}

    public int UserId {get; set;}
    public string UserName {get; set;}

    public double FinalRankingScore {get; set;}
    public int? Place {get; set;}
}