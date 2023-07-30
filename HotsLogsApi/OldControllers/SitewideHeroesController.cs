using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.ReplayParser;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class SitewideHeroesController : ControllerBase
{
    [HttpGet]
    public object GetDataByString([FromQuery] string parameter1, [FromQuery] int parameter2)
    {
        var queryString = Request.Query;

        if (!queryString.ContainsKey("GameMode") || !int.TryParse(queryString["GameMode"], out var selectedGameMode))
        {
            selectedGameMode = (int)GameMode.StormLeague;
        }

        return DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
            "HOTSLogs:SitewideCharacterStatisticsV2:" + parameter1 + ":" + parameter2 + ":" +
            selectedGameMode);
    }
}
