using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class PlayerLeaderboardRanking
{
    public int GameMode { get; set; }
    public int? CurrentMMR { get; set; }
    public int? LeagueID { get; set; }
    public string LeagueName { get; set; }
    public int? LeagueRank { get; set; }
    public int? LeagueRequiredGames { get; set; }
}