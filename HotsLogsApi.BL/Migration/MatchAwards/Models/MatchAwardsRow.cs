namespace HotsLogsApi.BL.Migration.MatchAwards.Models;

public class MatchAwardsRow
{
    public string HeroPortraitURL { get; set; }
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public string GamesPlayedTotal { get; set; }
    public string GamesPlayedWithAward { get; set; }

    // ReSharper disable once InconsistentNaming
    public string PercentMVP { get; set; }
    public string PercentHighestKillStreak { get; set; }

    // ReSharper disable once InconsistentNaming
    public string PercentMostXPContribution { get; set; }
    public string PercentMostHeroDamageDone { get; set; }
    public string PercentMostSiegeDamageDone { get; set; }
    public string PercentMostDamageTaken { get; set; }
    public string PercentMostHealing { get; set; }
    public string PercentMostStuns { get; set; }
    public string PercentMostMercCampsCaptured { get; set; }
    public string PercentMapSpecific { get; set; }
    public string PercentMostKills { get; set; }
    public string PercentHatTrick { get; set; }
    public string PercentClutchHealer { get; set; }
    public string PercentMostProtection { get; set; }
    public string PercentZeroDeaths { get; set; }
    public string PercentMostRoots { get; set; }
    public string Role { get; set; }

    // ReSharper disable once InconsistentNaming
    public string AliasCSV { get; set; }
    public string GameMode { get; set; }
    public string Event { get; set; }

    public string PercentMostDragonShrinesCaptured { get; set; }
    public string PercentMostCurseDamageDone { get; set; }
    public string PercentMostCoinsPaid { get; set; }
    public string PercentMostSkullsCollected { get; set; }
    public string PercentMostDamageToPlants { get; set; }
    public string PercentMostTimeInTemple { get; set; }
    public string PercentMostGemsTurnedIn { get; set; }
    public string PercentMostImmortalDamage { get; set; }
    public string PercentMostDamageToMinions { get; set; }
    public string PercentMostAltarDamage { get; set; }
    public string PercentMostDamageDoneToZerg { get; set; }
    public string PercentMostNukeDamageDone { get; set; }
}
