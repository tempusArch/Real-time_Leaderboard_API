namespace RealtimeLeaderboardAPI.Application;

public class ReadScoreDto {
    public int ScoreId {get; set;}
    
    public string GameTitle {get; set;}
    public string SeasonName {get; set;}
    public string UserName {get; set;}

    public double CurrentRankingScore {get; set;}
    public double HighestRankingScore {get; set;}

    public DateTime CreatedAt {get; set;} 
    public DateTime UpdatedAt {get; set;}
}