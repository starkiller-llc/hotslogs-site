using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamProfilePlayerAverageReplayCharacterUpgradeEventReplayLengthPercents
{
    public int PID { get; set; }
    public int T { get; set; }
    public int V { get; set; }
    public decimal P { get; set; }

    public int GP { get; set; }
    public int GW { get; set; }
}