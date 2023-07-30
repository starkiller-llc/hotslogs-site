namespace HotsLogsApi.BL.Migration.Default;

public class CharacterStatisticsViewData
{
    public string HeroPortraitURL { get; set; }
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesBanned { get; set; }
    public string Popularity { get; set; }
    public string WinPercent { get; set; }
    public decimal? WinPercentDelta { get; set; }
    public string Role { get; set; }
    public string AliasCSV { get; set; }
}
