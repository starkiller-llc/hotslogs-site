using DiscordWebhooks;
using HelperCore;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using HotsAdminConsole.Services;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ServiceStackReplacement;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

// ReSharper disable LocalizableElement

namespace HotsAdminConsole.Test;

[TestClass]
public class UnitTest1
{
    private string _connString;
    private IServiceProvider _svcp;

    [ClassInitialize]
    public void Init()
    {
        var services = new ServiceCollection();
        _connString =
            "server=localhost;port=3306;user id=root;password=yourpassword;persistsecurityinfo=True;database=HeroesData";

        services.AddDbContext<HeroesdataContext>(
            opts => opts.UseMySql(
                _connString,
                ServerVersion.AutoDetect(_connString)));

        services.AddScoped<StatsHelper>();
        services.AddScoped<MMR>();
        services.AddScoped<MMRProcessRecalc>();
        services.AddScoped<RedisSitewideCharacterAndMapStatisticsForStormLeague>();
        services.AddScoped<RestoreReplaysFromBucket>();

        services.Configure<HotsLogsOptions>(
            opts =>
            {
                opts.DiscordNodeHook = "foo";
            });
    }

    [TestMethod]
    public void TestAddReplay()
    {
        using var scope = _svcp.CreateScope();
        var svcp = scope.ServiceProvider;
        var dc = svcp.GetRequiredService<HeroesdataContext>();
        var locDic = dc.LocalizationAliases.ToArray();
        var bytes = File.ReadAllBytes(@"c:\myprojects\hotslogs\testreplays\a936d6a0-c21e-53bb-3681-d34fc86809af");
        DataHelper.AddReplay(dc, locDic, bytes, "127.0.0.1", logFunction: x => Debug.Print(x));
    }

    [TestMethod]
    public void TestFindDupes()
    {
        using var scope = _svcp.CreateScope();
        var svcp = scope.ServiceProvider;
        var connectionString = _connString;
        var dc = svcp.GetRequiredService<HeroesdataContext>();
        var redis = svcp.GetRequiredService<MyDbWrapper>();
        var statsHelper = new StatsHelper(dc, redis, connectionString, (x, _) => Debug.Print(x));
        // var dtStart = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // var dtStart = new DateTime(2019, 12, 1, 0, 0, 0, DateTimeKind.Utc);
        // var dtEnd = dtStart.AddDays(1);
        var dtTerminate = new DateTime(2015, 1, 1, 0, 0, 0);
        var dtEnd = DateTime.Now;
        var dtStart = dtEnd.AddDays(-1);
        while (dtEnd > dtTerminate)
        {
            Debug.Print($"{dtStart:g}");
            var dupesFound = statsHelper.CheckDupes(dtStart, dtEnd);
            Debug.Print($"{dtStart:g}... found {dupesFound} dupes");
            // dtStart = dtEnd;
            // dtEnd = dtStart.AddDays(1);

            dtEnd = dtStart;
            dtStart = dtStart.AddDays(-1);
        }
    }

    [TestMethod]
    public void TestMMR()
    {
        //MMR.AggregateReplayCharacterMMR(2, 3, new DateTime(2017, 12, 13), true);
        using var scope = _svcp.CreateScope();
        var svcp = scope.ServiceProvider;
        var mmr = svcp.GetRequiredService<MMR>();
        mmr.AggregateReplayCharacterMMR(2, 8, new DateTime(2019, 4, 16), true);
    }

    [TestMethod]
    public async Task TestMMRProcessLeaderboard()
    {
        using var scope = _svcp.CreateScope();
        var svcp = scope.ServiceProvider;
        var svc = svcp.GetRequiredService<MMRProcessLeaderboard>();
        svc.DryRun = true;
        await svc.Run();
    }

    [TestMethod]
    public async Task TestMMRProcessRecalc()
    {
        using var scope = _svcp.CreateScope();
        var svcp = scope.ServiceProvider;
        var svc = svcp.GetRequiredService<MMRProcessRecalc>();
        svc.DryRun = true;
        await svc.Run();
    }

    [TestMethod]
    public async Task TestMMRServiceStormLeague()
    {
        var connectionString = _connString;
        using var scope = _svcp.CreateScope();
        var svcp = scope.ServiceProvider;
        var svc = svcp.GetRequiredService<RedisSitewideCharacterAndMapStatisticsForStormLeague>();
        svc.DryRun = true;
        await svc.Run();
    }

    [TestMethod]
    public async Task TestRestoreReplaysFromBucket()
    {
        using var scope = _svcp.CreateScope();
        var svcp = scope.ServiceProvider;
        var svc = svcp.GetRequiredService<RestoreReplaysFromBucket>();
        svc.DryRun = true;
        await svc.Run();
    }

    [TestMethod]
    public void TestSanityDupCheck()
    {
        var connectionString = _connString;
        using var scope = _svcp.CreateScope();
        var svcp = scope.ServiceProvider;
        var dc = svcp.GetRequiredService<HeroesdataContext>();
        var redis = svcp.GetRequiredService<MyDbWrapper>();
        var statsHelper = new StatsHelper(dc, redis, connectionString, (x, _) => Debug.Print(x));
        // 2020-04-04 18:32:59
        var dt = new DateTime(2018, 2, 9, 20, 11, 02, DateTimeKind.Utc);
        var playerIds = new[]
        {
            12758564, 12844138, 12846970, 13032572, 13150598, 13152884, 13462270, 13471368, 13623534, 14043964,
        };
        var (rc, dups) = DataHelper.SanityDupCheck(
            dc,
            new Replay
            {
                TimestampReplay = dt,
                ReplayCharacters = playerIds.Select(x => new ReplayCharacter { PlayerId = x }).ToList(),
            });
    }

    [TestMethod]
    public void TestSanityDupCheck2()
    {
        var connectionString = _connString;
        // var statsHelper = new StatsHelper(connectionString, x => Debug.Print(x));
        // 2020-04-04 18:32:59
        // var dt = new DateTime(2018, 2, 9, 20, 11, 02, DateTimeKind.Utc);
        using var scope = _svcp.CreateScope();
        var svcp = scope.ServiceProvider;
        var dc = svcp.GetRequiredService<HeroesdataContext>();
        var rep = dc.Replays
            .Include(x => x.ReplayCharacters)
            .Single(x => x.ReplayId == 148363141);
        var (rc, dups) = DataHelper.SanityDupCheck(dc, rep);
    }

    [TestMethod]
    public async Task TestStatsHelperGetSitewideCharacterStatistics()
    {
        var connectionString = _connString;
        using var scope = _svcp.CreateScope();
        var svcp = scope.ServiceProvider;
        var dc = svcp.GetRequiredService<HeroesdataContext>();
        var redis = svcp.GetRequiredService<MyDbWrapper>();
        var statsHelper = new StatsHelper(dc, redis, connectionString, (x, _) => Debug.Print(x));
        var today = DateTime.Today;
        var dateFrom = today.AddDays(-1);
        var dateTo = dateFrom.AddDays(1);
        await statsHelper.GetSitewideCharacterStatistics(dateFrom, dateTo);
    }

    [TestMethod]
    public async Task TestWebhook()
    {
        var sender = _svcp.GetRequiredService<Sender>();
        await sender.SendServiceMessage(
            new ServiceMessage
            {
                ServiceName = "Test",
                Message = "Test",
            });
    }
}
