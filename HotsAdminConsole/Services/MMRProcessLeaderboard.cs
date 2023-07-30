using HelperCore;
using Heroes.ReplayParser;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("MMR Leaderboard", KeepRunning = true, Sort = 5, AutoStart = true, Port = 17006)]
public class MMRProcessLeaderboard : ServiceBase
{
    public MMRProcessLeaderboard(IServiceProvider svcp) : base(svcp) { }

    protected override Task RunOnce(CancellationToken token = default)
    {
        // ReSharper disable once ConvertToConstant.Local
        var maxDegreeOfParallelism = DryRun ? 1 : ServiceForm.MMRMaxDegrees;

        var recalculateAggregateForAllPlayers = false;
        {
            const string recalculateAggregateForAllPlayersFileName =
                @"C:\HOTSLogs\RecalculateAggregateForAllPlayers.txt";
            if (File.Exists(recalculateAggregateForAllPlayersFileName))
            {
                recalculateAggregateForAllPlayers = true;
                File.Delete(recalculateAggregateForAllPlayersFileName);
            }
        }

        var priorityGameModes = new Dictionary<int, int>
        {
            { (int)GameMode.StormLeague, 0 },
            { (int)GameMode.QuickMatch, 0 },
            { (int)GameMode.UnrankedDraft, 1 },
        };

        (int prio, Guid Guid) SortOrder((int battleNetRegionId, int gameMode) i)
        {
            var prio = priorityGameModes.ContainsKey(i.gameMode) ? priorityGameModes[i.gameMode] : int.MaxValue;
            return (prio, Guid.NewGuid());
        }

        var regionGameModes =
            DataHelper.BattleNetRegionNames.Keys
                .SelectMany(
                    _ => DataHelper.GameModeWithMMR,
                    (battleNetRegionId, gameMode) => (battleNetRegionId, gameMode))
                .OrderBy(SortOrder);

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
        Parallel.ForEach(
            regionGameModes,
            parallelOptions,
            threadData =>
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    using var scope = Svcp.CreateScope();
                    var mmr = scope.ServiceProvider.GetRequiredService<MMR>();
                    var awaiter = mmr.UpdatePlayersCurrentMMRAndLeagueRankEligibilityAsync(
                        threadData.battleNetRegionId,
                        threadData.gameMode,
                        recalculateAggregateForAllPlayers,
                        DryRun,
                        token: token,
                        logFunction: AddServiceOutput).GetAwaiter();
                    awaiter.GetResult();
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
            });

        token.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }
}
