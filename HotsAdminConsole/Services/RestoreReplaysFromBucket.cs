using HelperCore;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Heroes.ReplayParser;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Restore Replays From Bucket", KeepRunning = false, Sort = 17, AutoStart = false, Port = 17032)]
public class RestoreReplaysFromBucket : ServiceBase
{
    public RestoreReplaysFromBucket(IServiceProvider svcp) : base(svcp) { }

    protected override Task RunOnce(CancellationToken token = default)
    {
        using var scope = Svcp.CreateScope();
        const bool skipQuery = false;
        const string existingFn = @"c:\hotslogs\existing.csv";

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        const int chunkSize = 1000;
        const string bucketPath = @"d:\buckets\heroesreplays";
        var bucketFilePaths = Directory.GetFiles(bucketPath);
        var bucketFiles = bucketFilePaths.Select(Path.GetFileName).Where(x => x is not null).ToList();
        var existing = new List<string>();

        if (File.Exists(existingFn))
        {
            existing = File.ReadAllLines(existingFn).ToList();
        }

        LocalizationAlias[] locDic;
        var dc = HeroesdataContext.Create(scope);
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!skipQuery)
            {
                // Chunk files by groups of 1000 so that queries aren't too big.
                var missing = bucketFiles.Except(existing).ToList();
                var chunks = missing
                    .Select((x, i) => (x, i))
                    .GroupBy(t => t.i / chunkSize, t => t.x);
                AddServiceOutput($"Querying for {missing.Count} fingerprints");
                var logCount = 0;
                foreach (var chunk in chunks)
                {
                    AddServiceOutput($"Querying files {logCount}-{logCount + chunk.Count() - 1} fingerprints");
                    var files = chunk.ToArray();
                    var q = (from r in dc.Replays
                             where files.Contains(r.Hotsapifingerprint)
                             select r.Hotsapifingerprint).ToList();
                    existing.AddRange(q);
                    logCount += chunkSize;
                }
            }

            locDic = dc.LocalizationAliases.Where(
                i =>
                    i.Type == (int)DataHelper.LocalizationAliasType.Hero ||
                    i.Type == (int)DataHelper.LocalizationAliasType.Map).ToArray();
        }

        File.WriteAllLines(existingFn, existing);

        var replaysToAdd = bucketFiles.Except(existing).ToList();
        AddServiceOutput($"Adding {replaysToAdd.Count} replays");
        AddServiceOutput("Sorting files by descending create times");

        var dicTimes = new Dictionary<string, DateTime>();

        const string timesFn = @"c:\hotslogs\times.csv";

        DateTime? TryParse(string date)
        {
            if (DateTime.TryParse(date, out var dt))
            {
                return dt;
            }

            return null;
        }

        if (File.Exists(timesFn))
        {
            var readLines = File.ReadLines(timesFn);
            var q = from r in readLines
                    let spl = r.Split(',')
                    let nm = spl[0]
                    let dt = TryParse(spl[1])
                    where dt.HasValue
                    select (nm, dt: dt.Value);
            dicTimes = q.ToDictionary(x => x.nm, x => x.dt);
        }

#pragma warning disable CS0162
        // ReSharper disable HeuristicUnreachableCode
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (skipQuery)
        {
            replaysToAdd = dicTimes.Keys.ToList();
        }
        // ReSharper restore HeuristicUnreachableCode
#pragma warning restore CS0162

        DateTime GetFileTime(string s)
        {
            return dicTimes.ContainsKey(s) ? dicTimes[s] : new FileInfo(Path.Combine(bucketPath, s)).CreationTimeUtc;
        }

        var bucketFileDates = (from fn in replaysToAdd
                               let fCreationTime = GetFileTime(fn)
                               orderby fCreationTime descending
                               select (fn, fCreationTime))
            .ToList();

        bucketFileDates.ForEach(x => dicTimes[x.fn!] = x.fCreationTime);
        var lines = (from d in dicTimes
                     select $"{d.Key},{d.Value}")
            .ToArray();
        File.WriteAllLines(timesFn, lines);

        var total = replaysToAdd.Count;
        var pCounter = 0;
        var workChunkSize = 20;
        var chunked = bucketFileDates
            .Select((x, i) => (x, i))
            .GroupBy(x => x.i / workChunkSize)
            .OrderBy(x => x.Key)
            .Select(x => x.Select(y => y.x).ToArray())
            .ToList();
        var lck = new object();
        foreach (var bucketFileDatesChunk in chunked)
        {
            Parallel.ForEach(
                bucketFileDatesChunk,
                p =>
                {
                    var (f, createTime) = p;
                    var counter = Interlocked.Increment(ref pCounter);
                    byte[] bytes;
                    lock (lck)
                    {
                        try
                        {
                            bytes = File.ReadAllBytes(Path.Combine(bucketPath, f));
                        }
                        catch (Exception e)
                        {
                            AddServiceOutput($"Failed to read file {f}: {e}");
                            return;
                        }
                    }

                    using var sw = new StringWriter();
                    try
                    {
                        lock (lck)
                        {
                            AddServiceOutput($"Adding replay {f} ({createTime:M/d/yyyy HH:mm}) ({counter} / {total})");
                        }

                        using var scopeInner = Svcp.CreateScope();
                        var dcInner = HeroesdataContext.Create(scopeInner);
                        var (resultIndicator, _) = DataHelper.AddReplay(
                            dcInner,
                            locDic,
                            bytes,
                            "127.0.0.1",
                            // ReSharper disable once AccessToDisposedClosure
                            logFunction: x => sw.WriteLine(x));

                        if (resultIndicator != DataParser.ReplayParseResult.Success)
                        {
                            lock (lck)
                            {
                                AddServiceOutput($"Failed to add replay {f}: {resultIndicator}");
                                AddServiceOutput(sw.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        lock (lck)
                        {
                            AddServiceOutput($"Exception while adding replay {f}: {e}");
                            if (sw.ToString().Length > 0)
                            {
                                AddServiceOutput(sw.ToString());
                            }
                        }
                    }
                });
            AddServiceOutput("-----");
        }

        return Task.CompletedTask;
    }
}
