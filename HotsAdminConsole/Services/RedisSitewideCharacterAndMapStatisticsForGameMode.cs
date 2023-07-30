using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.ReplayParser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Newtonsoft.Json;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotsAdminConsole.Services;

// This is a base class for the actual game modes
public class RedisSitewideCharacterAndMapStatisticsForGameMode : ServiceBase
{
    private readonly bool _patches;
    private readonly bool _gameEvent;
    private readonly int _gameMode2;
    private int _eventId;
    private StatsHelper _statsHelper;
    private readonly List<string> _applicationEvents = new();
    private int[] _leagueIDs;
    private int[] _characterIDs;
    private int[] _mapIDs;
    private StatsServiceState _state;
    private Dictionary<int, string> _locDic;
    private HeroesdataContext _db;
    private MyDbWrapper _redis;

    private readonly Dictionary<CacheKey, List<HeroTalentBuildStatistic>> _popularBuildsCache = new();

    private CancellationToken _token;
#if !LOCALDEBUG
        public DateTime Now => DateTime.UtcNow;
#else
    public DateTime Now => new(2022, 5, 3, 12, 0, 0, DateTimeKind.Utc);
#endif

    private string PatchKey => _gameEvent ? "-1" : $"{_state.CurrentBuild}-{_state.UpToBuild}";

    public RedisSitewideCharacterAndMapStatisticsForGameMode(
        IServiceProvider svcp,
        GameMode gameMode,
        bool patches = false,
        bool gameEvent = false) : base(svcp)
    {
        _patches = patches;
        _gameEvent = gameEvent;
        _gameMode2 = (int)gameMode;
        var nowStr = Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    protected override async Task RunOnce(CancellationToken token = default)
    {
        await using var scope = Svcp.CreateAsyncScope();
        _token = token;
        _db = HeroesdataContext.Create(scope);
        _redis = MyDbWrapper.Create(scope);
        _statsHelper = new StatsHelper(_db, _redis, ConnectionString, Log, token: token);
        _popularBuildsCache.Clear();

        _statsHelper.TmpTableFullRetry += (s, e) =>
        {
            Log("Retrying due to MySql tmp table full error.");
        };

        try
        {
            Log("Start");

            // _state = StatsServiceState.Load(ServiceForm.ServiceName);
            _state = new StatsServiceState(ServiceName);

            _db.Database.SetCommandTimeout(MMR.LongCommandTimeout);
            _locDic = _db.LocalizationAliases.ToDictionary(i => i.IdentifierId, i => i.PrimaryName);

            _leagueIDs = _db.Leagues.Select(i => i.LeagueId).OrderBy(i => i).ToArray();
            _characterIDs = _db.LocalizationAliases
                .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero).Select(i => i.IdentifierId)
                .OrderBy(i => i).ToArray();
            _mapIDs = _db.LocalizationAliases
                .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map).Select(i => i.IdentifierId)
                .OrderBy(i => i).ToArray();

            Log($"Beginning to Set Statistics for Game Mode {GetGameMode()}", debug: true);

            _statsHelper.RefreshCaches();

            if (_patches)
            {
                // Get Sitewide Character Statistics, Map Statistics, Character Win Rate, and Talent Statistics since latest minor patch
                InitStage2State();
                await Stage2(scope);
            }
            else if (_gameEvent)
            {
                // Get Sitewide Character Statistics, Map Statistics, Character Win Rate, and Talent Statistics since latest minor patch
                switch (_state.Stage)
                {
                    case 2: goto Stage2Event;
                    case 11: goto Stage11Event;
                }

                InitStage2State();
                Stage2Event:
                await Stage2ForEvents(scope);

                InitStage11State();
                Stage11Event:
                await Stage11();
            }
            else
            {
                switch (_state.Stage)
                {
                    case 1: goto Stage1;
                    case 3: goto Stage3;
                    case 4: goto Stage4;
                    case 5: goto Stage5;
                    case 6: goto Stage6;
                    case 7: goto Stage7;
                    case 8: goto Stage8;
                    case 9: goto Stage9;
                    case 10: goto Stage10;
                    case 11: goto Stage11;
                }

                // Get Sitewide Character and Map Statistics for Last 7 Days for the Home Page
                InitStage1State();
                Stage1:
                await Stage1();

                // Get Sitewide Hero Talent Statistics for the Last 7 Days
                InitStage3State();
                Stage3:
                await Stage3();

                // Get Sitewide Character Statistics, Map Statistics, Character Win Rate, and Talent Statistics for each Week
                InitStage4State();
                Stage4:
                await Stage4();

                // Set Character Win Rate for each Week
                InitStage5State();
                Stage5:
                await Stage5();

                // Sitewide Character Win Percent Vs Other Characters
                InitStage6State();
                Stage6:
                await Stage6();

                // Sitewide Character Win Percent With Other Characters
                InitStage7State();
                Stage7:
                await Stage7();

                // Sitewide Character Win Rate by Game Time
                InitStage8State();
                Stage8:
                await Stage8();

                // Sitewide Character Win Rate by Character Level
                InitStage9State();
                Stage9:
                await Stage9();

                // Sitewide Character Win Rate by Talent Upgrade Event Average Value
                InitStage10State();
                Stage10:
                await Stage10();

                // Map Objective Statistics
                InitStage11State();
                Stage11:
                await Stage11();
            }

            Log($"Finished Setting Statistics for Game Mode {GetGameMode()}", debug: true);

            Log("All Done!");
        }
        // catch (Exception ex)
        // {
        //     throw ex;
        //    // Sleep after an error, to make sure we don't rage out database emails
        //    //Thread.Sleep(TimeSpan.FromMinutes(5));

