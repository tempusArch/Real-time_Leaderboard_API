namespace RealtimeLeaderboardAPI.Application;

public class LeaderboardListResponse {
    public IEnumerable<ReadLeaderboardDto> Items {get; set;} = new List<ReadLeaderboardDto>();
    public int TotalCount => Items.Count();
}