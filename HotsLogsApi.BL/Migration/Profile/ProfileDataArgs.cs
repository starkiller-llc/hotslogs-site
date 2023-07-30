using Heroes.ReplayParser;
using System;

namespace HotsLogsApi.BL.Migration.Profile;

public class ProfileDataArgs
{
    public int[] PlayerIdAlts { get; set; }
    public int[] GameModes { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }
    public int? MapId { get; set; }
    public GameMode? GameMode { get; set; }
}
