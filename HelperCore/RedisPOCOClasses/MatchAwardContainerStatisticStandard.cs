using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class MatchAwardContainerStatisticStandard
{
    public string Character { get; set; }
    public int GamesPlayedTotal { get; set; }
    public int GamesPlayedWithAward { get; set; }
    public decimal PercentMVP { get; set; }
    public decimal PercentHighestKillStreak { get; set; }
    public decimal PercentMostXPContribution { get; set; }
    public decimal PercentMostHeroDamageDone { get; set; }
    public decimal PercentMostSiegeDamageDone { get; set; }
    public decimal? PercentMostDamageTaken { get; set; }
    public decimal? PercentMostHealing { get; set; }
    public decimal? PercentMostStuns { get; set; }
    public decimal PercentMostMercCampsCaptured { get; set; }
    public decimal PercentMapSpecific { get; set; }
    public decimal PercentMostKills { get; set; }
    public decimal PercentHatTrick { get; set; }
    public decimal? PercentClutchHealer { get; set; }
    public decimal? PercentMostProtection { get; set; }
    public decimal PercentZeroDeaths { get; set; }
    public decimal? PercentMostRoots { get; set; }
}