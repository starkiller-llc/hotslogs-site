using HelperCore;
using Heroes.DataAccessLayer.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Match Awards", Sort = 10, Port = 17014, AutoStart = true, KeepRunning = true)]
public class RedisMatchAwards : ServiceBase
{
    public RedisMatchAwards(IServiceProvider svcp) : base(svcp)
    {
        FillRunDuration = TimeSpan.FromHours(2);
    }

    protected override Task RunOnce(CancellationToken token = default)
    {
        using var scope = Svcp.CreateScope();
        // Get LeagueID list
        int[] leagueIDs;
        var heroesEntity = HeroesdataContext.Create(scope);
        {
            leagueIDs = new[] { -1 }.Concat(heroesEntity.Leagues.Select(i => i.LeagueId).OrderBy(i => i)).ToArray();
        }

        foreach (var gameMode in DataHelper.GameModeWithStatistics)
        {
            foreach (var leagueId in leagueIDs)
            {
                var redisClient = MyDbWrapper.Create(scope);
                redisClient.TrySet(
                    $"HOTSLogs:MatchAwardsV2:{leagueId}:{gameMode}",
                    DataHelper.GetMatchAwardContainer(
                        new[] { gameMode },
                        leagueId,
                        daysOfStatisticsToQuery: ServiceForm.DaysOfStatisticsToQuery),
                    TimeSpan.FromDays(15));
            }
        }

        return Task.CompletedTask;
    }
}
