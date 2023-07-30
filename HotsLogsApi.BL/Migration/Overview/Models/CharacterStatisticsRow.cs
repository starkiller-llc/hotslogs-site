using System;

namespace HotsLogsApi.BL.Migration.Overview.Models;

public class CharacterStatisticsRow
{
    public string HeroPortraitURL { get; set; }
    public string PrimaryName { get; set; }
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public double CharacterLevel { get; set; }
    public string GamesPlayed { get; set; }
    public int GamesPlayedValue { get; set; }
    public TimeSpan AverageLength { get; set; }
    public string WinPercent { get; set; }
    public decimal WinPercentValue { get; set; }
}
