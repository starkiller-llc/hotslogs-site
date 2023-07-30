namespace HotsLogsApi.BL.Migration.Models;

public class RadChartDataRow<TX, TY>
{
    public TX X { get; set; }
    public TY WinPercent { get; set; }
    public int? GamesPlayed { get; set; }
}
