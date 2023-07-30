using System.Collections.Generic;

namespace HotsLogsApi.Models;

public class GameEventGamesAndInfo
{
    public GameEvent GameEvent { get; set; }
    public List<GameEventGame> Games { get; set; }
    public List<GameEventTeam> Teams { get; set; }
}
