using System.Collections.Generic;

namespace HotsLogsApi.Models;

public class GameEventTeam
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; }
    public List<GameEventPlayer> Players { get; set; }
    public string LogoUrl { get; set; }
}
