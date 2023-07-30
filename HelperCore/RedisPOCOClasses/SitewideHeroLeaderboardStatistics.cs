using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideHeroLeaderboardStatistics
{
    public SitewideHeroLeaderboardStatistic[] SitewideHeroLeaderboardStatisticArray { get; set; }
    public int BattleNetRegionId { get; set; }
    public int CharacterID { get; set; }
    public DateTime LastUpdated { get; set; }
}