using System;
using System.Collections.Generic;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamObjectiveRTOSummaryRadHtmlChart
{
    public string RadHtmlChartTitle { get; set; }
    public string XValueTitle { get; set; }
    public string XValueFormatString { get; set; }
    public string YValueTitle { get; set; }
    public string YValueFormatString { get; set; }

    public List<TeamObjectiveRTOSummaryRadHtmlChartItem> RadHtmlChartItems { get; set; } = new();
}