namespace HotsLogsApi.BL.Migration.Overview.Models;

public class TalentUpgradesGallRow
{
    public int? PlayerID { get; set; }
    public string PlayerName { get; set; }
    public int GamesPlayed { get; set; }
    public decimal WinPercent { get; set; }
    public decimal ReplayLengthPercentAtValue0 { get; set; }
    public decimal ReplayLengthPercentAtValue1 { get; set; }
    public decimal ReplayLengthPercentAtValue2 { get; set; }
    public decimal ReplayLengthPercentAtValue3 { get; set; }
    public decimal ReplayLengthPercentAtValue4 { get; set; }
}
