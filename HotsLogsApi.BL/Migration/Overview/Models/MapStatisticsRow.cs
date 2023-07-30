using System;

namespace HotsLogsApi.BL.Migration.Overview.Models;

public class MapStatisticsRow
{
    public string MapImageURL { get; set; }
    public string Map { get; set; }
    public string MapNameLocalized { get; set; }
    public string GamesPlayed { get; set; }
    public int GamesPlayedValue { get; set; }
    public TimeSpan AverageLength { get; set; }
    public string WinPercent { get; set; }
    public decimal WinPercentValue { get; set; }
}
