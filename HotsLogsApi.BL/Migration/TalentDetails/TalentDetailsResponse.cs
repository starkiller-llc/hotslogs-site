using HelperCore.RedisPOCOClasses;
using HotsLogsApi.BL.Migration.TalentDetails.Models;
using HotsLogsApi.Models;

namespace HotsLogsApi.BL.Migration.TalentDetails;

public class TalentDetailsResponse
{
    public TalentDetailsRow<int>[] TalentStatistics { get; set; }
    public PopularTalentBuildsRow[] PopularTalentBuilds { get; set; }
    public HeroTalentBuildStatistic[] TalentBuildStatistics { get; set; }
    public bool RecentPatchNotesVisible { get; set; }
    public string WinRatesByDate { get; set; }
    public string WinRatesByGameLength { get; set; }
    public WinRateWithOrVsRow[] WinRateVs { get; set; }
    public WinRateWithOrVsRow[] WinRateWith { get; set; }
    public MapStatsRow[] MapStatistics { get; set; }
    public string WinRateByHeroLevel { get; set; }
    public string WinRateByTalentUpgrade { get; set; }
    public UpgradeEventRow[] TalentUpgradeTypes { get; set; }
    public GameEventTeam[] Teams { get; set; }
}
