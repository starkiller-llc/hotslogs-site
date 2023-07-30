using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.ReplayParser;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class SitewideMapsController : ControllerBase
{
    [HttpGet]
    public object GetDataByString([FromQuery] string parameter1, [FromQuery] int parameter2)
    {
        var queryString = Request.Query;

        if (!queryString.ContainsKey("GameMode") || !int.TryParse(queryString["GameMode"], out var selectedGameMode))
        {
            selectedGameMode = (int)GameMode.StormLeague;
        }

        return DataHelper.RedisCacheGet<SitewideMapStatistics>(
            "HOTSLogs:SitewideMapStatisticsV2:" + parameter1 + ":" + parameter2 + ":" + selectedGameMode);
    }
}
