namespace RealtimeLeaderboardAPI.Domain;

public class Score {
    public int Id {get; set;}

    public int GameId {get; set;}
    
    public int SeasonId {get; set;}
    public Season Season {get; set;}
    
    public int UserId  {get; set;}
    public User User {get; set;}

    public double CurrentRankingScore {get; set;}
    public double HighestRankingScore {get; set;}

    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;

}