using HotsLogsApi.BL.Migration.Helpers;
using JetBrains.Annotations;

namespace HotsLogsApi.BL.Migration.Overview;

public class OverviewArgs : HelperArgsBase
{
    public bool TeamOverview { get; set; }
    public int? PlayerId { get; set; }
    public int? TeamId { get; set; }
    public new string Time { get; set; }
    public string GamesTogether { get; set; }
    public string PartySize { get; set; }
    public int?[] Tab { get; set; }

    [CanBeNull]
    public string HeroDetails { get; set; }

    [CanBeNull]
    public string MapDetails { get; set; }
}
