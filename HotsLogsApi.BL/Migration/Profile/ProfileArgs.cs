using HotsLogsApi.BL.Migration.Helpers;
using JetBrains.Annotations;

namespace HotsLogsApi.BL.Migration.Profile;

public class ProfileArgs : HelperArgsBase
{
    public int? PlayerId { get; set; }
    public int? EventId { get; set; }
    public string GameModeForMmr { get; set; }
    public new string Time { get; set; }
    public int Tab { get; set; }

    [CanBeNull]
    public string HeroDetails { get; set; }

    [CanBeNull]
    public string MapDetails { get; set; }

    public int? ReplayDetails { get; set; }
}
