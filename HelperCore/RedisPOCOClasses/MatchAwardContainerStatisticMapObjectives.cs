using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class MatchAwardContainerStatisticMapObjectives
{
    public string Character { get; set; }
    public int GamesPlayedTotal { get; set; }
    public int GamesPlayedWithAward { get; set; }
    public decimal? PercentMostImmortalDamage { get; set; }
    public decimal? PercentMostCoinsPaid { get; set; }
    public decimal? PercentMostCurseDamageDone { get; set; }
    public decimal? PercentMostDragonShrinesCaptured { get; set; }
    public decimal? PercentMostDamageToPlants { get; set; }
    public decimal? PercentMostDamageToMinions { get; set; }
    public decimal? PercentMostTimeInTemple { get; set; }
    public decimal? PercentMostGemsTurnedIn { get; set; }
    public decimal? PercentMostAltarDamage { get; set; }
    public decimal? PercentMostDamageDoneToZerg { get; set; }
    public decimal? PercentMostNukeDamageDone { get; set; }
    public decimal? PercentMostSkullsCollected { get; set; }
    public decimal? PercentMostTimePushing { get; set; }
    public decimal? PercentMostTimeOnPoint { get; set; }
    public decimal? PercentMostInterruptedCageUnlocks { get; set; }
}