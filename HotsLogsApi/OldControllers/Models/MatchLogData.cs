using System.Collections.Generic;

namespace HotsLogsApi.OldControllers.Models;

public class MatchLogData
{
    public List<string> Heroes { get; set; } = new List<string>();
    public List<string> TeamObjectiveNames { get; set; } = new List<string>();
    public List<string> TeamObjectiveImages { get; set; } = new List<string>();
    public List<int> XValues { get; set; } = new List<int>();
    public List<double> YValues { get; set; } = new List<double>();
    public List<int> XHeroDeaths { get; set; } = new List<int>();
    public List<double> YHeroDeaths { get; set; } = new List<double>();
    public List<int> XDiff { get; set; }
    public List<int> YDiff { get; set; }
    public List<int> MatchEventTimers { get; set; }
    public List<int> DeathTimers { get; set; }
    public List<string> DeathLabels { get; set; }
    public int MaxXpDifference { get; set; }
    public List<string> TeamObjectiveStyles { get; set; }
    public List<string> HeroDeathStyles { get; set; }
    public List<string> EventLabels { get; set; }
    public int MaxXpDiffTick { get; set; }
}
