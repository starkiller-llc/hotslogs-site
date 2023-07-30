namespace HotsLogsApi.BL.Migration.TalentDetails.Models;

public class WinRateWithOrVsRow
{
    public string HeroPortraitURL { get; set; }
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public string GamesPlayed { get; set; }
    public string WinPercent { get; set; }
    public decimal RelativeWinPercent { get; set; }
    public string Role { get; set; }
    public string AliasCSV { get; set; }
}
