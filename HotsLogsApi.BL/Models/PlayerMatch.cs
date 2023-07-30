using System;

namespace HotsLogsApi.Models;

public class PlayerTournamentMatch
{
    public int TournamentId { get; set; }
    public string TournamentName { get; set; }
    public int MatchId { get; set; }
    public DateTime MatchDeadline { get; set; }
    public int RoundNum { get; set; }
    public int? ReplayId { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; }
    public int OppTeamId { get; set; }
    public string OppTeamName { get; set; }
    public int? WonMatch { get; set; }
}
