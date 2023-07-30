using System;

namespace HotsLogsApi.BL.Migration.ScoreResults.Models;

public class TeamStatsResultType
{
    public int? PlayerID { get; set; }
    public string PlayerName { get; set; }
    public string GamesPlayed { get; set; }
    public string WinPercent { get; set; }
    public TimeSpan AverageLength { get; set; }
    public FavoriteHeroRow[] FavoriteHeroes { get; set; }
    public string TDRatio { get; set; }
    public string Takedowns { get; set; }
    public string SoloKills { get; set; }
    public string Deaths { get; set; }
    public string HeroDamage { get; set; }
    public string SiegeDamage { get; set; }
    public string Healing { get; set; }
    public string SelfHealing { get; set; }
    public string DamageTaken { get; set; }
    public string MercCampCaptures { get; set; }
    public string ExperienceContribution { get; set; }
    public string KDRatio { get; set; }
    public string Assists { get; set; }
    public string StructureDamage { get; set; }
    public string MinionDamage { get; set; }
    public string CreepDamage { get; set; }
    public string TownKills { get; set; }
}
