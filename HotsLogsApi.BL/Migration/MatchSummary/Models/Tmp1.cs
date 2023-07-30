using System;

namespace HotsLogsApi.BL.Migration.MatchSummary.Models;

public struct Tmp1
{
    public bool Team { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public string PlayerName { get; set; }
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public string HeroPortraitURL { get; set; }
    public string TeamObjectiveType { get; set; }
    public string Value { get; set; }
}
