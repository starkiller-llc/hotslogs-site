using HelperCore;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("MMR Calculation", KeepRunning = true, Sort = 2.9, AutoStart = true, Port = 17008)]
public class MMRProcessRecalc : ServiceBase
{
    private readonly Dictionary<TipName, ConcurrentDictionary<(int battleNetRegionId, int gameMode), DateTime?>>
        _tipRunning =
            new();

    public MMRProcessRecalc(IServiceProvider svcp) : base(svcp)
    {
        _tipRunning[TipName.Old] = new ConcurrentDictionary<(int battleNetRegionId, int gameMode), DateTime?>();
        _tipRunning[TipName.Recent] = new ConcurrentDictionary<(int battleNetRegionId, int gameMode), DateTime?>();
        _tipRunning[TipName.Manual] = new ConcurrentDictionary<(int battleNetRegionId, int gameMode), DateTime?>();
        FillRunDuration = TimeSpan.FromMinutes(5);
    }

    protected override async Task RunOnce(CancellationToken token = default)
    {
        await using var scope = Svcp.CreateAsyncScope();
        // ReSharper disable once ConvertToConstant.Local
        var maxDegreeOfParallelism = DryRun ? 1 : 10;

        void Log(string msg, bool debug = false)
        {
            if (DryRun)
            {
                msg = $"(dryrun) {msg}";
            }

            DataHelper.LogApplicationEvents(msg, "MMR Calc", debug);
        }

        Log("Start");

        await UpdateMMRRecalcAsync(scope, token);

        foreach (var dic in _tipRunning)
        {
            dic.Value.Clear();
        }

        var sw = new Stopwatch();
        sw.Start();
        var pThreadNum = 0;
        var threadRunTime = TimeSpan.FromMinutes(5);
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
        Parallel.ForEach(
            Enumerable.Range(0, 20),
            parallelOptions,
            _ =>
            {
                var threadData = -1;
                var entered = false;
                var work = 0;
                while (sw.Elapsed < threadRunTime && !token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    if (!entered)
                    {
                        entered = true;
                        threadData = Interlocked.Increment(ref pThreadNum);
                    }

                    using var innerScope = Svcp.CreateScope();
                    var dc = HeroesdataContext.Create(innerScope);
                    var workItems = GetWorkItems(dc);

                    if (!workItems.Any())
                    {
                        AddServiceOutput($"Thread {threadData} all caught up");
                        break;
                    }

                    int index;
                    for (index = 0; index < workItems.Count; index++)
                    {
                        var workItemAttempt = workItems[index];
                        var key = (workItemAttempt.BattleNetRegionId, workItemAttempt.GameMode);
                        // ReSharper disable once PossibleInvalidOperationException
                        var date = workItemAttempt.Date.Value;
                        var dic = _tipRunning[workItemAttempt.TipName];
                        if (dic.TryAdd(key, date) || dic.TryUpdate(key, date, null))
                        {
                            break;
                        }
                    }

                    if (index >= workItems.Count)
                    {
                        AddServiceOutput($"Thread {threadData} no free work items, exiting.");
                        break;
                    }

                    var workItem = workItems[index];
                    var battleNetRegionId = workItem.BattleNetRegionId;
                    var gameMode = workItem.GameMode;
                    // ReSharper disable once PossibleInvalidOperationException
                    var replayDateToProcess = workItem.Date.Value.Date;
                    var tipName = workItem.TipName;
                    var concDic = _tipRunning[tipName];

                    try
                    {
                        var name = tipName.ToString();
                        AddServiceOutput(
                            $"Thread {threadData} aggregating MMR {name} region {battleNetRegionId} mode {gameMode} date {replayDateToProcess:M/d/yyyy}");
                        var mmr = innerScope.ServiceProvider.GetRequiredService<MMR>();
                        var awaiter = mmr.AggregateReplayCharacterMMRAsync(
                            battleNetRegionId,
                            gameMode,
                            replayDateToProcess,
                            DryRun,
                            token).GetAwaiter();
                        awaiter.GetResult();

                        var newVal1 = replayDateToProcess.AddDays(1);
                        var newVal = newVal1 > DateTime.Now ? (DateTime?)null : newVal1;
                        switch (tipName)
                        {
                            case TipName.Old:
                                workItem.Ref.TipOld = newVal;
                                break;
                            case TipName.Recent:
                                workItem.Ref.TipRecent = newVal;
                                break;
                            case TipName.Manual:
                                workItem.Ref.TipManual = newVal;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if (!DryRun)
                        {
                            work++;
                            dc.SaveChanges();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        /* handled in while condition */
                    }
                    catch (Exception ex)
                    {
                        AddServiceOutput(
                            $"Thread {threadData} while processing for region {battleNetRegionId} mode {gameMode} date {replayDateToProcess:M/d/yyyy} on ");
                        AddServiceOutput("Error:" + ex);

                        DataHelper.SendServerErrorEmail(
                            "Error processing MMR Recalc " +
                            $"at date {replayDateToProcess:M/d/yyyy} for RegionID: {battleNetRegionId}, GameMode: {gameMode}<br><br>" +
                            $"Exception: {ex}");

                        // Sleep after an error, to make sure we don't rage out database emails
                        Thread.Sleep(TimeSpan.FromMinutes(1));
                    }
                    finally
                    {
                        concDic[(battleNetRegionId, gameMode)] = null;
                    }
                }

                if (entered)
                {
                    AddServiceOutput($"Thread {threadData} done for now (processed {work} days).");
                }
            });
        token.ThrowIfCancellationRequested();
        sw.Stop();

        Log("All Done!");
    }

    private static List<WorkItem> GetWorkItems(HeroesdataContext dc)
    {
        static int SortOrder(TipName tipName) =>
            tipName switch
            {
                TipName.Old => 2,
                TipName.Recent => 0,
                TipName.Manual => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(tipName), tipName, null),
            };

        var q1 = dc.MmrRecalcs.ToList();
        var q = (from x in q1
                let entry1 = new WorkItem
                {
                    BattleNetRegionId = x.BattleNetRegionId,
                    GameMode = x.GameMode,
                    TipName = TipName.Old,
                    Date = x.TipOld,
                    Ref = x,
                }
                let entry2 = new WorkItem
                {
                    BattleNetRegionId = x.BattleNetRegionId,
                    GameMode = x.GameMode,
                    TipName = TipName.Recent,
                    Date = x.TipRecent,
                    Ref = x,
                }
                let entry3 = new WorkItem
                {
                    BattleNetRegionId = x.BattleNetRegionId,
                    GameMode = x.GameMode,
                    TipName = TipName.Manual,
                    Date = x.TipManual,
                    Ref = x,
                }
                select new[] { entry1, entry2, entry3 })
            .AsEnumerable()
            .SelectMany(x => x)
            .Where(x => x.Date.HasValue)
            .OrderBy(i => (SortOrder(i.TipName), Guid.NewGuid()))
            .ToList();
        return q;
    }

    private async Task<List<IGrouping<(int BattleNetRegionId, int GameMode), UncalculatedReplay>>>
        GetUncalculatedReplaysAsync(
            IServiceScope scope,
            DateTime dtStart,
            DateTime dtEnd,
            CancellationToken token = default)
    {
        AddServiceOutput($"Fetching uncalculated replays in range {dtStart:M/d/yyyy} - {dtEnd:M/d/yyyy}");
        var dc = HeroesdataContext.Create(scope);
        dc.Database.SetCommandTimeout(30000);
        // dc.Database.Log = x => Debug.Print(x);

        var gameModes = new[] { 2, 3, 4, 5, 6, 8 };

        var q = from rc in dc.ReplayCharacters
            join r in dc.Replays on rc.ReplayId equals r.ReplayId
            join p in dc.Players on rc.PlayerId equals p.PlayerId
            where !rc.Mmrchange.HasValue && r.TimestampReplay < dtEnd && gameModes.Contains(r.GameMode) &&
                  r.TimestampReplay >= dtStart
            select new UncalculatedReplay
            {
                BattleNetRegionId = p.BattleNetRegionId,
                GameMode = r.GameMode,
                TimestampReplay = r.TimestampReplay,
                ReplayID = r.ReplayId,
            };

        var qq = await q.Distinct().ToListAsync(token);
        return qq.GroupBy(x => (x.BattleNetRegionId, x.GameMode)).ToList();
    }

    private async Task UpdateMMRRecalcAsync(IServiceScope scope, CancellationToken token = default)
    {
        var dtStart = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dtEnd = DateTime.UtcNow.Date.AddDays(1);
        var dtMid = dtEnd.AddDays(-20);
        var older = await GetUncalculatedReplaysAsync(scope, dtStart, dtMid, token);
        var recent = await GetUncalculatedReplaysAsync(scope, dtMid, dtEnd, token);

        var dc = HeroesdataContext.Create(scope);
        var mmrrecalcs =
            await dc.MmrRecalcs.ToDictionaryAsync(x => (x.BattleNetRegionId, x.GameMode), cancellationToken: token);

        for (var i = 0; i < 2; i++)
        {
            var old = i == 0;
            var collection = old ? older : recent;
            var name = old ? "old" : "recent";
            foreach (var uncalc in collection)
            {
                var earliestUncalc = uncalc.Min(x => x.TimestampReplay).Date;
                if (!mmrrecalcs.ContainsKey(uncalc.Key))
                {
                    var newEntry = new MmrRecalc
                    {
                        BattleNetRegionId = uncalc.Key.BattleNetRegionId,
                        GameMode = uncalc.Key.GameMode,
                    };
                    await dc.MmrRecalcs.AddAsync(newEntry, token);
                    mmrrecalcs[uncalc.Key] = newEntry;
                }

                var dbTip = old ? mmrrecalcs[uncalc.Key].TipOld : mmrrecalcs[uncalc.Key].TipRecent;
                if (!dbTip.HasValue || dbTip.Value > earliestUncalc)
                {
                    if (old)
                    {
                        mmrrecalcs[uncalc.Key].TipOld = earliestUncalc;
                    }
                    else
                    {
                        mmrrecalcs[uncalc.Key].TipRecent = earliestUncalc;
                    }

                    AddServiceOutput($"Setting {name} tip for {uncalc.Key} to {earliestUncalc}");
                }
                else
                {
                    AddServiceOutput($"Keeping {name} tip for {uncalc.Key} at {dbTip}");
                }
            }
        }

        if (!DryRun)
        {
            await dc.SaveChangesAsync(token);
        }
    }
}

internal class UncalculatedReplay
{
    public int BattleNetRegionId { get; set; }
    public int GameMode { get; set; }
    public DateTime TimestampReplay { get; set; }
    public int ReplayID { get; set; }
}

public class WorkItem
{
    public int BattleNetRegionId { get; set; }
    public int GameMode { get; set; }
    public TipName TipName { get; set; }
    public DateTime? Date { get; set; }
    public MmrRecalc Ref { get; set; }
}

public enum TipName
{
    Old,
    Recent,
    Manual,
}
