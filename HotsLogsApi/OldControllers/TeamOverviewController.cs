using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class TeamOverviewController : ControllerBase
{
    private readonly EventHelper _eventHelper;

    public TeamOverviewController(EventHelper eventHelper)
    {
        _eventHelper = eventHelper;
    }

    [HttpGet("{id}")]
    public object GetTeamOverview(string id)
    {
        var playerIDs = id.Split(',').Select(i => int.Parse(i)).ToArray();

        if (playerIDs.Length > 10)
        {
            return new { Message = "Please select less than 10 players" };
        }

        var teamProfile = _eventHelper.GetTeamProfile(playerIDs);

        // Sanitize the result while we are developing
        if (teamProfile.Players.Any(i => i.Value.IsLOO == true))
        {
            return new { Message = "Some players in this query have set their profile data to Private" };
        }

        teamProfile.Replays = teamProfile.Replays.Where(
            i => i.GM == (int)GameMode.QuickMatch || i.GM == (int)GameMode.HeroLeague ||
                 i.GM == (int)GameMode.TeamLeague || i.GM == (int)GameMode.StormLeague).ToArray();

        return teamProfile;
    }
}
