using System;
using System.Collections.Generic;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamObjectiveRTOSummaryRadGrid
{
    public string RadGridTitle { get; set; }
    public string ValueColumnHeaderText { get; set; }
    public string ValueFormatString { get; set; }

    public List<TeamObjectiveRTOSummaryRadGridRow> RadGridRows { get; set; } = new();
}