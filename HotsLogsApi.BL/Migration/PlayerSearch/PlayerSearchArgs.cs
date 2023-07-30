using HotsLogsApi.BL.Migration.Models;

namespace HotsLogsApi.BL.Migration.PlayerSearch;

public class PlayerSearchArgs
{
    public string Name { get; set; }
    public PageArgs Page { get; set; }
}
