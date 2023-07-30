using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService(
    "General Stats",
    KeepRunning = true,
    Sort = 6,
    AutoStart = true,
    Port = 17018)]
public class RedisSitewideCharacterAndMapStatistics : ServiceBase
{
#if !LOCALDEBUG
    public static DateTime Now => DateTime.UtcNow;
#else
    public static DateTime Now => new(2022, 5, 3, 12, 0, 0, DateTimeKind.Utc);
#endif

    public enum Stage
    {
        RestoreBannedPlayers,
        GeneralStatistics,
        SitewideTeamCompositionStatistics,
    }

    private readonly List<string> _applicationEvents = new();
    private StatsHelper _statsHelper;

    protected override async Task RunOnce(CancellationToken token = default)
    {
        await using var scope = Svcp.CreateAsyncScope();
        var heroesEntity = HeroesdataContext.Create(scope);
        var redisClient = MyDbWrapper.Create(scope);

        _statsHelper = new StatsHelper(heroesEntity, redisClient, ConnectionString, Log, token: token);

        Log("Start");

        var init = false;
        State state = null;

        Restore();

        heroesEntity.Database.SetCommandTimeout(MMR.LongCommandTimeout);
        var locDic =
            await heroesEntity.LocalizationAliases.ToDictionaryAsync(
                i => i.IdentifierId,
                i => i.PrimaryName,
                cancellationToken: token);

        #region Restore Banned Players

        if (init && state.Stage != Stage.RestoreBannedPlayers)
        {
            goto skipRestoreBannedPlayers;
        }

        init = false;
        Log("Beginning to Restore Banned Players", debug: true);

        var bannedPlayersRestoredCounter = 0;

        foreach (var bannedPlayer in heroesEntity.PlayerBanneds.ToArray())
        {
            if (!redisClient.ContainsKey($"HOTSLogs:SilencedPlayerID:{bannedPlayer.PlayerId}"))
            {
                if (!redisClient.ContainsKey(
                        $"HOTSLogs:SilencedPlayerIDAndExistingLeaderboardOptOut:{bannedPlayer.PlayerId}") &&
                    bannedPlayer.Player.LeaderboardOptOut != null)
                {
                    heroesEntity.LeaderboardOptOuts.Remove(bannedPlayer.Player.LeaderboardOptOut);
                }

                heroesEntity.PlayerBanneds.Remove(bannedPlayer);
                bannedPlayersRestoredCounter++;
            }
        }

        await heroesEntity.SaveChangesAsync(token);

        Log($"Finished Restoring Banned Players, Restored Accounts: {bannedPlayersRestoredCounter}", debug: true);

        Save(Stage.RestoreBannedPlayers);

        #endregion

        skipRestoreBannedPlayers:

        #region General Statistics

        if (init && state.Stage != Stage.GeneralStatistics)
        {
            goto skipGeneralStatistics;
        }

        init = false;
        redisClient.TrySet(
            "HOTSLogs:GeneralStatistics",
            new GeneralStatistics
            {
                TotalReplaysUploaded = heroesEntity.Replays.Count(),
                LastUpdated = Now,
            },
            TimeSpan.FromDays(30));

        Save(Stage.GeneralStatistics);

        #endregion

        skipGeneralStatistics:

        #region Sitewide Team Composition Statistics

        Log("Beginning to Set Sitewide Team Composition Statistics", debug: true);

        var datetimeBegin = Now.AddDays(-60);
        var datetimeEnd = Now.AddDays(1);

        var heroIdList = heroesEntity.LocalizationAliases
            .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero)
            .Select(i => (int?)i.IdentifierId).OrderBy(i => i).ToList();
        heroIdList.Add(null);
        var mapIdList = heroesEntity.LocalizationAliases
            .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map)
            .Select(i => (int?)i.IdentifierId).OrderBy(i => i).ToList();
        mapIdList.Add(null);

        // Get Sitewide Team Composition Statistics for All Maps, and each Map
        foreach (var mapId in mapIdList)
        {
            if (init && state.MapId.HasValue && mapId != state.MapId)
            {
                AddServiceOutput($"Skipping map id {mapId}");
                continue;
            }

            init = false;
            // Get Sitewide Team Composition Statistics for each Hero Role
            var stats = await _statsHelper.GetSitewideTeamCompositionStatistics(
                datetimeBegin,
                datetimeEnd,
                mapId,
                true,
                "NewGroup");
            _statsHelper.ScrubSitewideTeamCompositionStatistics(stats);

            var mapName = mapId.HasValue ? locDic[mapId.Value] : "-1";

            redisClient.TrySet(
                $"HOTSLogs:SitewideTeamCompositionStatistics:{mapName}:-1:HeroRoles",
                new SitewideTeamCompositionStatistics
                {
                    DateTimeBegin = datetimeBegin,
                    DateTimeEnd = datetimeEnd,
                    MapName = mapId.HasValue ? locDic[mapId.Value] : null,
                    Character = "Hero Roles",
                    LastUpdated = Now,
                    SitewideTeamCompositionStatisticArray = stats
                        .OrderByDescending(i => i.WinPercent).ToArray(),
                },
                TimeSpan.FromDays(30));

            // Get Sitewide Team Composition Statistics for All Heroes, and each Hero
            stats = await _statsHelper.GetSitewideTeamCompositionStatistics(datetimeBegin, datetimeEnd, mapId);
            foreach (var heroId in heroIdList)
            {
                var selStat1 = !heroId.HasValue
                    ? stats
                    : stats.Where(i => i.CharacterNamesCSV.Contains(locDic[heroId.Value]));

                var selStat = selStat1.OrderByDescending(i => i.GamesPlayed).Take(1000).ToArray();

                _statsHelper.ScrubSitewideTeamCompositionStatistics(selStat);

                var heroName = heroId.HasValue ? locDic[heroId.Value] : "-1";

                redisClient.TrySet(
                    $"HOTSLogs:SitewideTeamCompositionStatistics:{mapName}:{heroName}",
                    new SitewideTeamCompositionStatistics
                    {
                        DateTimeBegin = datetimeBegin,
                        DateTimeEnd = datetimeEnd,
                        MapName = mapId.HasValue ? locDic[mapId.Value] : null,
                        Character = heroId.HasValue ? locDic[heroId.Value] : null,
                        LastUpdated = Now,
                        SitewideTeamCompositionStatisticArray = selStat
                            .OrderByDescending(i => i.WinPercent).ToArray(),
                    },
                    TimeSpan.FromDays(30));
            }

            Save(Stage.SitewideTeamCompositionStatistics, mapId);
        }

        Log("Finished Setting Sitewide Team Composition Statistics", debug: true);

        #endregion

        ClearState(scope);

        Log("All Done!");

        void Save(Stage stage, int? mapId = null)
        {
            SaveState(
                scope,
                new State
                {
                    Stage = stage,
                    MapId = mapId,
                });
        }

        void Restore()
        {
            try
            {
                state = RestoreState<State>(scope);
                init = true;
            }
            catch
            {
                /* ignored */
            }
        }
    }

    private void Log(string msg, bool debug = false)
    {
        DataHelper.LogApplicationEvents(msg, ServiceName, debug);
        AddServiceOutput(msg);
    }

    public class State
    {
        public Stage Stage { get; set; }
        public int? MapId { get; set; }
    }

    public RedisSitewideCharacterAndMapStatistics(IServiceProvider svcp) : base(svcp) { }
}
