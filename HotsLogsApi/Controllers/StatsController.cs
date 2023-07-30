using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Microsoft.AspNetCore.Mvc;
using ServiceStackReplacement;
using System.Threading.Tasks;

namespace HotsLogsApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class StatsController : ControllerBase
{
    private readonly HeroesdataContext _dc;
    private readonly MyDbWrapper _redis;

    public StatsController(HeroesdataContext dc, MyDbWrapper redis)
    {
        _dc = dc;
        _redis = redis;
    }

    [HttpGet]
    public async Task<int> GetNumberOfReplays()
    {
        var generalStatistics = await _redis.GetAsync<GeneralStatistics>("HOTSLogs:GeneralStatistics");
        return generalStatistics.TotalReplaysUploaded;
    }
}
