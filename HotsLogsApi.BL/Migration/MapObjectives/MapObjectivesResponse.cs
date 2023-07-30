using HotsLogsApi.BL.Migration.MapObjectives.Models;
using HotsLogsApi.Models;
using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.MapObjectives;

public class MapObjectivesResponse
{
    public GameEventTeam[] Teams { get; set; }
    public List<MapObjectivesTable> Tables { get; set; }
    public List<MapObjectivesChart> Charts { get; set; }
}