        //    //DataHelper.SendServerErrorEmail(@"Error updating sitewide character and map statistics<br><br>Exception: " + ex);
        // }
        finally
        {
            await _db.DisposeAsync();
            _redis.Dispose();
        }
    }

    private int GetGameMode()
    {
        return _gameEvent ? _eventId : _gameMode2;
    }

    private void SetEventId(int eventId)
    {
        _eventId = eventId;
    }

    private void InitStage1State()
    {
        _state = new StatsServiceState(ServiceName)
        {
            Stage = 1,
            DtStart = Now.AddDays(-7),
            DtEnd = Now,
            CurrentBuild = _db.Replays.Max(i => i.ReplayBuild),
        };
    }

    private async Task Stage1()
    {
        Log("Get map stats for last 7 days");
        Log(
            $"Beginning to Set Sitewide Character and Map Statistics for Last 7 Days for the Home Page for game mode {GetGameMode()}", debug: true);

        await _statsHelper.SetSitewideCharacterAndMapAndTalentStatistics(
            GetGameMode(),
            _redis,
            _state.DtStart,
            _state.DtEnd,
            _leagueIDs.Union(new[] { -1 }).ToArray(),
            _characterIDs,
            _state.CurrentBuild);
    }

    private void InitStage2State()
    {
        _state = new StatsServiceState(ServiceName)
        {
            Stage = 2,
            DtStart = DateTime.MinValue,
            DtEnd = DateTime.MaxValue,
        };
    }

    public class State
    {
        public string Patch { get; set; }
        public int? League { get; set; }
        public int? Map { get; set; }
        public bool? Glob { get; set; }
        public int? EventId { get; set; }
    }

    private async Task Stage2(IServiceScope scope)
    {
        const int n = 2;
        Log($"Get stats for last {n} minor patches");
        //Log($"Get Sitewide Hero Talent Statistics since last {n} minor patches", debug: true);
        var gameVersions = DataHelper.GetGameVersions();

        var init = false;
        State state = null;

        Restore();

        foreach (var patch in gameVersions)
        {
            if (init && state.Patch != patch.Title)
            {
                AddServiceOutput($"Skipping {patch.Title}");
                continue;
            }

            _popularBuildsCache.Clear();
            _state.CurrentBuild = patch.BuildFirst;
            _state.UpToBuild = patch.BuildLast;

            Log($"Getting Data for patch {patch.Title} (builds {_state.CurrentBuild} - {_state.UpToBuild})", debug: true);

            if (init && state.Glob.HasValue)
            {
                AddServiceOutput("Skipping global stats for patch");
                goto skipGlobal;
            }

            await GetGlobalStats();

            Save(patch: patch.Title, glob: true);
            skipGlobal:

            if (init && state.League.HasValue)
            {
                AddServiceOutput("Skipping maps");
                goto skipMaps;
            }

            foreach (var map in _mapIDs)
            {
                await ThrowIfCancellationRequested();

                if (init && state.Map.HasValue && state.Map != map)
                {
                    AddServiceOutput($"Skipping map {map}");
                    continue;
                }

                init = false;
                await GetMapStats(map);

                Save(patch: patch.Title, glob: null, mapId: map);
            }

            skipMaps:

            foreach (var league in _leagueIDs)
            {
                await ThrowIfCancellationRequested();

                if (init && state.League.HasValue && state.League != league)
                {
                    AddServiceOutput($"Skipping league {league}");
                    continue;
                }

                await GetLeageDataForLeageMapStats(league);

                foreach (var map in _mapIDs)
                {
                    await ThrowIfCancellationRequested();

                    if (init && state.Map.HasValue && state.Map != map)
                    {
                        AddServiceOutput($"Skipping map {map}");
                        continue;
                    }

                    init = false;
                    await GetLeageMapStats(league, map);

                    Save(patch: patch.Title, glob: true, leagueId: league, mapId: map);
                }
            }

            foreach (var league in _leagueIDs)
            {
                await ThrowIfCancellationRequested();

                await GetLeagueStats(league);
            }
        }

        _popularBuildsCache.Clear();

        ClearState(scope);
        Log("Done Setting Sitewide Character and Map Statistics since last minor patch", debug: true);

        void Save(string patch, bool? glob = null, int? leagueId = null, int? mapId = null)
        {
            SaveState(
                scope,
                new State
                {
                    Patch = patch,
                    Glob = glob,
                    League = leagueId,
                    Map = mapId,
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

    private async Task Stage2ForEvents(IServiceScope scope)
    {
        //Log($"Get Sitewide Hero Talent Statistics for event {GetGameMode()}", debug: true);
        var gameEvents = DataHelper.GetGameEvents();

        var init = false;
        State state = null;

        Restore();

        foreach (var gameEvent in gameEvents)
        {
            SetEventId(gameEvent.Id);

            Log($"Get stats for event {GetGameMode()}");

            if (init && state.EventId != gameEvent.Id)
            {
                AddServiceOutput($"Skipping event {gameEvent.Id}: {gameEvent.Name}");
                continue;
            }

            _popularBuildsCache.Clear();
            _state.CurrentBuild = gameEvent.BuildFirst;
            _state.UpToBuild = gameEvent.BuildLast;

            Log(
                $"Getting Data for event {gameEvent.Id}: {gameEvent.Name} (builds {_state.CurrentBuild} - {_state.UpToBuild})", debug: true);

            if (init && state.Glob.HasValue)
            {
                AddServiceOutput("Skipping global stats for patch");
                goto skipGlobal;
            }

            await GetGlobalStats();

            Save(eventId: gameEvent.Id, glob: true);
            skipGlobal:

            foreach (var map in _mapIDs)
            {
                await ThrowIfCancellationRequested();

                if (init && state.Map.HasValue && state.Map != map)
                {
                    AddServiceOutput($"Skipping map {map}");
                    continue;
                }

                init = false;
                await GetMapStats(map);

                Save(eventId: gameEvent.Id, glob: null, mapId: map);
            }
        }

        _popularBuildsCache.Clear();

        ClearState(scope);
        Log("Done Setting Sitewide Character and Map Statistics since last minor patch", debug: true);

        void Save(string patch = null, bool? glob = null, int? leagueId = null, int? mapId = null, int? eventId = null)
        {
            SaveState(
                scope,
                new State
                {
                    Patch = patch,
                    Glob = glob,
                    League = leagueId,
                    Map = mapId,
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

    private async Task GetLeagueStats(int league)
    {
        var keyHero2 =
            $"HOTSLogs:SitewideCharacterStatisticsV2:{PatchKey}:{league}:{GetGameMode()}";

        var valueHero2 = new SitewideCharacterStatistics
        {
            DateTimeBegin = _state.DtStart,
            DateTimeEnd = _state.DtEnd,
            League = league,
            GameMode = GetGameMode(),
            LastUpdated = Now,
            SitewideCharacterStatisticArray = await
                _statsHelper.GetSitewideCharacterStatistics(
                    _state.DtStart,
                    _state.DtEnd,
                    replayBuild: _state.CurrentBuild,
                    upToBuild: _state.UpToBuild,
                    leagueId: league,
                    gameMode: GetGameMode()),
        };
        RedisSet(keyHero2, valueHero2, TimeSpan.FromDays(120));

        var keyMap2 =
            $"HOTSLogs:SitewideMapStatisticsV2:{PatchKey}:{league}:{GetGameMode()}";
        var valueMap2 = new SitewideMapStatistics
        {
            DateTimeBegin = _state.DtStart,
            DateTimeEnd = _state.DtEnd,
            League = league,
            GameMode = GetGameMode(),
            LastUpdated = Now,
            SitewideMapStatisticArray = await
                _statsHelper.GetSitewideMapStatistics(
                    _state.DtStart,
                    _state.DtEnd,
                    replayBuild: _state.CurrentBuild,
                    upToBuild: _state.UpToBuild,
                    leagueId: league,
                    gameMode: GetGameMode()),
        };
        RedisSet(keyMap2, valueMap2, TimeSpan.FromDays(120));
    }

    private async Task GetLeageDataForLeageMapStats(int league)
    {
        Log($"Getting Data for LeagueID: {league}", debug: true);

        var patchLeague = await GetTalentStats(
            replayBuild: _state.CurrentBuild,
            leagueId: league,
            gameModes: new[] { GetGameMode() },
            upToBuildArg: _state.UpToBuild);
        foreach (var hero in _characterIDs)
        {
            await ThrowIfCancellationRequested();
            var key =
                $"HOTSLogs:SitewideHeroTalentStatisticsV5:{PatchKey}:{_locDic[hero]}:-1:{league}:{GetGameMode()}";
            RedisSet(key, await RedisHeroTalentStats(patchLeague, hero, league: league));
        }
    }

    private async Task GetLeageMapStats(int league, int map)
    {
        Log($"Getting Data for LeagueID: {league}, MapID: {map}", debug: true);

        var patchLeagueMap = await GetTalentStats(
            replayBuild: _state.CurrentBuild,
            mapId: map,
            leagueId: league,
            gameModes: new[] { GetGameMode() },
            upToBuildArg: _state.UpToBuild);
        foreach (var hero in _characterIDs)
        {
            await ThrowIfCancellationRequested();
            var key =
                $"HOTSLogs:SitewideHeroTalentStatisticsV5:{PatchKey}:{_locDic[hero]}:{_locDic[map]}:{league}:{GetGameMode()}";
            RedisSet(key, await RedisHeroTalentStats(patchLeagueMap, hero, map, league));
        }
    }

    private async Task GetMapStats(int map)
    {
        Log($"Getting Data for MapID: {map}", debug: true);
        var patchMap = await GetTalentStats(
            replayBuild: _state.CurrentBuild,
            mapId: map,
            gameModes: new[] { GetGameMode() },
            upToBuildArg: _state.UpToBuild);

        foreach (var hero in _characterIDs)
        {
            await ThrowIfCancellationRequested();
            var key =
                $"HOTSLogs:SitewideHeroTalentStatisticsV5:{PatchKey}:{_locDic[hero]}:{_locDic[map]}:-1:{GetGameMode()}";
            RedisSet(key, await RedisHeroTalentStats(patchMap, hero, map));
        }
    }

    private async Task GetGlobalStats()
    {
        {
            var totalGamesPlayed = await DataHelper.CountGames(
                replayBuild: _state.CurrentBuild,
                gameModes: new[] { GetGameMode() },
                upToBuildArg: _state.UpToBuild);
            var key =
                $"HOTSLogs:NumberOfRatedGamesV1:{PatchKey}:-1:-1:-1:{GetGameMode()}";
            RedisSet(key, totalGamesPlayed, TimeSpan.FromDays(120));
        }

        var entirePatch = await GetTalentStats(
            replayBuild: _state.CurrentBuild,
            gameModes: new[] { GetGameMode() },
            upToBuildArg: _state.UpToBuild);

        foreach (var hero in _characterIDs)
        {
            // Get Sitewide Hero Talent Statistics
            var key =
                $"HOTSLogs:SitewideHeroTalentStatisticsV5:{PatchKey}:{_locDic[hero]}:-1:-1:{GetGameMode()}";
            RedisSet(key, await RedisHeroTalentStats(entirePatch, hero), TimeSpan.FromDays(120));
        }

        var keyHero =
            $"HOTSLogs:SitewideCharacterStatisticsV2:{PatchKey}:-1:{GetGameMode()}";
        var valueHero = new SitewideCharacterStatistics
        {
            DateTimeBegin = _state.DtStart,
            DateTimeEnd = _state.DtEnd,
            League = -1,
            GameMode = GetGameMode(),
            LastUpdated = Now,
            SitewideCharacterStatisticArray = await
                _statsHelper.GetSitewideCharacterStatistics(
                    _state.DtStart,
                    _state.DtEnd,
                    replayBuild: _state.CurrentBuild,
                    upToBuild: _state.UpToBuild,
                    leagueId: -1,
                    gameMode: GetGameMode()),
        };
        RedisSet(keyHero, valueHero, TimeSpan.FromDays(45)); //Changed from 120 01/16/2020

        var keyMap = $"HOTSLogs:SitewideMapStatisticsV2:{PatchKey}:-1:{GetGameMode()}";
        var valueMap = new SitewideMapStatistics
        {
            DateTimeBegin = _state.DtStart,
            DateTimeEnd = _state.DtEnd,
            League = -1,
            GameMode = GetGameMode(),
            LastUpdated = Now,
            SitewideMapStatisticArray = await
                _statsHelper.GetSitewideMapStatistics(
                    _state.DtStart,
                    _state.DtEnd,
                    replayBuild: _state.CurrentBuild,
                    upToBuild: _state.UpToBuild,
                    leagueId: -1,
                    gameMode: GetGameMode()),
        };
        RedisSet(keyMap, valueMap, TimeSpan.FromDays(45)); //Changed from 120 01/16/2020
    }

    private void InitStage3State()
    {
        _state = new StatsServiceState(ServiceName)
        {
            Stage = 3,
            DtStart = Now.AddDays(-7),
            DtEnd = Now,
        };
    }

    private async Task Stage3()
    {
        Log("Get talent stats for last 7 days");
        Log("Get Sitewide Hero Talent Statistics for the Last 7 Days", debug: true);

        foreach (var map in _mapIDs)
        {
            await ThrowIfCancellationRequested();
            Log($"Getting Data for MapID: {map}", debug: true);

            var statsMapAllBuilds = await GetTalentStats(mapId: map, gameModes: new[] { GetGameMode() });

            foreach (var hero in _characterIDs)
            {
                await ThrowIfCancellationRequested();
                var key =
                    $"HOTSLogs:SitewideHeroTalentStatisticsV5:Current:{_locDic[hero]}:{_locDic[map]}:-1:{GetGameMode()}";
                RedisSet(key, await RedisHeroTalentStats(statsMapAllBuilds, hero, map));
            }

            var statsMapCurrentBuild = await GetTalentStats(
                replayBuild: _state.CurrentBuild,
                mapId: map,
                gameModes: new[] { GetGameMode() });

            foreach (var hero in _characterIDs)
            {
                await ThrowIfCancellationRequested();
                var key =
                    $"HOTSLogs:SitewideHeroTalentStatisticsV5:CurrentBuild:{_locDic[hero]}:{_locDic[map]}:-1:{GetGameMode()}";
                RedisSet(key, await RedisHeroTalentStats(statsMapCurrentBuild, hero, map));
            }
        }

        foreach (var league in _leagueIDs)
        {
            await ThrowIfCancellationRequested();
            Log($"Getting Data for LeagueID: {league}", debug: true);

            var statsLeagueAllBuilds = await GetTalentStats(leagueId: league, gameModes: new[] { GetGameMode() });

            foreach (var hero in _characterIDs)
            {
                await ThrowIfCancellationRequested();
                var key =
                    $"HOTSLogs:SitewideHeroTalentStatisticsV5:Current:{_locDic[hero]}:-1:{league}:{GetGameMode()}";
                RedisSet(key, await RedisHeroTalentStats(statsLeagueAllBuilds, hero, league: league));
            }

            var statsLeagueCurrentBuild = await GetTalentStats(
                replayBuild: _state.CurrentBuild,
                leagueId: league,
                gameModes: new[] { GetGameMode() });

            foreach (var hero in _characterIDs)
            {
                await ThrowIfCancellationRequested();
                var key =
                    $"HOTSLogs:SitewideHeroTalentStatisticsV5:CurrentBuild:{_locDic[hero]}:-1:{league}:{GetGameMode()}";
                RedisSet(key, await RedisHeroTalentStats(statsLeagueCurrentBuild, hero, league: league));
            }

            foreach (var map in _mapIDs)
            {
                await ThrowIfCancellationRequested();
                Log($"Getting Data for LeagueID: {league}, MapID: {map}", debug: true);

                var statsLeagueMapAllBuilds =
                    await GetTalentStats(mapId: map, leagueId: league, gameModes: new[] { GetGameMode() });

                foreach (var hero in _characterIDs)
                {
                    await ThrowIfCancellationRequested();
                    var key =
                        $"HOTSLogs:SitewideHeroTalentStatisticsV5:Current:{_locDic[hero]}:{_locDic[map]}:{league}:{GetGameMode()}";
                    RedisSet(key, await RedisHeroTalentStats(statsLeagueMapAllBuilds, hero, map, league));
                }

                var statsLeagueMapCurrentBuild = await GetTalentStats(
                    replayBuild: _state.CurrentBuild,
                    mapId: map,
                    leagueId: league,
                    gameModes: new[] { GetGameMode() });

                foreach (var hero in _characterIDs)
                {
                    await ThrowIfCancellationRequested();
                    var key =
                        $"HOTSLogs:SitewideHeroTalentStatisticsV5:CurrentBuild:{_locDic[hero]}:{_locDic[map]}:{league}:{GetGameMode()}";
                    RedisSet(key, await RedisHeroTalentStats(statsLeagueMapCurrentBuild, hero, map, league));
                }
            }
        }

        Log("Done Setting Sitewide Character and Map Statistics for Last 7 Days", debug: true);
    }

    private void InitStage4State()
    {
        _state = new StatsServiceState(ServiceName)
        {
            Stage = 4,
        };
    }

    private async Task Stage4()
    {
        var earliestDate = Now.AddDays(-90).StartOfWeek(DayOfWeek.Sunday);
        var latestDate = Now.StartOfWeek(DayOfWeek.Sunday);

        for (_state.DtStart = latestDate;
             _state.DtStart >= earliestDate;
             _state.DtStart = _state.DtStart.AddDays(-7))
        {
            _popularBuildsCache.Clear();
            await ThrowIfCancellationRequested();
            _state.DtEnd = _state.DtStart.AddDays(7);

            Log(
                "Set Sitewide Character Statistics, Map Statistics, and Character Win Rate for each Week - " +
                $"Current Week: {_state.DtStart.ToShortDateString()}", debug: true);

            var dtEndString = _state.DtEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var keyHero = $"HOTSLogs:SitewideCharacterStatisticsV2:{dtEndString}:-1:{GetGameMode()}";
            var valueHero = new SitewideCharacterStatistics
            {
                DateTimeBegin = _state.DtStart,
                DateTimeEnd = _state.DtEnd,
                League = -1,
                GameMode = GetGameMode(),
                LastUpdated = Now,
                SitewideCharacterStatisticArray = await
                    _statsHelper.GetSitewideCharacterStatistics(_state.DtStart, _state.DtEnd, -1, GetGameMode()),
            };
            RedisSet(keyHero, valueHero, TimeSpan.FromDays(45)); //Changed from 120 01/16/2020

            var keyMap = $"HOTSLogs:SitewideMapStatisticsV2:{dtEndString}:-1:{GetGameMode()}";
            var valueMap = new SitewideMapStatistics
            {
                DateTimeBegin = _state.DtStart,
                DateTimeEnd = _state.DtEnd,
                League = -1,
                GameMode = GetGameMode(),
                LastUpdated = Now,
                SitewideMapStatisticArray = await
                    _statsHelper.GetSitewideMapStatistics(_state.DtStart, _state.DtEnd, -1, GetGameMode()),
            };
            RedisSet(keyMap, valueMap, TimeSpan.FromDays(45)); //Changed from 120 01/16/2020

            // Get the most active Replay Build in this time frame
            // We can't just get the max build because there are outlier replays uploaded with strange dates
#if !LOCALDEBUG
                const string mostActiveReplayBuildQuery =
                    @"select ReplayBuild
                      from Replay use index (IX_GameMode_TimestampReplay)
                      where GameMode = @gameMode
                      and TimestampReplay > @datetimeBegin
                      and TimestampReplay < @datetimeEnd
                      group by ReplayBuild having count(*) > 100
                      order by ReplayBuild desc limit 1";
#else
            const string mostActiveReplayBuildQuery =
                @"select ReplayBuild
                      from Replay use index (IX_GameMode_TimestampReplay)
                      where GameMode = @gameMode
                      and TimestampReplay > @datetimeBegin
                      and TimestampReplay < @datetimeEnd
                      group by ReplayBuild having count(*) > 1 -- in debug we want this week regardless of number of games
                      order by ReplayBuild desc limit 1";
#endif
            await using (var mySqlConnection = new MySqlConnection(ConnectionString))
            {
                await mySqlConnection.OpenAsync(_token);

                await using var mySqlCommand = new MySqlCommand(mostActiveReplayBuildQuery, mySqlConnection)
                {
                    CommandTimeout = MMR.LongCommandTimeout,
                };
                mySqlCommand.Parameters.AddWithValue("@gameMode", GetGameMode());
                mySqlCommand.Parameters.AddWithValue("@datetimeBegin", _state.DtStart);
                mySqlCommand.Parameters.AddWithValue("@datetimeEnd", _state.DtEnd);

                var currentBuildObject = await mySqlCommand.ExecuteScalarAsync(_token);

                if (currentBuildObject != null)
                {
                    _state.CurrentBuild = (int)currentBuildObject;
                }
                else
                {
                    // Less than 100 replays currently uploaded for this game mode and date range
                    continue;
                }
            }

            var week = await GetTalentStats(replayBuild: _state.CurrentBuild, gameModes: new[] { GetGameMode() });

            foreach (var hero in _characterIDs)
            {
                await ThrowIfCancellationRequested();
                // Get Sitewide Hero Talent Statistics
                var key =
                    $"HOTSLogs:SitewideHeroTalentStatisticsV5:{dtEndString}:{_locDic[hero]}:-1:-1:{GetGameMode()}";
                RedisSet(key, await RedisHeroTalentStats(week, hero), TimeSpan.FromDays(120));
            }

            foreach (var map in _mapIDs)
            {
                await ThrowIfCancellationRequested();
                var weekMap = await GetTalentStats(
                    replayBuild: _state.CurrentBuild,
                    mapId: map,
                    gameModes: new[] { GetGameMode() });

                foreach (var hero in _characterIDs)
                {
                    await ThrowIfCancellationRequested();
                    var key =
                        $"HOTSLogs:SitewideHeroTalentStatisticsV5:{dtEndString}:{_locDic[hero]}:{_locDic[map]}:-1:{GetGameMode()}";
                    RedisSet(key, await RedisHeroTalentStats(weekMap, hero, map), TimeSpan.FromDays(120));
                }
            }

            foreach (var league in _leagueIDs)
            {
                await ThrowIfCancellationRequested();
                var keyHero2 = $"HOTSLogs:SitewideCharacterStatisticsV2:{dtEndString}:{league}:{GetGameMode()}";
                var valueHero2 = new SitewideCharacterStatistics
                {
                    DateTimeBegin = _state.DtStart,
                    DateTimeEnd = _state.DtEnd,
                    League = league,
                    GameMode = GetGameMode(),
                    LastUpdated = Now,
                    SitewideCharacterStatisticArray = await
                        _statsHelper.GetSitewideCharacterStatistics(
                            _state.DtStart,
                            _state.DtEnd,
                            league,
                            GetGameMode()),
                };
                RedisSet(keyHero2, valueHero2, TimeSpan.FromDays(120));

                var keyMap2 = $"HOTSLogs:SitewideMapStatisticsV2:{dtEndString}:{league}:{GetGameMode()}";
                var valueMap2 = new SitewideMapStatistics
                {
                    DateTimeBegin = _state.DtStart,
                    DateTimeEnd = _state.DtEnd,
                    League = league,
                    GameMode = GetGameMode(),
                    LastUpdated = Now,
                    SitewideMapStatisticArray = await
                        _statsHelper.GetSitewideMapStatistics(_state.DtStart, _state.DtEnd, league, GetGameMode()),
                };
                RedisSet(keyMap2, valueMap2, TimeSpan.FromDays(120));

                var weekLeague = await GetTalentStats(
                    replayBuild: _state.CurrentBuild,
                    leagueId: league,
                    gameModes: new[] { GetGameMode() });

                foreach (var hero in _characterIDs)
                    // Get Sitewide Hero Talent Statistics
                {
                    var key =
                        $"HOTSLogs:SitewideHeroTalentStatisticsV5:{dtEndString}:{_locDic[hero]}:-1:{league}:{GetGameMode()}";
                    RedisSet(key, await RedisHeroTalentStats(weekLeague, hero, league: league), TimeSpan.FromDays(120));
                }

                foreach (var map in _mapIDs)
                {
                    await ThrowIfCancellationRequested();
                    var weekLeagueMap = await GetTalentStats(
                        replayBuild: _state.CurrentBuild,
                        mapId: map,
                        leagueId: league,
                        gameModes: new[] { GetGameMode() });

                    foreach (var hero in _characterIDs)
                    {
                        await ThrowIfCancellationRequested();
                        var key =
                            $"HOTSLogs:SitewideHeroTalentStatisticsV5:{dtEndString}:{_locDic[hero]}:{_locDic[map]}:{league}:{GetGameMode()}";
                        RedisSet(
                            key,
                            await RedisHeroTalentStats(weekLeagueMap, hero, map, league),
                            TimeSpan.FromDays(120));
                    }
                }
            }
        }

        _popularBuildsCache.Clear();

        Log("Finished Setting Sitewide Character Statistics, Map Statistics, and Character Win Rate for each Week", debug: true);
    }

    private async Task ThrowIfCancellationRequested()
    {
        await Task.Yield();
        _token.ThrowIfCancellationRequested();
    }

    private void InitStage5State()
    {
        _state = new StatsServiceState(ServiceName)
        {
            Stage = 5,
        };
    }

    private async Task Stage5()
    {
        Log("Get win rates for each week");
        Log("Begin Set Character Win Rate for each Week", debug: true);

        var heroStatList = new List<SitewideCharacterStatistics>();

        for (var dt = Now.AddDays(-90).StartOfWeek(DayOfWeek.Sunday);
             dt < Now;
             dt = dt.AddDays(7))
        {
            var dtStr = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            foreach (var league in _leagueIDs.Union(new[] { -1 }))
            {
                await ThrowIfCancellationRequested();
                var heroStat =
                    _redis.Get<SitewideCharacterStatistics>(
                        $"HOTSLogs:SitewideCharacterStatisticsV2:{dtStr}:{league}:{GetGameMode()}");
                if (heroStat != null)
                {
                    heroStatList.Add(heroStat);
                }
            }
        }

        foreach (var league in _leagueIDs.Union(new[] { -1 }))
        {
            foreach (var hero in _characterIDs)
            {
                await ThrowIfCancellationRequested();
                var key = $"HOTSLogs:SitewideCharacterWinRateByDateTime:{league}:{GetGameMode()}:{_locDic[hero]}";
                var value = new SitewideCharacterWinRateByDateTime
                {
                    Character = _locDic[hero],
                    League = league,
                    GameMode = GetGameMode(),
                    LastUpdated = Now,
                    DateTimeWinRates = heroStatList.Where(
                            i =>
                                i.League == league &&
                                i.SitewideCharacterStatisticArray.Any(j => j.Character == _locDic[hero]))
                        .Select(
                            i => new
                            {
                                i.DateTimeBegin,
                                i.DateTimeEnd,
                                SitewideCharacterStatistic =
                                    i.SitewideCharacterStatisticArray.Single(j => j.Character == _locDic[hero]),
                            })
                        .Select(
                            i => new SitewideDateTimeWinRate
                            {
                                DateTimeBegin = i.DateTimeBegin,
                                DateTimeEnd = i.DateTimeEnd,
                                GamesPlayed = i.SitewideCharacterStatistic.GamesPlayed,
                                WinPercent = i.SitewideCharacterStatistic.WinPercent,
                            })
                        .OrderBy(i => i.DateTimeBegin)
                        .ToArray(),
                };
                RedisSet(key, value, TimeSpan.FromDays(120));
            }
        }

        Log("Finished Setting Character Win Rate for each Week", debug: true);
    }

    private void InitStage6State()
    {
        _state = new StatsServiceState(ServiceName)
        {
            Stage = 6,
            DtStart = Now.AddDays(-7),
        };
    }

    private async Task Stage6()
    {
        Log("Get win rates versus");
        Log("Begin Set Sitewide Character Win Percent Vs Other Characters", debug: true);

        foreach (var hero in _characterIDs)
        {
            await ThrowIfCancellationRequested();
            Log($"Begin Set {_locDic[hero]}", debug: true);

            var key = $"HOTSLogs:SitewideCharacterWinPercentVsOtherCharacters:-1:{GetGameMode()}:{_locDic[hero]}";
            var value =
                await _statsHelper.GetSitewideCharacterWinPercentAndOtherCharactersAsync(
                    _state.DtStart,
                    -1,
                    GetGameMode(),
                    hero,
                    "!=");
            RedisSet(key, value);

            foreach (var league in _leagueIDs)
            {
                await ThrowIfCancellationRequested();
                var key2 =
                    $"HOTSLogs:SitewideCharacterWinPercentVsOtherCharacters:{league}:{GetGameMode()}:{_locDic[hero]}";
                var value2 =
                    await _statsHelper.GetSitewideCharacterWinPercentAndOtherCharactersAsync(
                        _state.DtStart,
                        league,
                        GetGameMode(),
                        hero,
                        "!=");
                RedisSet(key2, value2);
            }
        }

        Log("Finished Setting Sitewide Character Win Percent Vs Other Characters", debug: true);
    }

    private void InitStage7State()
    {
        _state = new StatsServiceState(ServiceName)
        {
            Stage = 7,
            DtStart = Now.AddDays(-7),
        };
    }

    private async Task Stage7()
    {
        Log("Get win rates with");
        Log("Begin Set Sitewide Character Win Percent With Other Characters", debug: true);

        foreach (var hero in _characterIDs)
        {
            await ThrowIfCancellationRequested();
            Log($"Begin Set {_locDic[hero]}", debug: true);

            var key =
                $"HOTSLogs:SitewideCharacterWinPercentWithOtherCharacters:-1:{GetGameMode()}:{_locDic[hero]}";
            var value =
                await _statsHelper.GetSitewideCharacterWinPercentAndOtherCharactersAsync(
                    _state.DtStart,
                    -1,
                    GetGameMode(),
                    hero,
                    "=");
            RedisSet(key, value);

            foreach (var league in _leagueIDs)
            {
                await ThrowIfCancellationRequested();
                var key2 =
                    $"HOTSLogs:SitewideCharacterWinPercentWithOtherCharacters:{league}:{GetGameMode()}:{_locDic[hero]}";
                var value2 =
                    await _statsHelper.GetSitewideCharacterWinPercentAndOtherCharactersAsync(
                        _state.DtStart,
                        league,
                        GetGameMode(),
                        hero,
                        "=");
                RedisSet(key2, value2);
            }
        }

        Log("Finished Setting Sitewide Character Win Percent With Other Characters", debug: true);
    }

    private void InitStage8State()
    {
        _state = new StatsServiceState(ServiceName)
        {
            Stage = 8,
            DtStart = Now.AddDays(-30),
            DtEnd = Now,
        };
    }

    private async Task Stage8()
    {
        Log("Get win rates by game time");
        Log("Begin Set Sitewide Character Win Rate by Game Time", debug: true);

        var keyAggregate2 = $"HOTSLogs:SitewideCharacterGameTimeWinRates:-1:{GetGameMode()}";
        var heroStats2 =
            _statsHelper.GetSitewideCharacterStatisticsByGameTime(_state.DtStart, _state.DtEnd, -1, GetGameMode());
        RedisSet(keyAggregate2, heroStats2);
        foreach (var t in heroStats2)
        {
            var key = $"HOTSLogs:SitewideCharacterGameTimeWinRates:-1:{GetGameMode()}:{t.Character}";
            RedisSet(key, t);
        }

        foreach (var league in _leagueIDs)
        {
            await ThrowIfCancellationRequested();
            var heroStats =
                _statsHelper.GetSitewideCharacterStatisticsByGameTime(
                    _state.DtStart,
                    _state.DtEnd,
                    league,
                    GetGameMode());
            var keyAggregate = $"HOTSLogs:SitewideCharacterGameTimeWinRates:{league}:{GetGameMode()}";
            RedisSet(keyAggregate, heroStats);
            foreach (var t in heroStats)
            {
                await ThrowIfCancellationRequested();
                var key = $"HOTSLogs:SitewideCharacterGameTimeWinRates:{league}:{GetGameMode()}:{t.Character}";
                RedisSet(key, t);
            }
        }

        Log("Finished Setting Sitewide Character Win Rate by Game Time", debug: true);
    }

    private void InitStage9State()
    {
        _state = new StatsServiceState(ServiceName)
        {
            Stage = 9,
            DtStart = Now.AddDays(-30),
            DtEnd = Now,
        };
    }

    private async Task Stage9()
    {
        Log("Get win rates by level");
        Log("Begin Set Sitewide Character Win Rate by Character Level", debug: true);

        var heroStats2 =
            _statsHelper.GetSitewideCharacterStatisticsByCharacterLevel(
                _state.DtStart,
                _state.DtEnd,
                -1,
                GetGameMode());

        foreach (var heroStat in heroStats2.GroupBy(i => i.Character))
        {
            await ThrowIfCancellationRequested();
            var key = $"HOTSLogs:SitewideCharacterLevelWinRates:-1:{GetGameMode()}:{heroStat.Key}";
            var value = heroStat.OrderBy(i => i.CharacterLevel).ToArray();
            RedisSet(key, value);
        }

        foreach (var heroStat in heroStats2.GroupBy(i => i.CharacterLevel))
        {
            await ThrowIfCancellationRequested();
            var key = $"HOTSLogs:SitewideCharacterLevelWinRates:-1:{GetGameMode()}:{heroStat.Key}";
            var value = heroStat.OrderBy(i => i.Character).ToArray();
            RedisSet(key, value);
        }

        foreach (var league in _leagueIDs)
        {
            await ThrowIfCancellationRequested();
            heroStats2 =
                _statsHelper.GetSitewideCharacterStatisticsByCharacterLevel(
                    _state.DtStart,
                    _state.DtEnd,
                    league,
                    GetGameMode());

            foreach (var heroStat in heroStats2.GroupBy(i => i.Character))
            {
                await ThrowIfCancellationRequested();
                var key = $"HOTSLogs:SitewideCharacterLevelWinRates:{league}:{GetGameMode()}:{heroStat.Key}";
                var value = heroStat.OrderBy(i => i.CharacterLevel).ToArray();
                RedisSet(key, value);
            }

            foreach (var heroStat in heroStats2.GroupBy(i => i.CharacterLevel))
            {
                await ThrowIfCancellationRequested();
                var key = $"HOTSLogs:SitewideCharacterLevelWinRates:{league}:{GetGameMode()}:{heroStat.Key}";
                var value = heroStat.OrderBy(i => i.Character).ToArray();
                RedisSet(key, value);
            }
        }

        Log("Finished Setting Sitewide Character Win Rate by Character Level", debug: true);
    }

    private void InitStage10State()
    {
        _state = new StatsServiceState(ServiceName)
        {
            Stage = 10,
            DtStart = Now.AddDays(-30),
            DtEnd = Now,
        };
    }

    private async Task Stage10()
    {
        Log("Get win rates by talent upgrade");
        Log("Begin Set Sitewide Character Win Rate by Talent Upgrade Event Average Value", debug: true);

        foreach (var league in _leagueIDs.Concat(new[] { -1 }))
        {
            await ThrowIfCancellationRequested();
            var statsByUpgrade =
                _statsHelper.GetSitewideCharacterWinRateByTalentUpgradeEventAverageValue(
                    _state.DtStart,
                    _state.DtEnd,
                    league,
                    GetGameMode());
            foreach (var statEntry in statsByUpgrade)
            {
                await ThrowIfCancellationRequested();
                var key = $"HOTSLogs:SitewideTalentUpgradeWinRates:{league}:{GetGameMode()}:{statEntry.Character}";
                RedisSet(key, statEntry);
            }
        }

        Log("Finished Setting Sitewide Character Win Rate by Talent Upgrade Event Average Value", debug: true);
    }

    private void InitStage11State()
    {
        _state = new StatsServiceState(ServiceName)
        {
            Stage = 11,
            DtStart = _gameEvent ? DateTime.MinValue : Now.AddDays(-30),
            DtEnd = _gameEvent ? DateTime.MaxValue : Now,
        };
    }

    private async Task Stage11()
    {
        Log("Get map objective statistics");
        Log("Begin Set Map Objective Statistics", debug: true);

        await _statsHelper.SetMapObjectiveStatistics(GetGameMode(), _state.DtStart, _state.DtEnd);

        Log("Finished Setting Map Objective Statistics", debug: true);
    }

    private void RedisSet<T>(string key, T value, TimeSpan? expiry = null)
    {
        var exp = expiry ?? TimeSpan.FromDays(30);
        _redis.TrySet(key, value, exp);
    }

    private async Task<SitewideHeroTalentStatistics> RedisHeroTalentStats(
        SitewideHeroTalentStatistic[] stats,
        int hero,
        int? map = null,
        int league = -1)
    {
        var rc = new SitewideHeroTalentStatistics
        {
            DateTimeBegin = _state.DtStart,
            DateTimeEnd = _state.DtEnd,
            ReplayBuild = _state.UpToBuild ?? _state.CurrentBuild,
            Character = _locDic[hero],
            League = league,
            GameMode = GetGameMode(),
            LastUpdated = Now,
            SitewideHeroTalentStatisticArray = FilteredStats(stats, hero),
            HeroTalentBuildStatisticArray = await GetPopularBuilds(hero, map, league),
        };

        if (map.HasValue)
        {
            rc.MapName = _locDic[map.Value];
        }

        return rc;
    }

    private SitewideHeroTalentStatistic[] FilteredStats(SitewideHeroTalentStatistic[] stats, int hero)
    {
        return stats.Where(i => i.Character == _locDic[hero])
            .OrderBy(i => i.TalentID)
            .ToArray();
    }

    private async Task<HeroTalentBuildStatistic[]> GetPopularBuilds(int hero, int? map = null, int? league = null)
    {
        var key = new CacheKey
        {
            UpToBuild = _state.UpToBuild,
            CurrentBuild = _state.CurrentBuild,
            DtEnd = _state.DtEnd,
            DtStart = _state.DtStart,
        };

        if (!_popularBuildsCache.ContainsKey(key))
        {
            var zrc = await _statsHelper.GetSitewideHeroTalentPopularBuilds2(
                _state.DtStart,
                _state.DtEnd,
                _state.CurrentBuild,
                GetGameMode(),
                upToBuild: _state.UpToBuild);
            _popularBuildsCache[key] = zrc;
        }

        var source = _popularBuildsCache[key];

        var rc = _statsHelper.GetSitewideHeroTalentPopularBuilds3(
            source,
            _locDic[hero],
            _state.CurrentBuild,
            hero,
            map,
            league ?? -1,
            upToBuild: _state.UpToBuild);

        return rc;
    }

    private async Task<SitewideHeroTalentStatistic[]> GetTalentStats(
        int? replayBuild = null,
        int? characterId = null,
        int? mapId = null,
        int? leagueId = null,
        int[] gameModes = null,
        int[] playerIDs = null,
        bool innerJoinTalentInformation = false,
        int[] replayIDs = null,
        int? upToBuildArg = null)
    {
        return await DataHelper.GetSitewideHeroTalentStatisticsAsync(
            datetimeBegin: _state.DtStart,
            datetimeEnd: _state.DtEnd,
            replayBuild: replayBuild,
            characterId: characterId,
            mapId: mapId,
            leagueId: leagueId,
            gameModes: gameModes,
            playerIDs: playerIDs,
            innerJoinTalentInformation: innerJoinTalentInformation,
            replayIDs: replayIDs,
            upToBuild: upToBuildArg,
            token: _token);
    }

    private void Log(string msg, bool debug = false)
    {
        DataHelper.LogApplicationEvents(msg, ServiceName, debug);
        AddServiceOutput(msg);
    }

    private class StatsServiceState
    {
        private readonly string _serviceName;
        private int _currentBuild;
        private DateTime _dtEnd;

        private DateTime _dtStart;
        private int _stage;
        private int? _upToBuild;

        public StatsServiceState(string serviceName)
        {
            _serviceName = serviceName;
        }

        public DateTime DtStart
        {
            get => _dtStart;
            set
            {
                if (value.Equals(_dtStart))
                {
                    return;
                }

                _dtStart = value;
                Save();
            }
        }

        public DateTime DtEnd
        {
            get => _dtEnd;
            set
            {
                if (value.Equals(_dtEnd))
                {
                    return;
                }

                _dtEnd = value;
                Save();
            }
        }

        public int CurrentBuild
        {
            get => _currentBuild;
            set
            {
                if (value == _currentBuild)
                {
                    return;
                }

                _currentBuild = value;
                Save();
            }
        }

        public int? UpToBuild
        {
            get => _upToBuild;
            set
            {
                if (value == _upToBuild)
                {
                    return;
                }

                _upToBuild = value;
                Save();
            }
        }

        public int Stage
        {
            get => _stage;
            set
            {
                if (value == _stage)
                {
                    return;
                }

                _stage = value;
                Save();
            }
        }

        // ReSharper disable once UnusedMember.Local
        public static StatsServiceState Load(string serviceName)
        {
            try
            {
                var json = File.ReadAllText("state.json");
                var state = JsonConvert.DeserializeObject<StatsServiceState>(json);
                return state ?? new StatsServiceState(serviceName);
            }
            catch
            {
                var newState = new StatsServiceState(serviceName);
                return newState;
            }
        }

        private void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this);
                File.WriteAllText($"state {_serviceName}.json", json);
            }
            catch
            {
                // ignored - probably file in use, we'll save it next time, no worries.
            }
        }
    }
}

internal struct CacheKey
{
    public DateTime DtStart;
    public DateTime DtEnd;
    public int CurrentBuild;
    public int? UpToBuild;
}
