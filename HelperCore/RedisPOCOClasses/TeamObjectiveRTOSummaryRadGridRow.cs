using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamObjectiveRTOSummaryRadGridRow
{
    public string RowTitle { get; set; }
    public int GamesPlayed { get; set; }
    public decimal Value { get; set; }
}