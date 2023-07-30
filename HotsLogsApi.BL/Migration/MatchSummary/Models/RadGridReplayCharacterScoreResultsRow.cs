using System;

namespace HotsLogsApi.BL.Migration.MatchSummary.Models;

public class RadGridReplayCharacterScoreResultsRow
{
    public int? PlayerID { get; set; }
    public string PlayerName { get; set; }
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public bool Team { get; set; }
    public string Takedowns { get; set; }
    public string SoloKills { get; set; }
    public string Assists { get; set; }
    public string Deaths { get; set; }
    public TimeSpan TimeSpentDead { get; set; }
    public string HeroDamage { get; set; }
    public string SiegeDamage { get; set; }
    public string Healing { get; set; }
    public string SelfHealing { get; set; }
    public string DamageTaken { get; set; }
    public int MercCampCaptures { get; set; }
    public string ExperienceContribution { get; set; }
    public string ScoreTooltip { get; set; }
}
