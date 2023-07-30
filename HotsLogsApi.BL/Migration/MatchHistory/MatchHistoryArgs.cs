using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.Models;

namespace HotsLogsApi.BL.Migration.MatchHistory;

public class MatchHistoryArgs : HelperArgsBase
{
    public int? PlayerId { get; set; }
    public int? EventId { get; set; }
    public int[] OtherPlayerIds { get; set; }
    public string Filter { get; set; }
    public PageArgs Page { get; set; }
    public SortArgs Sort { get; set; }
}
