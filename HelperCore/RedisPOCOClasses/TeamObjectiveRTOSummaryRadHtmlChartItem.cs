using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamObjectiveRTOSummaryRadHtmlChartItem
{
    public string Tooltip { get; set; }
    public decimal XValue { get; set; }
    public decimal YValue { get; set; }

    public static string FormatTooltipForHtml(
        int gamesPlayed,
        decimal value,
        string valueFormatString,
        string valueTitle)
    {
        return @"<table><tr><td style='text-align: right;'><strong>Games Played: </strong></td><td>" + gamesPlayed +
               @"</td></tr><tr><td style='text-align: right;'><strong>" + valueTitle + @": </strong></td><td>" +
               value.ToString(valueFormatString) + @"</td></tr></table>";
    }
}