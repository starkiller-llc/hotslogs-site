using System;
using System.Collections.Generic;

namespace HotsLogsApi.Models;

public class GameEventGame
{
    public int ReplayId { get; set; }
    public DateTime DateTime { get; set; }
    public int? WinningTeamId { get; set; }
    public int? LosingTeamId { get; set; }
    public int MapId { get; set; }
    public string Map { get; set; }
    public List<GameEventPlayer> WinningPlayers { get; set; }
    public List<GameEventPlayer> LosingPlayers { get; set; }
}
