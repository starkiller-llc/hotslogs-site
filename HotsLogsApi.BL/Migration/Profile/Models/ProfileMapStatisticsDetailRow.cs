using System;

namespace HotsLogsApi.BL.Migration.Profile.Models;

public class ProfileMapStatisticsDetailRow
{
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public string GamesPlayed { get; set; }
    public TimeSpan AverageLength { get; set; }
    public string WinPercent { get; set; }
    public string Role { get; set; }
    public string AliasCSV { get; set; }
}
