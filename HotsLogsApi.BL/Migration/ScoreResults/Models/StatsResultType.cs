using System;

namespace HotsLogsApi.BL.Migration.ScoreResults.Models;

public class StatsResultType
{
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public string HeroPortraitURL { get; set; }
    public string GamesPlayed { get; set; }
    public string WinPercent { get; set; }
    public string TDRatio { get; set; }
    public string KDRatio { get; set; }
    public string Assists { get; set; }
    public string Takedowns { get; set; }
    public string Deaths { get; set; }
    public string HeroDamage { get; set; }
    public string SiegeDamage { get; set; }
    public string SelfHealing { get; set; }
    public string DamageTaken { get; set; }
    public string ExperienceContribution { get; set; }
    public decimal MercCampCaptures { get; set; }
    public string Healing { get; set; }
    public string SoloKills { get; set; }
    public TimeSpan AverageLength { get; set; }
    public string StructureDamage { get; set; }
    public string MinionDamage { get; set; }
    public string CreepDamage { get; set; }
    public string TownKills { get; set; }
    public string GameMode { get; set; }
    public string Event { get; set; }
}
