using HelperCore.RedisPOCOClasses;

namespace HotsLogsApi.BL.Migration.Rankings;

public class RankingsResponse
{
    public string LeaderboardMMRInfoLink { get; set; }
    public string CurrentPlayerLeagueRank { get; set; }
    public string LastUpdatedText { get; set; }
    public string LeagueRequirement { get; set; }
    public SitewideLeaderboardStatisticV3[] Stats { get; set; }
    public int Total { get; set; }
}
