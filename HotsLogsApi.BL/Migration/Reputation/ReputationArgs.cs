using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.Models;

namespace HotsLogsApi.BL.Migration.Reputation;

public class ReputationArgs : HelperArgsBase
{
    public int Region { get; set; }
    public string Filter { get; set; }
    public PageArgs Page { get; set; }
    public SortArgs Sort { get; set; }
}
