using HelperCore.RedisPOCOClasses;

namespace HotsLogsApi.BL.Migration.HeroRankings;

public class HeroRankingsResponse
{
    public string LastUpdatedText { get; set; }
    public SitewideHeroLeaderboardStatistic[] Stats { get; set; }
    public int Total { get; set; }
}
