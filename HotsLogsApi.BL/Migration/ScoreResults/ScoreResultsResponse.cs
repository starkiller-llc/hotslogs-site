using HotsLogsApi.BL.Migration.ScoreResults.Models;
using HotsLogsApi.Models;
using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.ScoreResults;

public class ScoreResultsResponse
{
    public GameEventTeam[] Teams { get; set; }
    public string LastUpdatedText { get; set; }
    public StatsResultType[] GeneralStats { get; set; }
    public Dictionary<string, StatsResultType[]> RoleStats { get; set; }
}
