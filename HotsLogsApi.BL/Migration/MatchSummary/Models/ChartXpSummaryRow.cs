// ReSharper disable InconsistentNaming

namespace HotsLogsApi.BL.Migration.MatchSummary.Models;

public class ChartXpSummaryRow
{
    public int GameTimeMinute { get; set; }
    public int WinnerMinionAndCreepXP { get; set; }
    public int WinnerStructureXP { get; set; }
    public int WinnerHeroXP { get; set; }
    public int WinnerTrickleXP { get; set; }
    public string MinionAndCreepXPTooltip { get; set; }
    public string StructureXPTooltip { get; set; }
    public string HeroXPTooltip { get; set; }
    public string TrickleXPTooltip { get; set; }
    public int LoserMinionAndCreepXP { get; set; }
    public int LoserStructureXP { get; set; }
    public int LoserHeroXP { get; set; }
    public int LoserTrickleXP { get; set; }
}
