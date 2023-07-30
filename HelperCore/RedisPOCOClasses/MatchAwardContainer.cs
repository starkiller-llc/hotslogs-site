using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class MatchAwardContainer
{
    public DateTime DateTimeBegin { get; set; }
    public DateTime DateTimeEnd { get; set; }
    public int League { get; set; }
    public int[] GameModes { get; set; }
    public int? PlayerID { get; set; }
    public DateTime LastUpdated { get; set; }
    public MatchAwardContainerStatisticStandard[] StatisticStandard { get; set; }
    public MatchAwardContainerStatisticMapObjectives[] StatisticMapObjectives { get; set; }
}