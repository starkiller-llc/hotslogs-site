using HotsLogsApi.BL.Migration.Reputation.Models;

namespace HotsLogsApi.BL.Migration.Reputation;

public class ReputationResponse
{
    public ReputationLeaderboardEntry[] Stats { get; set; }
    public int Total { get; set; }
}
