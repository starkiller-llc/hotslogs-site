using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.Models;

public class RadChartDef<TX, TY>
    where TY : struct
    where TX : struct
{
    public string Type { get; set; }
    public string YType { get; set; } = RadChartYType.Percent;
    public string YTitle { get; set; } = "Win Percent";
    public IEnumerable<RadChartDataRow<TX, TY>> Data { get; set; }
    public TY? MinY { get; set; }
    public TY? MaxY { get; set; }
    public string Name { get; set; }
    public TY? SuggestedMinY { get; set; }
    public TY? SuggestedMaxY { get; set; }
}
