using System;
using System.Collections.Generic;

namespace HotsAdminConsole;

internal class TeamObjectiveReplay
{
    public int GameMode { get; set; }
    public int MapID { get; set; }
    public TimeSpan ReplayLength { get; set; }
    public List<TeamObjectiveRTO> TeamObjectives { get; set; } = new();
}
