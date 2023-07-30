using HotsLogsApi.BL.Migration.Helpers;

namespace HotsLogsApi.BL.Migration.MatchAwards;

public class MatchAwardsArgs : HelperArgsBase
{
    public int Type { get; set; }
    public int? PlayerId { get; set; }
}
