using HelperCore;
using HelperCore.RedisPOCOClasses;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class TalentDetailsController : ControllerBase
{
    [HttpGet]
    public object GetTalentDetail([FromQuery] string parameter1, [FromQuery] string parameter2)
    {
        var queryString = Request.Query;

        if (!queryString.ContainsKey("LeagueID") || !int.TryParse(queryString["LeagueID"], out var selectedLeague))
        {
            selectedLeague = -1;
        }

        if (!queryString.ContainsKey("GameMode") || !int.TryParse(queryString["GameMode"], out var selectedGameMode))
        {
            selectedGameMode = -1;
        }

        return DataHelper.RedisCacheGet<SitewideHeroTalentStatistics>(
            "HOTSLogs:SitewideHeroTalentStatisticsV5:" + parameter1 + ":" + parameter2 + ":" +
            queryString["MapName"] + ":" + selectedLeague + ":" + selectedGameMode);
    }
}
