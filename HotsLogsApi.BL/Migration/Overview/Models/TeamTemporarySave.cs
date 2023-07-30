using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.Overview.Models;

public class TeamTemporarySave
{
    public List<int> players { get; set; }
    public string text { get; set; }
}
