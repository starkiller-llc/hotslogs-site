using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideLeaderboardStatisticsV3
{
    public SitewideLeaderboardStatisticV3[] SitewideLeaderboardStatisticArray { get; set; }
    public int BattleNetRegionId { get; set; }
    public int GameMode { get; set; }
    public int League { get; set; }
    public DateTime LastUpdated { get; set; }
}