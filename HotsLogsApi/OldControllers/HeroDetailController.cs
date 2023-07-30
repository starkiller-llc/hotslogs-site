using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.ReplayParser;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class HeroDetailController : ControllerBase
{
    [HttpGet]
    public object GetHeroDetail([FromQuery] string parameter1, [FromQuery] int parameter2)
    {
        var winPercentVsOtherHeroes = DataHelper.RedisCacheGet<SitewideCharacterWinPercentVsOtherCharacters>(
            "HOTSLogs:SitewideCharacterWinPercentVsOtherCharacters:" + parameter2 + ":" +
            (int)GameMode.StormLeague + ":" + parameter1);
        if (winPercentVsOtherHeroes != null)
        {
            return new
            {
                WinPercentVsOtherHeroes = winPercentVsOtherHeroes.SitewideCharacterStatisticArray.Select(
                    i => new
                    {
                        i.HeroPortraitURL,
                        i.Character,
                        i.GamesPlayed,
                        i.AverageLength,
                        i.WinPercent,
                    }),
            };
        }

        return null;
    }
}
