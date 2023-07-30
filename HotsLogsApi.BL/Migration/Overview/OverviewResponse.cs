using HotsLogsApi.BL.Migration.Overview.Models;
using HotsLogsApi.BL.Migration.ScoreResults.Models;
using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.Overview;

public class OverviewResponse
{
    public TeamStatsResultType[] MatchStats { get; set; }
    public string Title { get; set; }
    public Dictionary<string, TeamStatsResultType[]> RoleStats { get; set; }
    public MapStatisticsRow[] MapStats { get; set; }
    public CharacterStatisticsRow[] HeroStats { get; set; }
    public TalentUpgradesNovaRow[] NovaStats { get; set; }
    public TalentUpgradesGallRow[] GallStats { get; set; }
    public string TeamMembers { get; set; }
    public CharacterStatisticsDetailsRow[] HeroDetails { get; set; }
    public MapStatisticsDetailsRow[] MapDetails { get; set; }
    public bool IsTruncated { get; set; }
}
