using System;

namespace HotsLogsApi.BL.Migration.PlayerSearch.Models;

public class PlayerSearchResult
{
    public int PlayerID { get; set; }
    public string Region { get; set; }
    public string Name { get; set; }
    public string LeageName { get; set; }
    public int? CurrentMMR { get; set; }
    public int? GamesPlayed { get; set; }
    public DateTime? PremiumSupporterSince { get; set; }
}
