using HotsLogsApi.BL.Migration.PlayerSearch.Models;

namespace HotsLogsApi.BL.Migration.PlayerSearch;

public class PlayerSearchResponse
{
    public PlayerSearchResult[] Results { get; set; }
    public int Total { get; set; }
}
