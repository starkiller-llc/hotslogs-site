using HelperCore.RedisPOCOClasses;
using HotsLogsApi.BL.Migration.Overview.Models;
using HotsLogsApi.BL.Migration.Profile.Models;
using HotsLogsApi.BL.Migration.TalentDetails.Models;

namespace HotsLogsApi.BL.Migration.Profile;

public class ProfileResponse
{
    public string Title { get; set; }
    public bool Unauthorized { get; set; }
    public string HeaderLinks { get; set; }
    public ProfileCharacterStatisticsRow[] CharacterStats { get; set; }
    public TalentDetailsRow<string>[] HeroDetails { get; set; }
    public MapStatisticsRow[] MapStats { get; set; }
    public MapStatisticsDetailsRow[] MapDetails { get; set; }
    public WinRateWithOrVsRow[] WinRateVsStats { get; set; }
    public WinRateWithOrVsRow[] WinRateWithStats { get; set; }
    public ProfileFriendsRow[] FriendsStats { get; set; }
    public ProfileFriendsRow[] RivalsStats { get; set; }
    public ProfileSharedReplayRow[] ReplaySearchStats { get; set; }
    public ProfileSharedReplayDetailRow[] ReplayDetails { get; set; }
    public string MilestoneChart { get; set; }
    public string WinRateChart { get; set; }
    public PlayerProfileCharacterRoleStatistic[] RoleStats { get; set; }
    public string[][] GeneralInformation { get; set; }
}
