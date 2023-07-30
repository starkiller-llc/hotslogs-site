using HotsLogsApi.BL.Migration.MatchSummary.Models;
using System;

namespace HotsLogsApi.BL.Migration.MatchSummary;

public class MatchSummaryResponse
{
    public bool PermalinkVisible { get; set; } = true;
    public bool ReplayDownloadVisible { get; set; }
    public string ReplayDownloadHref { get; set; }
    public bool PanelReplayViewerVisible { get; set; }
    public string MapName { get; set; }
    public string PermalinkHref { get; set; }
    public ReplayPlayerRecord[] MatchDetails { get; set; }
    public ScoreResultsTotals[] CharacterScoreResultsTotals { get; set; }
    public HeroBanRow[] HeroBans { get; set; }
    public TalentUpgradesRow[] TalentUpgrades { get; set; }
    public bool LengthPercentAtValue5Hide { get; set; }
    public bool PanelMatchLogVisible { get; set; }
    public string ChartXpSummaryJson { get; set; }
    public TalentUpgradesStacksRow[] TalentUpgradesStacks { get; set; }
    public bool HideTeamObjectivesHeroField { get; set; }
    public Tmp1[] TeamObjectives { get; set; }
    public RadGridReplayCharacterScoreResultsRow[] ScoreResults { get; set; }
    public TimeSpan ReplayLength { get; set; }
    public DateTimeOffset ReplayTime { get; set; }
}
