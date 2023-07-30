using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamObjectiveRTOSummaryContainer
{
    public TeamObjectiveRTOSummaryRadGrid[] TeamObjectiveRTOSummaryRadGrids { get; set; } =
        Array.Empty<TeamObjectiveRTOSummaryRadGrid>();

    public TeamObjectiveRTOSummaryRadHtmlChart[] TeamObjectiveRTOSummaryRadHtmlCharts { get; set; } =
        Array.Empty<TeamObjectiveRTOSummaryRadHtmlChart>();
}