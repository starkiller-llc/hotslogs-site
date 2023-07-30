using HotsLogsApi.BL.Migration.MatchAwards.Models;
using HotsLogsApi.Models;

namespace HotsLogsApi.BL.Migration.MatchAwards;

public class MatchAwardsResponse
{
    public GameEventTeam[] Teams { get; set; }
    public string LastUpdatedText { get; set; }
    public MatchAwardsRow[] Stats { get; set; }
    public string Title { get; set; }
    public bool MostRecentDaysVisible { get; set; }
    public bool Unauthorized { get; set; }
}
