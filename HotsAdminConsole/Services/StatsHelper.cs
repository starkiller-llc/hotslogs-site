using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Heroes.ReplayParser;
using MySqlConnector;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace HotsAdminConsole.Services;

public sealed class StatsHelper
{
    private readonly SqlQueryLogger _queryLogger;
    public string ConnectionString { get; }

    private List<HeroTalentInformation> _talentsCache;
    private readonly HeroesdataContext _dc;
    private readonly MyDbWrapper _redisClient;
    private readonly CancellationToken _token;
    private Dictionary<int, string> _locDic;

    public event EventHandler TmpTableFullRetry;

#if !LOCALDEBUG
    public DateTime Now => DateTime.UtcNow;
#else
    public DateTime Now { get; } = new(2022, 5, 3, 12, 0, 0, DateTimeKind.Utc);
#endif

    public StatsHelper(
        HeroesdataContext dc,
        MyDbWrapper redisClient,
        string connectionString,
        Action<string, bool> log,
        bool logQueries = false,
        CancellationToken token = default)
    {
        _dc = dc;
        _redisClient = redisClient;
        _token = token;
        _queryLogger = new SqlQueryLogger(log)
        {
            IsActive = logQueries,
        };
        ConnectionString = connectionString;
        RefreshCaches();
    }

    public async Task SetSitewideCharacterAndMapAndTalentStatistics(
        int gameMode,
        MyDbWrapper redisClient,
        DateTime currentDateTimeBegin,
        DateTime currentDateTimeEnd,
        int[] leagueIDs,
        int[] characterIDs,
        int currentBuild)
    {
        foreach (var leagueId in leagueIDs)
        {
            redisClient.TrySet(
                "HOTSLogs:SitewideCharacterStatisticsV2:Current:" + leagueId + ":" + gameMode,
                new SitewideCharacterStatistics
                {
                    DateTimeBegin = currentDateTimeBegin,
                    DateTimeEnd = currentDateTimeEnd,
                    League = leagueId,
                    GameMode = gameMode,
                    LastUpdated = Now,
                    SitewideCharacterStatisticArray = await GetSitewideCharacterStatistics(
                        currentDateTimeBegin,
                        currentDateTimeEnd,
                        leagueId,
                        gameMode),
                },
                TimeSpan.FromDays(30));

            redisClient.TrySet(
                "HOTSLogs:SitewideMapStatisticsV2:Current:" + leagueId + ":" + gameMode,
                new SitewideMapStatistics
                {
                    DateTimeBegin = currentDateTimeBegin,
                    DateTimeEnd = currentDateTimeEnd,
                    League = leagueId,
                    GameMode = gameMode,
                    LastUpdated = Now,
                    SitewideMapStatisticArray = await GetSitewideMapStatistics(
                        currentDateTimeBegin,
                        currentDateTimeEnd,
                        leagueId,
                        gameMode),
                },
                TimeSpan.FromDays(30));

            redisClient.TrySet(
                "HOTSLogs:SitewideCharacterStatisticsV2:CurrentBuild:" + leagueId + ":" + gameMode,
                new SitewideCharacterStatistics
                {
                    DateTimeBegin = currentDateTimeBegin,
                    DateTimeEnd = currentDateTimeEnd,
                    League = leagueId,
                    GameMode = gameMode,
                    LastUpdated = Now,
                    SitewideCharacterStatisticArray = await GetSitewideCharacterStatistics(
                        currentDateTimeBegin,
                        currentDateTimeEnd,
                        leagueId,
                        gameMode,
                        currentBuild),
                },
                TimeSpan.FromDays(30));

            redisClient.TrySet(
                "HOTSLogs:SitewideMapStatisticsV2:CurrentBuild:" + leagueId + ":" + gameMode,
                new SitewideMapStatistics
                {
                    DateTimeBegin = currentDateTimeBegin,
                    DateTimeEnd = currentDateTimeEnd,
                    League = leagueId,
                    GameMode = gameMode,
                    LastUpdated = Now,
                    SitewideMapStatisticArray = await GetSitewideMapStatistics(
                        currentDateTimeBegin,
                        currentDateTimeEnd,
                        leagueId,
                        gameMode,
                        currentBuild),
                },
                TimeSpan.FromDays(30));
        }

        var sitewideHeroTalentStatisticArray = await DataHelper.GetSitewideHeroTalentStatisticsAsync(
            datetimeBegin: currentDateTimeBegin,
            datetimeEnd: currentDateTimeEnd,
            replayBuild: null,
            characterId: null,
            mapId: null,
            leagueId: null,
            gameModes: new[] { gameMode },
            token: _token);

        foreach (var currentCharacterId in characterIDs)
        {
            redisClient.TrySet(
                "HOTSLogs:SitewideHeroTalentStatisticsV5:Current:" +
                _locDic[currentCharacterId] + ":-1:-1:" + gameMode,
                new SitewideHeroTalentStatistics
                {
                    DateTimeBegin = currentDateTimeBegin,
                    DateTimeEnd = currentDateTimeEnd,
                    ReplayBuild = currentBuild,
                    Character = _locDic[currentCharacterId],
                    League = -1,
                    GameMode = gameMode,
                    LastUpdated = Now,
                    SitewideHeroTalentStatisticArray = sitewideHeroTalentStatisticArray
                        .Where(i => i.Character == _locDic[currentCharacterId])
                        .OrderBy(i => i.TalentID).ToArray(),
                    HeroTalentBuildStatisticArray = await GetSitewideHeroTalentPopularBuilds(
                        currentDateTimeBegin,
                        currentDateTimeEnd,
                        currentBuild,
                        currentCharacterId,
                        null,
                        -1,
                        gameMode),
                },
                TimeSpan.FromDays(30));
        }

        sitewideHeroTalentStatisticArray = await DataHelper.GetSitewideHeroTalentStatisticsAsync(
            datetimeBegin: currentDateTimeBegin,
            datetimeEnd: currentDateTimeEnd,
            replayBuild: currentBuild,
            characterId: null,
            mapId: null,
            leagueId: null,
            gameModes: new[] { gameMode },
            token: _token);

        foreach (var currentCharacterId in characterIDs)
        {
            redisClient.TrySet(
                "HOTSLogs:SitewideHeroTalentStatisticsV5:CurrentBuild:" +
                _locDic[currentCharacterId] + ":-1:-1:" + gameMode,
                new SitewideHeroTalentStatistics
                {
                    DateTimeBegin = currentDateTimeBegin,
                    DateTimeEnd = currentDateTimeEnd,
                    ReplayBuild = currentBuild,
                    Character = _locDic[currentCharacterId],
                    League = -1,
                    GameMode = gameMode,
                    LastUpdated = Now,
                    SitewideHeroTalentStatisticArray = sitewideHeroTalentStatisticArray
                        .Where(i => i.Character == _locDic[currentCharacterId])
                        .OrderBy(i => i.TalentID).ToArray(),
                    HeroTalentBuildStatisticArray = await GetSitewideHeroTalentPopularBuilds(
                        currentDateTimeBegin,
                        currentDateTimeEnd,
                        currentBuild,
                        currentCharacterId,
                        null,
                        -1,
                        gameMode),
                },
                TimeSpan.FromDays(30));
        }
    }

    public async Task<SitewideCharacterStatistic[]> GetSitewideCharacterStatistics(
        DateTime datetimeBegin,
        DateTime datetimeEnd,
        int leagueId = -1,
        int gameMode = 8,
        int replayBuild = -1,
        int? upToBuild = null)
    {
        const string mySqlCommandText =
            @"select rc.CharacterID,
                count(*) as GamesPlayed,
                sum(rc.IsWinner) as GamesWon,
                SEC_TO_TIME(cast(AVG(TIME_TO_SEC(r.ReplayLength)) as unsigned)) as AverageLength,
                avg(rcsr.Takedowns) as AvgTakedowns,
                avg(rcsr.SoloKills) as AvgSoloKills,
                avg(rcsr.Assists) as AvgAssists,
                avg(rcsr.Deaths) as AvgDeaths,
                avg(rcsr.HeroDamage) as AvgHeroDamage,
                avg(rcsr.SiegeDamage) as AvgSiegeDamage,
                avg(rcsr.StructureDamage) as AvgStructureDamage,
                avg(rcsr.MinionDamage) as AvgMinionDamage,
                avg(rcsr.CreepDamage) as AvgCreepDamage,
                avg(rcsr.SummonDamage) as AvgSummonDamage,
                avg(rcsr.TimeCCdEnemyHeroes) as AvgTimeCCdEnemyHeroes,
                avg(rcsr.Healing) as AvgHealing,
                avg(rcsr.SelfHealing) as AvgSelfHealing,
                avg(rcsr.DamageTaken) as AvgDamageTaken,
                avg(rcsr.ExperienceContribution) as AvgExperienceContribution,
                avg(rcsr.TownKills) as AvgTownKills,
                avg(rcsr.TimeSpentDead) as AvgTimeSpentDead,
                avg(rcsr.MercCampCaptures) as AvgMercCampCaptures,
                avg(rcsr.WatchTowerCaptures) as AvgWatchTowerCaptures,
                avg(rcsr.MetaExperience) as AvgMetaExperience
                from ReplayCharacter rc
                left join ReplayCharacterScoreResult rcsr on rcsr.ReplayID = rc.ReplayID and rcsr.PlayerID = rc.PlayerID
                {1}
                join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
                {4} 
                where r.GameMode = @gameMode and r.TimestampReplay >= @datetimeBegin and r.TimestampReplay < @datetimeEnd and (rc.CharacterLevel >= 5 or r.GameMode = 7 or r.GameMode >= 1000)
                {0} {2} {3}
                group by rc.CharacterID";

        var locDic = _dc.LocalizationAliases.ToDictionary(i => i.IdentifierId, i => i.PrimaryName);

        var sitewideCharacterStatisticsDictionary = new Dictionary<int, SitewideCharacterStatistic>();
        await RetryIfTmpTableFull(ReadFromSql);

        async Task ReadFromSql()
        {
            await using var mySqlConnection = new MySqlConnection(ConnectionString);
            string buildTerm = null;
            if (replayBuild != -1)
            {
                buildTerm = upToBuild.HasValue
                    ? "and r.ReplayBuild >= @replayBuild and r.ReplayBuild <= @upToBuild"
                    : "and r.ReplayBuild = @replayBuild";
            }

            var leaderboardRankingJoin = GetLeaderboardRankingJoin(gameMode);

            await mySqlConnection.OpenAsync(_token);
            var cmdText = string.Format(
                mySqlCommandText,
                leagueId != -1 ? "and lr.LeagueID = @leagueID" : null,
                //gameMode == 3 ? "left join ReplayCharacter rcMirror on rcMirror.ReplayID = rc.ReplayID and rcMirror.PlayerID != rc.PlayerID and rcMirror.CharacterID = rc.CharacterID" : null,
                gameMode == 3 ? "left join replay_mirror rcMirror on rcMirror.ReplayID = rc.ReplayID" : null,
                gameMode == 3 ? "and rcMirror.ReplayID is null" : null,
                buildTerm,
                leaderboardRankingJoin);
            await using var mySqlCommand = new MySqlCommand(cmdText, mySqlConnection)
            {
                CommandTimeout = MMR.LongCommandTimeout,
            };
            mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
            mySqlCommand.Parameters.AddWithValue("@datetimeBegin", datetimeBegin);
            mySqlCommand.Parameters.AddWithValue("@datetimeEnd", datetimeEnd);
            if (leagueId != -1)
            {
                mySqlCommand.Parameters.AddWithValue("@leagueID", leagueId);
            }

            if (replayBuild != -1)
            {
                mySqlCommand.Parameters.AddWithValue("@replayBuild", replayBuild);
                if (upToBuild.HasValue)
                {
                    mySqlCommand.Parameters.AddWithValue("@upToBuild", upToBuild);
                }
            }

            LogSqlCommand(mySqlCommand);
            await using var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(_token);
            while (await mySqlDataReader.ReadAsync(_token))
            {
                var obj = new
                {
                    AverageLength = mySqlDataReader["AverageLength"] is DBNull
                        ? (TimeSpan?)null
                        : mySqlDataReader.GetTimeSpan("AverageLength"),
                    GamesPlayed = mySqlDataReader.GetInt64("GamesPlayed"),
                    CharacterID = mySqlDataReader.GetInt32("CharacterID"),
                    GamesWon = mySqlDataReader.GetDecimal("GamesWon"),
                    AvgTakedowns = mySqlDataReader["AvgTakedowns"] is DBNull
                        ? 0
                        : mySqlDataReader.GetDecimal("AvgTakedowns"),
                    AvgSoloKills = mySqlDataReader["AvgSoloKills"] is DBNull
                        ? 0
                        : mySqlDataReader.GetDecimal("AvgSoloKills"),
                    AvgAssists = mySqlDataReader["AvgAssists"] is DBNull ? 0 : mySqlDataReader.GetDecimal("AvgAssists"),
                    AvgDeaths = mySqlDataReader["AvgDeaths"] is DBNull ? 0 : mySqlDataReader.GetDecimal("AvgDeaths"),
                    AvgHeroDamage = mySqlDataReader["AvgHeroDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgHeroDamage"),
                    AvgSiegeDamage = mySqlDataReader["AvgSiegeDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgSiegeDamage"),
                    AvgStructureDamage = mySqlDataReader["AvgStructureDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgStructureDamage"),
                    AvgMinionDamage = mySqlDataReader["AvgMinionDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgMinionDamage"),
                    AvgCreepDamage = mySqlDataReader["AvgCreepDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgCreepDamage"),
                    AvgSummonDamage = mySqlDataReader["AvgSummonDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgSummonDamage"),
                    AvgTimeCCdEnemyHeroes = mySqlDataReader["AvgTimeCCdEnemyHeroes"] is DBNull
                        ? (TimeSpan?)null
                        : TimeSpan.FromSeconds((double)mySqlDataReader.GetDecimal("AvgTimeCCdEnemyHeroes")),
                    AvgHealing = mySqlDataReader["AvgHealing"] is DBNull
                        ? (int?)null
                        : (int)mySqlDataReader.GetDecimal("AvgHealing"),
                    AvgSelfHealing = mySqlDataReader["AvgSelfHealing"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgSelfHealing"),
                    AvgDamageTaken = mySqlDataReader["AvgDamageTaken"] is DBNull
                        ? (int?)null
                        : (int)mySqlDataReader.GetDecimal("AvgDamageTaken"),
                    AvgExperienceContribution = mySqlDataReader["AvgExperienceContribution"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgExperienceContribution"),
                    AvgTownKills = mySqlDataReader["AvgTownKills"] is DBNull
                        ? 0
                        : mySqlDataReader.GetDecimal("AvgTownKills"),
                    AvgTimeSpentDead = mySqlDataReader["AvgTimeSpentDead"] is DBNull
                        ? TimeSpan.Zero
                        : TimeSpan.FromSeconds((double)mySqlDataReader.GetDecimal("AvgTimeSpentDead")),
                    AvgMercCampCaptures = mySqlDataReader["AvgMercCampCaptures"] is DBNull
                        ? 0
                        : mySqlDataReader.GetDecimal("AvgMercCampCaptures"),
                    AvgWatchTowerCaptures = mySqlDataReader["AvgWatchTowerCaptures"] is DBNull
                        ? 0
                        : mySqlDataReader.GetDecimal("AvgWatchTowerCaptures"),
                    AvgMetaExperience = mySqlDataReader["AvgMetaExperience"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgMetaExperience"),
                };

                if (obj.AverageLength.HasValue)
                {
                    sitewideCharacterStatisticsDictionary[obj.CharacterID] = new SitewideCharacterStatistic
                    {
                        HeroPortraitURL = locDic[obj.CharacterID].PrepareForImageURL(),
                        Character = locDic[obj.CharacterID],
                        GamesPlayed = (int)obj.GamesPlayed,
                        AverageLength = obj.AverageLength.Value,
                        WinPercent = obj.GamesWon / obj.GamesPlayed,
                        AverageScoreResult = new AverageTeamProfileReplayCharacterScoreResult
                        {
                            T = obj.AvgTakedowns,
                            S = obj.AvgSoloKills,
                            A = obj.AvgAssists,
                            D = obj.AvgDeaths,
                            HD = obj.AvgHeroDamage,
                            SiD = obj.AvgSiegeDamage,
                            StD = obj.AvgStructureDamage,
                            MD = obj.AvgMinionDamage,
                            CD = obj.AvgCreepDamage,
                            SuD = obj.AvgSummonDamage,
                            TCCdEH = obj.AvgTimeCCdEnemyHeroes,
                            H = obj.AvgHealing,
                            SH = obj.AvgSelfHealing,
                            DT = obj.AvgDamageTaken,
                            EC = obj.AvgExperienceContribution,
                            TK = obj.AvgTownKills,
                            TSD = obj.AvgTimeSpentDead,
                            MCC = obj.AvgMercCampCaptures,
                            WTC = obj.AvgWatchTowerCaptures,
                            ME = obj.AvgMetaExperience,
                        },
                    };
                }
            }
        }

        // Fill in Ban data
        var sitewideCharacterBanStatisticsDictionary = await GetSitewideCharacterBanStatistics(
            datetimeBegin,
            datetimeEnd,
            leagueId,
            gameMode,
            replayBuild,
            upToBuild: upToBuild);
        if (sitewideCharacterBanStatisticsDictionary.Count > 0 && sitewideCharacterStatisticsDictionary.Count > 0)
        {
            foreach (var sitewideCharacterBanStatistic in sitewideCharacterBanStatisticsDictionary)
            {
                if (sitewideCharacterStatisticsDictionary.ContainsKey(sitewideCharacterBanStatistic.Key))
                {
                    sitewideCharacterStatisticsDictionary[sitewideCharacterBanStatistic.Key].GamesBanned +=
                        sitewideCharacterBanStatistic.Value;
                }
                else
                {
                    sitewideCharacterStatisticsDictionary[sitewideCharacterBanStatistic.Key] =
                        new SitewideCharacterStatistic
                        {
                            HeroPortraitURL = locDic[sitewideCharacterBanStatistic.Key].PrepareForImageURL(),
                            Character = locDic[sitewideCharacterBanStatistic.Key],
                            GamesPlayed = 0,
                            GamesBanned = sitewideCharacterBanStatistic.Value,
                            AverageLength = TimeSpan.Zero,
                            WinPercent = 0,
                        };
                }
            }
        }

        return sitewideCharacterStatisticsDictionary.Values.ToArray();
    }

    private bool IsEventGameMode(int gameMode)
    {
        return gameMode > 100;
    }

    private void LogSqlCommand(
        MySqlCommand mySqlCommand,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        // ReSharper disable ExplicitCallerInfoArgument
        _queryLogger.LogSqlCommand(
            mySqlCommand,
            new[] { "@characterID" },
            callerFilePath,
            callerLineNumber,
            callerMemberName);
        // ReSharper restore ExplicitCallerInfoArgument
    }

    public async Task<SitewideMapStatistic[]> GetSitewideMapStatistics(
        DateTime datetimeBegin,
        DateTime datetimeEnd,
        int leagueId = -1,
        int gameMode = 8,
        int replayBuild = -1,
        int? upToBuild = null)
    {
        const string mySqlCommandText =
            @"select
                r.MapID,
                rc.CharacterID,
                count(*) as GamesPlayed,
                sum(rc.IsWinner) as GamesWon,
                SEC_TO_TIME(cast(AVG(TIME_TO_SEC(r.ReplayLength)) as unsigned)) as AverageLength,
                avg(rcsr.Takedowns) as AvgTakedowns,
                avg(rcsr.SoloKills) as AvgSoloKills,
                avg(rcsr.Assists) as AvgAssists,
                avg(rcsr.Deaths) as AvgDeaths,
                avg(rcsr.HeroDamage) as AvgHeroDamage,
                avg(rcsr.SiegeDamage) as AvgSiegeDamage,
                avg(rcsr.StructureDamage) as AvgStructureDamage,
                avg(rcsr.MinionDamage) as AvgMinionDamage,
                avg(rcsr.CreepDamage) as AvgCreepDamage,
                avg(rcsr.SummonDamage) as AvgSummonDamage,
                avg(rcsr.TimeCCdEnemyHeroes) as AvgTimeCCdEnemyHeroes,
                avg(rcsr.Healing) as AvgHealing,
                avg(rcsr.SelfHealing) as AvgSelfHealing,
                avg(rcsr.DamageTaken) as AvgDamageTaken,
                avg(rcsr.ExperienceContribution) as AvgExperienceContribution,
                avg(rcsr.TownKills) as AvgTownKills,
                avg(rcsr.TimeSpentDead) as AvgTimeSpentDead,
                avg(rcsr.MercCampCaptures) as AvgMercCampCaptures,
                avg(rcsr.WatchTowerCaptures) as AvgWatchTowerCaptures,
                avg(rcsr.MetaExperience) as AvgMetaExperience
                from ReplayCharacter rc
                left join ReplayCharacterScoreResult rcsr on rcsr.ReplayID = rc.ReplayID and rcsr.PlayerID = rc.PlayerID
                {1}
                join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
                {4}
                where r.GameMode = @gameMode and r.TimestampReplay >= @datetimeBegin and r.TimestampReplay < @datetimeEnd and (rc.CharacterLevel >= 5 or r.GameMode = 7 or r.GameMode >= 1000)
                {0} {2} {3}
                group by r.MapID, rc.CharacterID";

        var locDic = _dc.LocalizationAliases.ToDictionary(i => i.IdentifierId, i => i.PrimaryName);

        var sitewideCharacterStatisticsDictionaryByMap = new Dictionary<int, Dictionary<int, SitewideMapStatistic>>();

        await RetryIfTmpTableFull(ReadFromSql);

        // Fill in Ban data
        async Task ReadFromSql()
        {
            await using var mySqlConnection = new MySqlConnection(ConnectionString);
            string buildTerm = null;
            if (replayBuild != -1)
            {
                buildTerm = upToBuild.HasValue
                    ? "and r.ReplayBuild >= @replayBuild and r.ReplayBuild <= @upToBuild"
                    : "and r.ReplayBuild = @replayBuild";
            }

            var leaderboardRankingJoin = GetLeaderboardRankingJoin(gameMode);

            await mySqlConnection.OpenAsync(_token);
            var cmdText = string.Format(
                mySqlCommandText,
                leagueId != -1 ? "and lr.LeagueID = @leagueID" : null,
                //gameMode == 3 ? "left join ReplayCharacter rcMirror on rcMirror.ReplayID = rc.ReplayID and rcMirror.PlayerID != rc.PlayerID and rcMirror.CharacterID = rc.CharacterID" : null,
                gameMode == 3 ? "left join replay_mirror rcMirror on rcMirror.ReplayID = rc.ReplayID" : null,
                gameMode == 3 ? "and rcMirror.ReplayID is null" : null,
                buildTerm,
                leaderboardRankingJoin);
            await using var mySqlCommand = new MySqlCommand(cmdText, mySqlConnection)
            {
                CommandTimeout = MMR.LongCommandTimeout,
            };
            mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
            mySqlCommand.Parameters.AddWithValue("@datetimeBegin", datetimeBegin);
            mySqlCommand.Parameters.AddWithValue("@datetimeEnd", datetimeEnd);
            if (leagueId != -1)
            {
                mySqlCommand.Parameters.AddWithValue("@leagueID", leagueId);
            }

            if (replayBuild != -1)
            {
                mySqlCommand.Parameters.AddWithValue("@replayBuild", replayBuild);
                if (upToBuild.HasValue)
                {
                    mySqlCommand.Parameters.AddWithValue("@upToBuild", upToBuild);
                }
            }

            LogSqlCommand(mySqlCommand);
            await using var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(_token);
            while (await mySqlDataReader.ReadAsync(_token))
            {
                var obj = new
                {
                    AverageLength = mySqlDataReader.GetTimeSpan("AverageLength"),
                    GamesPlayed = mySqlDataReader.GetInt64("GamesPlayed"),
                    MapID = mySqlDataReader.GetInt32("MapID"),
                    CharacterID = mySqlDataReader.GetInt32("CharacterID"),
                    GamesWon = mySqlDataReader.GetDecimal("GamesWon"),
                    AvgTakedowns = mySqlDataReader["AvgTakedowns"] is DBNull
                        ? 0
                        : mySqlDataReader.GetDecimal("AvgTakedowns"),
                    AvgSoloKills = mySqlDataReader["AvgSoloKills"] is DBNull
                        ? 0
                        : mySqlDataReader.GetDecimal("AvgSoloKills"),
                    AvgAssists = mySqlDataReader["AvgAssists"] is DBNull ? 0 : mySqlDataReader.GetDecimal("AvgAssists"),
                    AvgDeaths = mySqlDataReader["AvgDeaths"] is DBNull ? 0 : mySqlDataReader.GetDecimal("AvgDeaths"),
                    AvgHeroDamage = mySqlDataReader["AvgHeroDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgHeroDamage"),
                    AvgSiegeDamage = mySqlDataReader["AvgSiegeDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgSiegeDamage"),
                    AvgStructureDamage = mySqlDataReader["AvgStructureDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgStructureDamage"),
                    AvgMinionDamage = mySqlDataReader["AvgMinionDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgMinionDamage"),
                    AvgCreepDamage = mySqlDataReader["AvgCreepDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgCreepDamage"),
                    AvgSummonDamage = mySqlDataReader["AvgSummonDamage"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgSummonDamage"),
                    AvgTimeCCdEnemyHeroes = mySqlDataReader["AvgTimeCCdEnemyHeroes"] is DBNull
                        ? (TimeSpan?)null
                        : TimeSpan.FromSeconds((double)mySqlDataReader.GetDecimal("AvgTimeCCdEnemyHeroes")),
                    AvgHealing = mySqlDataReader["AvgHealing"] is DBNull
                        ? (int?)null
                        : (int)mySqlDataReader.GetDecimal("AvgHealing"),
                    AvgSelfHealing = mySqlDataReader["AvgSelfHealing"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgSelfHealing"),
                    AvgDamageTaken = mySqlDataReader["AvgDamageTaken"] is DBNull
                        ? (int?)null
                        : (int)mySqlDataReader.GetDecimal("AvgDamageTaken"),
                    AvgExperienceContribution = mySqlDataReader["AvgExperienceContribution"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgExperienceContribution"),
                    AvgTownKills = mySqlDataReader["AvgTownKills"] is DBNull
                        ? 0
                        : mySqlDataReader.GetDecimal("AvgTownKills"),
                    AvgTimeSpentDead = mySqlDataReader["AvgTimeSpentDead"] is DBNull
                        ? TimeSpan.Zero
                        : TimeSpan.FromSeconds((double)mySqlDataReader.GetDecimal("AvgTimeSpentDead")),
                    AvgMercCampCaptures = mySqlDataReader["AvgMercCampCaptures"] is DBNull
                        ? 0
                        : mySqlDataReader.GetDecimal("AvgMercCampCaptures"),
                    AvgWatchTowerCaptures = mySqlDataReader["AvgWatchTowerCaptures"] is DBNull
                        ? 0
                        : mySqlDataReader.GetDecimal("AvgWatchTowerCaptures"),
                    AvgMetaExperience = mySqlDataReader["AvgMetaExperience"] is DBNull
                        ? 0
                        : (int)mySqlDataReader.GetDecimal("AvgMetaExperience"),
                };

                if (!sitewideCharacterStatisticsDictionaryByMap.ContainsKey(obj.MapID))
                {
                    sitewideCharacterStatisticsDictionaryByMap[obj.MapID] = new Dictionary<int, SitewideMapStatistic>();
                }

                sitewideCharacterStatisticsDictionaryByMap[obj.MapID][obj.CharacterID] = new SitewideMapStatistic
                {
                    Map = locDic[obj.MapID],
                    Character = locDic[obj.CharacterID],
                    GamesPlayed = (int)obj.GamesPlayed,
                    AverageLength = obj.AverageLength,
                    WinPercent = obj.GamesWon / obj.GamesPlayed,
                    AverageScoreResult = new AverageTeamProfileReplayCharacterScoreResult
                    {
                        T = obj.AvgTakedowns,
                        S = obj.AvgSoloKills,
                        A = obj.AvgAssists,
                        D = obj.AvgDeaths,
                        HD = obj.AvgHeroDamage,
                        SiD = obj.AvgSiegeDamage,
                        StD = obj.AvgStructureDamage,
                        MD = obj.AvgMinionDamage,
                        CD = obj.AvgCreepDamage,
                        SuD = obj.AvgSummonDamage,
                        TCCdEH = obj.AvgTimeCCdEnemyHeroes,
                        H = obj.AvgHealing,
                        SH = obj.AvgSelfHealing,
                        DT = obj.AvgDamageTaken,
                        EC = obj.AvgExperienceContribution,
                        TK = obj.AvgTownKills,
                        TSD = obj.AvgTimeSpentDead,
                        MCC = obj.AvgMercCampCaptures,
                        WTC = obj.AvgWatchTowerCaptures,
                        ME = obj.AvgMetaExperience,
                    },
                };
            }
        }

        var sitewideCharacterBanStatisticsDictionaryByMap =
            new Dictionary<int, Dictionary<int, int>>();

        foreach (var key in sitewideCharacterStatisticsDictionaryByMap.Keys)
        {
            sitewideCharacterBanStatisticsDictionaryByMap[key] = await GetSitewideCharacterBanStatistics(
                datetimeBegin,
                datetimeEnd,
                leagueId,
                gameMode,
                replayBuild,
                mapId: key,
                upToBuild: upToBuild);
        }

        if (sitewideCharacterBanStatisticsDictionaryByMap.Count > 0)
        {
            foreach (var sitewideCharacterBanStatisticsForMap in sitewideCharacterBanStatisticsDictionaryByMap)
            {
                foreach (var sitewideCharacterBanStatistic in sitewideCharacterBanStatisticsForMap.Value)
                {
                    if (sitewideCharacterStatisticsDictionaryByMap.ContainsKey(
                            sitewideCharacterBanStatisticsForMap.Key) &&
                        sitewideCharacterStatisticsDictionaryByMap[sitewideCharacterBanStatisticsForMap.Key]
                            .ContainsKey(sitewideCharacterBanStatistic.Key))
                    {
                        sitewideCharacterStatisticsDictionaryByMap[sitewideCharacterBanStatisticsForMap.Key][
                            sitewideCharacterBanStatistic.Key].GamesBanned += sitewideCharacterBanStatistic.Value;
                    }
                    else
                    {
                        if (!sitewideCharacterStatisticsDictionaryByMap.ContainsKey(
                                sitewideCharacterBanStatisticsForMap.Key))
                        {
                            sitewideCharacterStatisticsDictionaryByMap[sitewideCharacterBanStatisticsForMap.Key] =
                                new Dictionary<int, SitewideMapStatistic>();
                        }

                        sitewideCharacterStatisticsDictionaryByMap[sitewideCharacterBanStatisticsForMap.Key][
                            sitewideCharacterBanStatistic.Key] = new SitewideMapStatistic
                            {
                                Map = locDic[sitewideCharacterBanStatisticsForMap.Key],
                                Character = locDic[sitewideCharacterBanStatistic.Key],
                                GamesPlayed = 0,
                                GamesBanned = sitewideCharacterBanStatistic.Value,
                                AverageLength = TimeSpan.Zero,
                                WinPercent = 0,
                            };
                    }
                }
            }
        }

        var sitewideMapStatisticsList = new List<SitewideMapStatistic>();
        foreach (var sitewideCharacterStatisticDictionaryForMap in sitewideCharacterStatisticsDictionaryByMap)
        {
            sitewideMapStatisticsList.AddRange(sitewideCharacterStatisticDictionaryForMap.Value.Values);
        }

        return sitewideMapStatisticsList.ToArray();
    }

    public async Task<HeroTalentBuildStatistic[]> GetSitewideHeroTalentPopularBuilds(
        DateTime datetimeBegin,
        DateTime datetimeEnd,
        int replayBuild,
        int characterId,
        int? mapId = null,
        int leagueId = -1,
        int gameMode = 8,
        bool includeStormTalents = false,
        int? upToBuild = null)
    {
        var source = await GetSitewideHeroTalentPopularBuilds2(
            datetimeBegin,
            datetimeEnd,
            replayBuild,
            gameMode,
            upToBuild);

        var target = GetSitewideHeroTalentPopularBuilds3(
            source,
            _locDic[characterId],
            replayBuild,
            characterId,
            mapId,
            leagueId,
            includeStormTalents,
            upToBuild);

        return target;
    }

    public async Task<List<HeroTalentBuildStatistic>> GetSitewideHeroTalentPopularBuilds2(
        DateTime datetimeBegin,
        DateTime datetimeEnd,
        int replayBuild,
        int gameMode = 8,
        int? upToBuild = null)
    {
        var mySqlCommandTextNormal =
            @"select q.CharacterID, q.LeagueID, q.MapID, count(*) as GamesPlayed, sum(q.IsWinner) as GamesWon, q.TalentSelection from
                    (select rc.CharacterID, lr.LeagueID, r.MapID, rc.ReplayID, rc.PlayerID, rc.IsWinner, t.TalentSelection
                    from replay_playertalentbuilds t
                    join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = t.ReplayID
                    join ReplayCharacter rc on rc.ReplayID = t.ReplayID and rc.PlayerID = t.PlayerID
                    {0}
                    join LeaderboardRanking lr on lr.PlayerID = rc.PlayerID and (lr.GameMode = r.gameMode or (r.gameMode in (7,9) and lr.GameMode = 3))
                    where (rc.CharacterLevel >= 5 or r.GameMode = 7 or r.GameMode >= 1000)
                    and r.TimestampReplay >= @datetimeBegin
                    and r.TimestampReplay < @datetimeEnd
                    {2}
                    and r.GameMode = @gameMode
                    {1}
                    group by rc.CharacterID, lr.LeagueID, r.MapID, rc.ReplayID, rc.PlayerID, rc.IsWinner) q
                    group by q.CharacterID, q.LeagueID, q.MapID, q.TalentSelection
                    order by GamesPlayed desc";

        var mySqlCommandTextEvent =
            @"select q.CharacterID, null LeagueID, q.MapID, count(*) as GamesPlayed, sum(q.IsWinner) as GamesWon, q.TalentSelection from
                    (select rc.CharacterID, r.MapID, rc.ReplayID, rc.PlayerID, rc.IsWinner, t.TalentSelection
                    from replay_playertalentbuilds t
                    join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = t.ReplayID
                    join ReplayCharacter rc on rc.ReplayID = t.ReplayID and rc.PlayerID = t.PlayerID
                    {0}
                    where 
                        r.TimestampReplay >= @datetimeBegin
                    and r.TimestampReplay < @datetimeEnd
                    {2}
                    and r.GameMode = @gameMode
                    {1}
                    group by rc.CharacterID, r.MapID, rc.ReplayID, rc.PlayerID, rc.IsWinner) q
                    group by q.CharacterID, q.MapID, q.TalentSelection
                    order by GamesPlayed desc";

        var heroTalentBuildStatistics = new List<HeroTalentBuildStatistic>();

        var mySqlCommandText = IsEventGameMode(gameMode)
            ? mySqlCommandTextEvent
            : mySqlCommandTextNormal;

        var replayTerm = upToBuild.HasValue
            ? "and r.ReplayBuild >= @replayBuild and r.ReplayBuild <= @upToBuild"
            : "and r.ReplayBuild = @replayBuild";

        var cmdText = string.Format(
            mySqlCommandText,
            //gameMode == 3 ? "left join ReplayCharacter rcMirror on rcMirror.ReplayID = rc.ReplayID and rcMirror.PlayerID != rc.PlayerID and rcMirror.CharacterID = rc.CharacterID" : null,
            gameMode == 3 ? "left join replay_mirror rcMirror on rcMirror.ReplayID = rc.ReplayID" : null,
            gameMode == 3 ? "and rcMirror.ReplayID is null" : null,
            replayTerm);

        await RetryIfTmpTableFull(ReadFromSql);

        async Task ReadFromSql()
        {
            await using var mySqlConnection = new MySqlConnection(ConnectionString);
            await mySqlConnection.OpenAsync(_token);
            await using var mySqlCommand =
                new MySqlCommand(cmdText, mySqlConnection) { CommandTimeout = MMR.LongCommandTimeout };
            mySqlCommand.Parameters.AddWithValue("@datetimeBegin", datetimeBegin);
            mySqlCommand.Parameters.AddWithValue("@datetimeEnd", datetimeEnd);
            mySqlCommand.Parameters.AddWithValue("@replayBuild", replayBuild);
            if (upToBuild.HasValue)
            {
                mySqlCommand.Parameters.AddWithValue("@upToBuild", upToBuild);
            }

            mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);

            // Read Statistics and Talent Selection CSV
            LogSqlCommand(mySqlCommand);
            await using var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(_token);
            while (await mySqlDataReader.ReadAsync(_token))
            {
                var obj = new
                {
                    CharacterID = mySqlDataReader.GetInt32("CharacterID"),
                    LeagueID =
                        mySqlDataReader["LeagueID"] is DBNull ? (int?)null : mySqlDataReader.GetInt32("LeagueID"),
                    MapID = mySqlDataReader.GetInt32("MapID"),
                    GamesPlayed = mySqlDataReader.GetInt64("GamesPlayed"),
                    GamesWon = (int)mySqlDataReader.GetDecimal("GamesWon"),
                    TalentSelection = mySqlDataReader.GetString("TalentSelection"),
                };

                heroTalentBuildStatistics.Add(
                    new HeroTalentBuildStatistic
                    {
                        GamesPlayed = (int)obj.GamesPlayed,
                        TalentNameDescription = { [0] = obj.TalentSelection },
                        GamesWon = obj.GamesWon,
                        CharacterID = obj.CharacterID,
                        LeagueID = obj.LeagueID,
                        MapID = obj.MapID,
                    });
            }
        }

        return heroTalentBuildStatistics.ToList();
    }

    public HeroTalentBuildStatistic[] GetSitewideHeroTalentPopularBuilds3(
        List<HeroTalentBuildStatistic> source,
        string characterName,
        int replayBuild,
        int characterId,
        int? mapId = null,
        int leagueId = -1,
        bool includeStormTalents = false,
        int? upToBuild = null)
    {
        List<IGrouping<string, HeroTalentBuildStatistic>> q;
        if (mapId.HasValue && leagueId != -1)
        {
            q = source
                .Where(x => x.CharacterID == characterId)
                .Where(x => x.MapID == mapId.Value)
                .Where(x => x.LeagueID == leagueId)
                .GroupBy(x => ExcludeStormTalentsIfNecessary(includeStormTalents, x.TalentNameDescription[0]))
                .ToList();
        }
        else if (mapId.HasValue)
        {
            q = source
                .Where(x => x.CharacterID == characterId)
                .Where(x => x.MapID == mapId.Value)
                .GroupBy(x => ExcludeStormTalentsIfNecessary(includeStormTalents, x.TalentNameDescription[0]))
                .ToList();
        }
        else if (leagueId != -1)
        {
            q = source
                .Where(x => x.CharacterID == characterId)
                .Where(x => x.LeagueID == leagueId)
                .GroupBy(x => ExcludeStormTalentsIfNecessary(includeStormTalents, x.TalentNameDescription[0]))
                .ToList();
        }
        else
        {
            q = source
                .Where(x => x.CharacterID == characterId)
                .GroupBy(x => ExcludeStormTalentsIfNecessary(includeStormTalents, x.TalentNameDescription[0]))
                .ToList();
        }

        var q2 = from x in q
                 let gamesPlayed = x.Sum(y => y.GamesPlayed)
                 let gamesWon = x.Sum(y => y.GamesWon)
                 orderby gamesPlayed descending
                 select new HeroTalentBuildStatistic
                 {
                     CharacterID = characterId,
                     GamesPlayed = gamesPlayed,
                     WinPercent = 1.0m * gamesWon / gamesPlayed,
                     TalentNameDescription = { [0] = x.Key },
                 };
        source = q2.ToList();

        // Currently we have queried the Top 1000 most popular builds for this Hero
        // Many of these builds only differ by one or two talents though
        // Let's group these together so we can show a greater variety of builds
        for (var i = 0; i < source.Count; i++)
        {
            // Compare the current build against all other builds
            var currentBuild = SplitTalentSelection(source[i].TalentNameDescription[0]);
            var isSimilarBuildFoundToGroupBy = false;

            for (var j = i + 1; j < source.Count; j++)
            {
                // Loop through all builds to compare to the 'current' build from above
                var comparingBuild = SplitTalentSelection(source[j].TalentNameDescription[0]);
                var buildDifferences = new List<int>();

                // Count the build differences
                for (var k = 0; k < currentBuild.Length; k++)
                {
                    if (k < comparingBuild.Length && currentBuild[k] != comparingBuild[k])
                    {
                        buildDifferences.Add(k);
                    }
                }

                if (buildDifferences.Count <= 1)
                {
                    // Tolerable talent difference between the builds; if 'Games Played' is limited, let's group by similar builds
                    if (!isSimilarBuildFoundToGroupBy && source[i].GamesPlayed <
                        DataHelper.GamesPlayedRequirementForWinPercentDisplay)
                    {
                        foreach (var t in buildDifferences)
                        {
                            currentBuild[t] = -1;
                        }

                        source[i].TalentNameDescription[0] = string.Join(",", currentBuild);

                        isSimilarBuildFoundToGroupBy = true;
                    }

                    if (isSimilarBuildFoundToGroupBy)
                    {
                        source[i] += source[j];
                    }

                    source.RemoveAt(j--);
                }
            }
        }

        var talentDic = _talentsCache
            .Where(
                i => i.Character == characterName && replayBuild >= i.ReplayBuildFirst &&
                     replayBuild <= i.ReplayBuildLast)
            .ToDictionary(i => i.TalentId, i => i);

        // Split the Talent Selection CSV and Cache Talent Title and Description
        foreach (var bld in source)
        {
            var talentIds = SplitTalentSelection(bld.TalentNameDescription[0]);

            for (var tier = 0; tier < talentIds.Length && (tier <= 5 || (tier <= 6 && includeStormTalents)); tier++)
            {
                if (talentDic.ContainsKey(talentIds[tier]))
                {
                    var tInfo = talentDic[talentIds[tier]];
                    bld.TalentImageURL[tier] = tInfo.TalentName.PrepareForImageURL();
                    bld.TalentNameDescription[tier] = $"{tInfo.TalentName}: {tInfo.TalentDescription}";
                    bld.TalentName[tier] = tInfo.TalentName;
                }
                else if (talentIds[tier] == -1)
                {
                    bld.TalentImageURL[tier] = "PlayerChoice";
                    bld.TalentNameDescription[tier] = "Player's Choice";
                    bld.TalentName[tier] = "Player's Choice";
                }
                else
                {
                    bld.TalentImageURL[tier] = "UnknownTalent";
                    bld.TalentNameDescription[tier] = $"Talent {talentIds[tier]}";
                    bld.TalentName[tier] = $"Talent {talentIds[tier]}";
                }
            }
        }

        return source
            .OrderByDescending(i => i.GamesPlayed)
            .Take(10)
            .OrderByDescending(i => i.WinPercent)
            .ToArray();
    }

    private int[] SplitTalentSelection(string talentSelection)
    {
        return string.IsNullOrWhiteSpace(talentSelection)
            ? Array.Empty<int>()
            : talentSelection.Split(',').Select(int.Parse).ToArray();
    }

    public async Task<Dictionary<int, int>> GetSitewideCharacterBanStatistics(
        DateTime datetimeBegin,
        DateTime datetimeEnd,
        int leagueId = -1,
        int gameMode = 8,
        int replayBuild = -1,
        int mapId = -1,
        int? upToBuild = null)
    {
        const string mySqlCommandText =
            @"select rthb.CharacterID, count(*) as GamesBanned from
                (select rc.ReplayID
                from ReplayCharacter rc
                join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
                {3}
                where r.GameMode = @gameMode and r.TimestampReplay >= @datetimeBegin and r.TimestampReplay < @datetimeEnd and (rc.CharacterLevel >= 5 or r.GameMode = 7 or r.GameMode >= 1000) {1} {2}
                group by rc.ReplayID {0}) q
                join ReplayTeamHeroBan rthb on rthb.ReplayID = q.ReplayID
                group by rthb.CharacterID";

        var sitewideCharacterBanStatistics = new Dictionary<int, int>();

        string buildTerm = null;
        if (replayBuild != -1)
        {
            buildTerm = upToBuild.HasValue
                ? "and r.ReplayBuild >= @replayBuild and r.ReplayBuild <= @upToBuild"
                : "and r.ReplayBuild = @replayBuild";
        }

        var leaderboardRankingJoin = GetLeaderboardRankingJoin(gameMode);

        var cmdText = string.Format(
            mySqlCommandText,
            leagueId != -1 ? "having round(avg(lr.LeagueID)) = @leagueID" : null,
            buildTerm,
            mapId != -1 ? "and r.MapID = @mapID" : null,
            leaderboardRankingJoin);

        await RetryIfTmpTableFull(ReadFromSql);

        async Task ReadFromSql()
        {
            await using var mySqlConnection = new MySqlConnection(ConnectionString);
            await mySqlConnection.OpenAsync(_token);
            await using var mySqlCommand = new MySqlCommand(cmdText, mySqlConnection)
            {
                CommandTimeout = MMR.LongCommandTimeout,
            };
            mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
            mySqlCommand.Parameters.AddWithValue("@datetimeBegin", datetimeBegin);
            mySqlCommand.Parameters.AddWithValue("@datetimeEnd", datetimeEnd);
            if (leagueId != -1)
            {
                mySqlCommand.Parameters.AddWithValue("@leagueID", leagueId);
            }

            if (replayBuild != -1)
            {
                mySqlCommand.Parameters.AddWithValue("@replayBuild", replayBuild);
                if (upToBuild.HasValue)
                {
                    mySqlCommand.Parameters.AddWithValue("@upToBuild", upToBuild);
                }
            }

            if (mapId != -1)
            {
                mySqlCommand.Parameters.AddWithValue("@mapID", mapId);
            }

            LogSqlCommand(mySqlCommand);
            await using var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(_token);
            while (await mySqlDataReader.ReadAsync(_token))
            {
                var obj = new
                {
                    CharacterID = mySqlDataReader.GetInt32("CharacterID"),
                    GamesBanned = mySqlDataReader.GetInt64("GamesBanned"),
                };

                sitewideCharacterBanStatistics[obj.CharacterID] = (int)obj.GamesBanned;
            }
        }

        return sitewideCharacterBanStatistics;
    }

    public async Task<SitewideTeamCompositionStatistic[]> GetSitewideTeamCompositionStatistics(
        DateTime datetimeBegin,
        DateTime datetimeEnd,
        int? mapId = null,
        bool breakUpLargeQueries = true,
        string queryByRole = null)
    {
        if (datetimeEnd <= datetimeBegin)
        {
            return null;
        }

        if (breakUpLargeQueries && (datetimeEnd - datetimeBegin).TotalDays > 1)
        {
            // Break up a larger query into smaller ones using recursive calls
            var sitewideTeamCompositionStatisticsSum = Array.Empty<SitewideTeamCompositionStatistic>();
            for (; datetimeBegin < datetimeEnd; datetimeBegin = datetimeBegin.AddDays(1))
            {
                var datetimeEndRange = datetimeBegin.AddDays(1);
                if (datetimeEndRange > datetimeEnd)
                {
                    datetimeEndRange = datetimeEnd;
                }

                var dailySitewideTeamCompositionStatistic = await GetSitewideTeamCompositionStatistics(
                    datetimeBegin,
                    datetimeEndRange,
                    mapId,
                    false,
                    queryByRole);
                sitewideTeamCompositionStatisticsSum = sitewideTeamCompositionStatisticsSum
                    .Concat(dailySitewideTeamCompositionStatistic).GroupBy(i => i.CharacterNamesCSV).Select(
                        i => new SitewideTeamCompositionStatistic
                        {
                            GamesPlayed = i.Sum(j => j.GamesPlayed),
                            WinPercent = i.Sum(j => j.GamesPlayed * j.WinPercent) / i.Sum(j => j.GamesPlayed),
                            CharacterNamesCSV = i.Key,
                        }).ToArray();
            }

            return sitewideTeamCompositionStatisticsSum;
        }

        const string mySqlCommandTextCharacter =
            @"select q.`Character`, count(*) as GamesPlayed, sum(q.IsWinner) / count(*) as WinPercent from
                (select group_concat(la.PrimaryName order by la.PrimaryName) as `Character`, rc.IsWinner
                from ReplayCharacter rc
                join Replay r use index(IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
                join LocalizationAlias la on la.IdentifierId = rc.CharacterID
                where r.GameMode = 8 and r.TimestampReplay > @datetimeBegin and r.TimestampReplay <= @datetimeEnd {0} {1}
                group by rc.ReplayID, rc.IsWinner) q
                group by q.`Character`
                order by GamesPlayed desc";

        const string mySqlCommandTextCharacterRole =
            @"select q.`Character`, count(*) as GamesPlayed, sum(q.IsWinner) / count(*) as WinPercent from
                (select group_concat(la.`{1}` order by case la.`{1}`
                when 'Tank' then 0
                when 'Bruiser' then 1
                when 'Warrior' then 2           -- obsolete
                when 'Healer' then 3
                when 'Support' then 4
                when 'Ambusher' then 5          -- obsolete
                when 'Burst Damage' then 6      -- obsolete
                when 'Sustained Damage' then 7  -- obsolete
                when 'Assassin' then 8          -- obsolete
                when 'Siege' then 9             -- obsolete
                when 'Utility' then 10          -- obsolete
                when 'Specialist' then 11       -- obsolete
                when 'Ranged Assassin' then 12
                when 'Melee Assassin' then 13
                else 14 end
                ) as `Character`, rc.IsWinner
                from ReplayCharacter rc
                join Replay r use index(IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
                join LocalizationAlias la on la.IdentifierId = rc.CharacterID
                where r.GameMode = 8 and r.TimestampReplay > @datetimeBegin and r.TimestampReplay <= @datetimeEnd {0}
                group by rc.ReplayID, rc.IsWinner) q
                group by q.`Character`
                order by GamesPlayed desc";

        // ReSharper disable once CollectionNeverUpdated.Local
        var sitewideTeamCompositionStatistics = new List<SitewideTeamCompositionStatistic>();
        var cmdText = string.Format(
            queryByRole != null ? mySqlCommandTextCharacterRole : mySqlCommandTextCharacter,
            mapId.HasValue ? "and r.MapID = @mapID" : null,
            queryByRole);

        await RetryIfTmpTableFull(ReadFromSql);

        async Task ReadFromSql()
        {
            await using var mySqlConnection = new MySqlConnection(ConnectionString);
            await mySqlConnection.OpenAsync(_token);
            await using var mySqlCommand = new MySqlCommand(cmdText, mySqlConnection)
            {
                CommandTimeout = MMR.LongCommandTimeout,
            };
            mySqlCommand.Parameters.AddWithValue("@datetimeBegin", datetimeBegin);
            mySqlCommand.Parameters.AddWithValue("@datetimeEnd", datetimeEnd);

            if (mapId.HasValue)
            {
                mySqlCommand.Parameters.AddWithValue("@mapID", mapId.Value);
            }

            // Read Statistics and Talent Selection CSV                         -- REMOVED 2/26/2020 due to DB loss
            // -- Restored 13/5/2020 why was it removed anyway? -- Aviad
            LogSqlCommand(mySqlCommand);
            await using var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(_token);
            while (await mySqlDataReader.ReadAsync(_token))
            {
                var obj = new
                {
                    GamesPlayed = mySqlDataReader.GetInt64("GamesPlayed"),
                    WinPercent = mySqlDataReader.GetDecimal("WinPercent"),
                    Character = mySqlDataReader.GetString("Character"),
                };

                sitewideTeamCompositionStatistics.Add(
                    new SitewideTeamCompositionStatistic
                    {
                        GamesPlayed = (int)obj.GamesPlayed,
                        WinPercent = obj.WinPercent,
                        CharacterNamesCSV = obj.Character,
                    });
            }
        }

        return sitewideTeamCompositionStatistics.OrderByDescending(i => i.GamesPlayed).ToArray();
    }

    public void ScrubSitewideTeamCompositionStatistics(
        SitewideTeamCompositionStatistic[] sitewideTeamCompositionStatistics)
    {
        foreach (var t in sitewideTeamCompositionStatistics)
        {
            var characterArray = t.CharacterNamesCSV.Split(',');

            for (var j = 0; j < characterArray.Length; j++)
            {
                switch (j)
                {
                    case 0:
                        t.CharacterName1 = characterArray[j];
                        t.CharacterImageURL1 = characterArray[j].PrepareForImageURL();
                        break;
                    case 1:
                        t.CharacterName2 = characterArray[j];
                        t.CharacterImageURL2 = characterArray[j].PrepareForImageURL();
                        break;
                    case 2:
                        t.CharacterName3 = characterArray[j];
                        t.CharacterImageURL3 = characterArray[j].PrepareForImageURL();
                        break;
                    case 3:
                        t.CharacterName4 = characterArray[j];
                        t.CharacterImageURL4 = characterArray[j].PrepareForImageURL();
                        break;
                    case 4:
                        t.CharacterName5 = characterArray[j];
                        t.CharacterImageURL5 = characterArray[j].PrepareForImageURL();
                        break;
                }
            }
        }
    }

    public async Task SetMapObjectiveStatistics(
        int gameMode,
        DateTime currentDateTimeBegin,
        DateTime currentDateTimeEnd)
    {
        const string mySqlCommandText =
            @"select
                r.ReplayID,
                r.GameMode,
                r.MapID,
                r.ReplayLength,
                rto.IsWinner,
                rto.TeamObjectiveType,
                rto.TimeSpan,
                rc.CharacterID,
                rto.`Value`
                from Replay r use index (IX_GameMode_TimestampReplay)
                join ReplayTeamObjective rto on rto.ReplayID = r.ReplayID
                left join ReplayCharacter rc on rc.ReplayID = rto.ReplayID and rc.PlayerID = rto.PlayerID
                where r.TimestampReplay > @datetimeBegin and r.TimestampReplay < @datetimeEnd and r.GameMode = {0}
                order by r.ReplayID, rto.TimeSpan";

        var replayDictionary = new Dictionary<int, TeamObjectiveReplay>();

        await RetryIfTmpTableFull(ReadFromSql);

        async Task ReadFromSql()
        {
            await using var mySqlConnection = new MySqlConnection(ConnectionString);
            await mySqlConnection.OpenAsync(_token);
            var cmdText = string.Format(mySqlCommandText, gameMode);
            await using var mySqlCommand = new MySqlCommand(cmdText, mySqlConnection)
            {
                CommandTimeout = MMR.LongCommandTimeout,
            };
            mySqlCommand.Parameters.AddWithValue("@datetimeBegin", currentDateTimeBegin);
            mySqlCommand.Parameters.AddWithValue("@datetimeEnd", currentDateTimeEnd);

            LogSqlCommand(mySqlCommand);
            await using var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(_token);
            while (await mySqlDataReader.ReadAsync(_token))
            {
                var obj = new
                {
                    ReplayID = mySqlDataReader.GetInt32("ReplayID"),
                    GameMode = mySqlDataReader.GetInt32("GameMode"),
                    MapID = mySqlDataReader.GetInt32("MapID"),
                    ReplayLength = mySqlDataReader.GetTimeSpan("ReplayLength"),
                    IsWinner = mySqlDataReader.GetUInt64("IsWinner"),
                    TeamObjectiveType = mySqlDataReader.GetInt32("TeamObjectiveType"),
                    TimeSpan = mySqlDataReader.GetTimeSpan("TimeSpan"),
                    CharacterID = mySqlDataReader.IsDBNull(mySqlDataReader.GetOrdinal("CharacterID"))
                        ? (int?)null
                        : mySqlDataReader.GetInt32("CharacterID"),
                    Value = mySqlDataReader.GetInt32("Value"),
                };

                if (!replayDictionary.ContainsKey(obj.ReplayID))
                {
                    replayDictionary[obj.ReplayID] = new TeamObjectiveReplay
                    {
                        GameMode = obj.GameMode,
                        MapID = obj.MapID,
                        ReplayLength = obj.ReplayLength,
                    };
                }

                replayDictionary[obj.ReplayID].TeamObjectives.Add(
                    new TeamObjectiveRTO
                    {
                        IsWinner = obj.IsWinner == 1,
                        TeamObjectiveType = obj.TeamObjectiveType,
                        TimeSpan = obj.TimeSpan,
                        CharacterID = obj.CharacterID,
                        Value = obj.Value,
                    });
            }
        }

        var localizationAliasMaps = _dc.LocalizationAliases
            .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map).ToArray();
        var mapIdDictionary = _dc.LocalizationAliases.ToDictionary(i => i.IdentifierId, i => i.PrimaryName);

        foreach (var map in localizationAliasMaps.Union(
                     new[]
                     {
                         new LocalizationAlias
                         {
                             IdentifierId = -1,
                             PrimaryName = "All Maps",
                         },
                     }))
        {
            switch (map.PrimaryName)
            {
                case "All Maps":
                {
                    var catapultRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(j => j.TeamObjectiveType == (int)TeamObjectiveType.FirstCatapultSpawn)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var winRatePerMapForFirstCatapultRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Win Rate with First Catapult Spawn",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = catapultRTOs.GroupBy(i => i.ReplayID).Select(
                                i => new
                                {
                                    replayDictionary[i.Key].MapID,
                                    i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner,
                                })
                            .GroupBy(i => i.MapID).Select(
                                i => new
                                {
                                    MapID = i.Key,
                                    GamesPlayed = i.Count(),
                                    WinPercent = i.Any() ? (decimal)i.Count(j => j.IsWinner) / i.Count() : 0m,
                                })
                            .OrderByDescending(i => i.WinPercent).Select(
                                i => new TeamObjectiveRTOSummaryRadGridRow
                                {
                                    RowTitle = mapIdDictionary[i.MapID],
                                    GamesPlayed = i.GamesPlayed,
                                    Value = i.WinPercent,
                                }).ToList(),
                    };

                    var bossRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(j => j.TeamObjectiveType == (int)TeamObjectiveType.BossCampCaptureWithCampID)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();
                    var replayCountByMapWithoutBoss = replayDictionary
                        .Where(
                            i => i.Value.TeamObjectives.All(
                                j =>
                                    j.TeamObjectiveType != (int)TeamObjectiveType.BossCampCaptureWithCampID))
                        .GroupBy(i => i.Value.MapID).ToDictionary(i => i.Key, i => i.Count());

                    var winRatePerMapForFirstBossCaptureRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Win Rate with First Boss Capture",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = bossRTOs.GroupBy(i => i.ReplayID).Select(
                                i => new
                                {
                                    replayDictionary[i.Key].MapID,
                                    i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner,
                                })
                            .GroupBy(i => i.MapID).Select(
                                i => new
                                {
                                    MapID = i.Key,
                                    GamesPlayed = i.Count(),
                                    WinPercent = i.Any() ? (decimal)i.Count(j => j.IsWinner) / i.Count() : 0m,
                                })
                            .OrderByDescending(i => i.WinPercent).Select(
                                i => new TeamObjectiveRTOSummaryRadGridRow
                                {
                                    RowTitle = mapIdDictionary[i.MapID],
                                    GamesPlayed = i.GamesPlayed,
                                    Value = i.WinPercent,
                                }).ToList(),
                    };

                    var gamesEndedByBossRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Percent of Games Ended with a Boss Push",
                        ValueColumnHeaderText = "Percent of Games",
                        ValueFormatString = "P1",
                        RadGridRows = bossRTOs.GroupBy(i => i.ReplayID).Select(
                                i => new
                                {
                                    replayDictionary[i.Key].MapID,
                                    IsGameEndedByBoss =
                                        i.Any(j => j.TeamObjectiveRTO.IsWinner) && i
                                            .Where(j => j.TeamObjectiveRTO.IsWinner)
                                            .OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Last().TeamObjectiveRTO
                                            .TimeSpan > replayDictionary[i.Key].ReplayLength
                                            .Add(TimeSpan.FromSeconds(-100)),
                                })
                            .GroupBy(i => i.MapID).Select(
                                i => new
                                {
                                    MapID = i.Key,
                                    GamesPlayed = i.Count(j => j.IsGameEndedByBoss),
                                    PercentGamesEndedByBoss = i.Any()
                                        ? (decimal)i.Count(j => j.IsGameEndedByBoss) /
                                          (i.Count() + (replayCountByMapWithoutBoss.ContainsKey(i.Key)
                                              ? replayCountByMapWithoutBoss[i.Key]
                                              : 0))
                                        : 0m,
                                })
                            .OrderByDescending(i => i.PercentGamesEndedByBoss).Select(
                                i =>
                                    new TeamObjectiveRTOSummaryRadGridRow
                                    {
                                        RowTitle = mapIdDictionary[i.MapID],
                                        GamesPlayed = i.GamesPlayed,
                                        Value = i.PercentGamesEndedByBoss,
                                    }).ToList(),
                    };

                    var gamesWonAndLostByBossCount = bossRTOs.GroupBy(i => i.TeamObjectiveRTO.IsWinner)
                        .ToDictionary(
                            i => i.Key,
                            i => i.GroupBy(j => j.ReplayID).Select(j => j.Count()).GroupBy(j => j)
                                .ToDictionary(j => j.Key, j => j.Count()));

                    if (gamesWonAndLostByBossCount.Count != 2)
                    {
                        break;
                    }

                    // Also count win rate with 0 bosses, but only on maps with a boss camp
                    var mapsWithBosses = bossRTOs.Select(i => replayDictionary[i.ReplayID].MapID).Distinct()
                        .ToDictionary(i => i, _ => true);
                    var rtosWithoutBossWinsAndLosses = replayDictionary.Values
                        .Where(i => mapsWithBosses.ContainsKey(i.MapID)).Select(
                            i =>
                                new[] { true, false }
                                    .Select(
                                        j =>
                                            i.TeamObjectives.Any(
                                                k =>
                                                    k.IsWinner == j && k.TeamObjectiveType ==
                                                    (int)TeamObjectiveType.BossCampCaptureWithCampID)
                                                ? (bool?)null
                                                : j)
                                    .Where(j => j.HasValue)
                                    .ToDictionary(j => j!.Value, j => j!.Value)).ToArray();
                    foreach (var isWinner in gamesWonAndLostByBossCount.Keys)
                    {
                        gamesWonAndLostByBossCount[isWinner][0] =
                            rtosWithoutBossWinsAndLosses.Count(i => i.ContainsKey(isWinner));
                    }

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids =
                                new[]
                                {
                                    winRatePerMapForFirstCatapultRadGrid,
                                    winRatePerMapForFirstBossCaptureRadGrid,
                                    gamesEndedByBossRadGrid,
                                }.Where(i => i.RadGridRows.Count > 0).ToArray(),
                            TeamObjectiveRTOSummaryRadHtmlCharts = new[]
                            {
                                new TeamObjectiveRTOSummaryRadHtmlChart
                                {
                                    RadHtmlChartTitle = "Win Rate by Bosses Collected in One Game",
                                    XValueTitle = "Bosses Collected in One Game",
                                    XValueFormatString = "N0",
                                    YValueTitle = "Win Percent",
                                    YValueFormatString = "P1",
                                    RadHtmlChartItems = gamesWonAndLostByBossCount[true].Select(
                                            i => new
                                            {
                                                BossCount = i.Key,
                                                GamesPlayed =
                                                    i.Value + (gamesWonAndLostByBossCount[false]
                                                        .ContainsKey(i.Key)
                                                        ? gamesWonAndLostByBossCount[false][i.Key]
                                                        : 0),
                                                WinRate = i.Value > 0
                                                    ? (decimal)i.Value /
                                                      (i.Value + (gamesWonAndLostByBossCount[false]
                                                          .ContainsKey(i.Key)
                                                          ? gamesWonAndLostByBossCount[false][i.Key]
                                                          : 0))
                                                    : 0m,
                                            })
                                        .Where(i => i.GamesPlayed > 100)
                                        .OrderBy(i => i.BossCount).Select(
                                            i =>
                                                new TeamObjectiveRTOSummaryRadHtmlChartItem
                                                {
                                                    Tooltip =
                                                        TeamObjectiveRTOSummaryRadHtmlChartItem
                                                            .FormatTooltipForHtml(
                                                                i.GamesPlayed,
                                                                i.WinRate,
                                                                "P1",
                                                                "Win Percent"),
                                                    XValue = i.BossCount,
                                                    YValue = i.WinRate,
                                                }).ToList(),
                                },
                            },
                        },
                        TimeSpan.FromDays(30));
                }
                break;

                case "Battlefield of Eternity":
                {
                    var battlefieldOfEternityImmortalFightEndWithPowerPercentRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(
                                    j => j.TeamObjectiveType == (int)TeamObjectiveType
                                        .BattlefieldOfEternityImmortalFightEndWithPowerPercent)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var firstImmortalRTOs = battlefieldOfEternityImmortalFightEndWithPowerPercentRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner)
                        .ToArray();
                    var winRateWithFirstImmortal = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First Immortal",
                        GamesPlayed = firstImmortalRTOs.Length,
                        Value = firstImmortalRTOs.Length > 0
                            ? (decimal)firstImmortalRTOs.Count(i => i) / firstImmortalRTOs.Length
                            : 0,
                    };

                    var firstTwoImmortalRTOs = battlefieldOfEternityImmortalFightEndWithPowerPercentRTOs
                        .GroupBy(i => i.ReplayID)
                        .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Take(2).ToArray())
                        .Where(i => i.Length == 2).Select(
                            i => new
                            {
                                FirstTwoImmortalWin = i.All(j => j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                FirstTwoImmortalLoss = i.All(j => !j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                            }).ToArray();
                    var firstTwoImmortalGamesPlayed =
                        firstTwoImmortalRTOs.Sum(i => i.FirstTwoImmortalWin + i.FirstTwoImmortalLoss);
                    var winRateWithFirstTwoImmortal = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First and Second Immortal",
                        GamesPlayed = firstTwoImmortalGamesPlayed,
                        Value = firstTwoImmortalGamesPlayed > 0
                            ? (decimal)firstTwoImmortalRTOs.Sum(i => i.FirstTwoImmortalWin) /
                              firstTwoImmortalGamesPlayed
                            : 0,
                    };

                    var gamesThatEndWithObjective = battlefieldOfEternityImmortalFightEndWithPowerPercentRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.Any(j => j.TeamObjectiveRTO.IsWinner) && i.Where(j => j.TeamObjectiveRTO.IsWinner)
                                    .OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Last().TeamObjectiveRTO.TimeSpan >
                                replayDictionary[i.Key].ReplayLength.Add(TimeSpan.FromSeconds(-120))).ToArray();
                    var gamesEndedByImmortal = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Percent of Games Ended with an Immortal Push",
                        GamesPlayed = gamesThatEndWithObjective.Count(i => i),
                        Value = gamesThatEndWithObjective.Length > 0
                            ? (decimal)gamesThatEndWithObjective.Count(i => i) /
                              gamesThatEndWithObjective.Length
                            : 0,
                    };

                    var battlefieldOfEternityRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Immortal Fights",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = new List<TeamObjectiveRTOSummaryRadGridRow>
                        {
                            winRateWithFirstImmortal,
                            winRateWithFirstTwoImmortal,
                            gamesEndedByImmortal,
                        },
                    };

                    var gamesWonAndLostByObjectiveCount = battlefieldOfEternityImmortalFightEndWithPowerPercentRTOs
                        .GroupBy(i => i.TeamObjectiveRTO.IsWinner).ToDictionary(
                            i => i.Key,
                            i => i.GroupBy(j => j.ReplayID).Select(j => j.Count()).GroupBy(j => j)
                                .ToDictionary(j => j.Key, j => j.Count()));

                    if (gamesWonAndLostByObjectiveCount.Count != 2)
                    {
                        break;
                    }

                    // Also count win rate with 0 objectives
                    var rtosWithoutObjectiveWinsAndLosses = replayDictionary.Values
                        .Where(i => i.MapID == map.IdentifierId).Select(
                            i =>
                                new[] { true, false }
                                    .Select(
                                        j =>
                                            !i.TeamObjectives.Any(
                                                k =>
                                                    k.IsWinner == j && k.TeamObjectiveType == (int)TeamObjectiveType
                                                        .BattlefieldOfEternityImmortalFightEndWithPowerPercent)
                                                ? j
                                                : (bool?)null)
                                    .Where(j => j.HasValue)
                                    .ToDictionary(j => j!.Value, j => j!.Value)).ToArray();
                    foreach (var isWinner in gamesWonAndLostByObjectiveCount.Keys)
                    {
                        gamesWonAndLostByObjectiveCount[isWinner][0] =
                            rtosWithoutObjectiveWinsAndLosses.Count(i => i.ContainsKey(isWinner));
                    }

                    var winRateByObjectiveCountRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Immortal Objectives Won in One Game",
                        XValueTitle = "Immortal Objectives Won in One Game",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = gamesWonAndLostByObjectiveCount[true].Select(
                                i => new
                                {
                                    ObjectiveCount = i.Key,
                                    GamesPlayed =
                                        i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                            ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                            : 0),
                                    WinRate = i.Value > 0
                                        ? (decimal)i.Value /
                                          (i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                              ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                              : 0))
                                        : 0m,
                                })
                            .Where(i => i.GamesPlayed > 100 || i.ObjectiveCount == 0)
                            .OrderBy(i => i.ObjectiveCount).Select(
                                i => new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip =
                                        TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                            i.GamesPlayed,
                                            i.WinRate,
                                            "P1",
                                            "Win Percent"),
                                    XValue = i.ObjectiveCount,
                                    YValue = i.WinRate,
                                }).ToList(),
                    };

                    var winPercentByImmortalPowerPercent = battlefieldOfEternityImmortalFightEndWithPowerPercentRTOs
                        .GroupBy(i => (decimal)(i.TeamObjectiveRTO.Value / 5 * 5) / 100).Where(i => i.Count() > 100)
                        .Select(
                            i => new
                            {
                                ImmortalPowerPercent = i.Key,
                                GamesPlayed = i.Count(),
                                WinPercent = (decimal)i.Count(j => j.TeamObjectiveRTO.IsWinner) / i.Count(),
                            }).OrderBy(i => i.ImmortalPowerPercent).ToArray();

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids = new[] { battlefieldOfEternityRadGrid },
                            TeamObjectiveRTOSummaryRadHtmlCharts = new[]
                            {
                                winRateByObjectiveCountRadHtmlChart,
                                new TeamObjectiveRTOSummaryRadHtmlChart
                                {
                                    RadHtmlChartTitle = "Win Rate by Immortal Shield Percent",
                                    XValueTitle = "Immortal Shield Percent",
                                    XValueFormatString = "P0",
                                    YValueTitle = "Win Percent",
                                    YValueFormatString = "P1",
                                    RadHtmlChartItems = winPercentByImmortalPowerPercent.Select(
                                        i =>
                                            new TeamObjectiveRTOSummaryRadHtmlChartItem
                                            {
                                                Tooltip =
                                                    TeamObjectiveRTOSummaryRadHtmlChartItem
                                                        .FormatTooltipForHtml(
                                                            i.GamesPlayed,
                                                            i.WinPercent,
                                                            "P1",
                                                            "Win Percent"),
                                                XValue = i.ImmortalPowerPercent,
                                                YValue = i.WinPercent,
                                            }).ToList(),
                                },
                            },
                        },
                        TimeSpan.FromDays(30));
                }
                break;

                case "Blackheart's Bay":
                {
                    var blackheartsBayGhostShipCapturedWithCoinCostRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(
                                    j => j.TeamObjectiveType ==
                                         (int)TeamObjectiveType.BlackheartsBayGhostShipCapturedWithCoinCost)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var firstTurnInRTOs = blackheartsBayGhostShipCapturedWithCoinCostRTOs.GroupBy(i => i.ReplayID)
                        .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner)
                        .ToArray();
                    var winRateWithFirstTurnIn = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First Ghost Ship Capture",
                        GamesPlayed = firstTurnInRTOs.Length,
                        Value = firstTurnInRTOs.Length > 0
                            ? (decimal)firstTurnInRTOs.Count(i => i) / firstTurnInRTOs.Length
                            : 0,
                    };

                    var firstTwoTurnInRTOs = blackheartsBayGhostShipCapturedWithCoinCostRTOs
                        .GroupBy(i => i.ReplayID)
                        .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Take(2).ToArray())
                        .Where(i => i.Length == 2).Select(
                            i => new
                            {
                                FirstTwoTurnInWin = i.All(j => j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                FirstTwoTurnInLoss = i.All(j => !j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                            }).ToArray();
                    var firstTwoTurnInsGamesPlayed =
                        firstTwoTurnInRTOs.Sum(i => i.FirstTwoTurnInWin + i.FirstTwoTurnInLoss);
                    var winRateWithFirstTwoTurnIns = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First and Second Ghost Ship Capture",
                        GamesPlayed = firstTwoTurnInsGamesPlayed,
                        Value = firstTwoTurnInsGamesPlayed > 0
                            ? (decimal)firstTwoTurnInRTOs.Sum(i => i.FirstTwoTurnInWin) /
                              firstTwoTurnInsGamesPlayed
                            : 0,
                    };

                    var gamesThatEndWithObjective = blackheartsBayGhostShipCapturedWithCoinCostRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.Any(j => j.TeamObjectiveRTO.IsWinner) && i.Where(j => j.TeamObjectiveRTO.IsWinner)
                                    .OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Last().TeamObjectiveRTO.TimeSpan >
                                replayDictionary[i.Key].ReplayLength.Add(TimeSpan.FromSeconds(-60))).ToArray();
                    var gamesEndedByObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Percent of Games Ended with a Ghost Ship Capture",
                        GamesPlayed = gamesThatEndWithObjective.Count(i => i),
                        Value = gamesThatEndWithObjective.Length > 0
                            ? (decimal)gamesThatEndWithObjective.Count(i => i) /
                              gamesThatEndWithObjective.Length
                            : 0,
                    };

                    var mainMapRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Ghost Ship Captures",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = new List<TeamObjectiveRTOSummaryRadGridRow>
                        {
                            winRateWithFirstTurnIn,
                            winRateWithFirstTwoTurnIns,
                            gamesEndedByObjective,
                        },
                    };

                    var gamesWonAndLostByObjectiveCount = blackheartsBayGhostShipCapturedWithCoinCostRTOs
                        .GroupBy(i => i.TeamObjectiveRTO.IsWinner).ToDictionary(
                            i => i.Key,
                            i => i.GroupBy(j => j.ReplayID).Select(j => j.Count()).GroupBy(j => j)
                                .ToDictionary(j => j.Key, j => j.Count()));

                    if (gamesWonAndLostByObjectiveCount.Count != 2)
                    {
                        break;
                    }

                    // Also count win rate with 0 objectives
                    var rtosWithoutObjectiveWinsAndLosses = replayDictionary.Values
                        .Where(i => i.MapID == map.IdentifierId).Select(
                            i =>
                                new[] { true, false }
                                    .Select(
                                        j =>
                                            i.TeamObjectives.Any(
                                                k =>
                                                    k.IsWinner == j && k.TeamObjectiveType ==
                                                    (int)TeamObjectiveType.BlackheartsBayGhostShipCapturedWithCoinCost)
                                                ? (bool?)null
                                                : j)
                                    .Where(j => j.HasValue)
                                    .ToDictionary(j => j!.Value, j => j!.Value)).ToArray();
                    foreach (var isWinner in gamesWonAndLostByObjectiveCount.Keys)
                    {
                        gamesWonAndLostByObjectiveCount[isWinner][0] =
                            rtosWithoutObjectiveWinsAndLosses.Count(i => i.ContainsKey(isWinner));
                    }

                    var winRateByObjectiveCountRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Ghost Ship Captures in One Game",
                        XValueTitle = "Ghost Ship Captures in One Game",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = gamesWonAndLostByObjectiveCount[true].Select(
                                i => new
                                {
                                    ObjectiveCount = i.Key,
                                    GamesPlayed =
                                        i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                            ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                            : 0),
                                    WinRate = i.Value > 0
                                        ? (decimal)i.Value /
                                          (i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                              ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                              : 0))
                                        : 0m,
                                })
                            .Where(i => i.GamesPlayed > 100 || i.ObjectiveCount == 0)
                            .OrderBy(i => i.ObjectiveCount).Select(
                                i => new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip =
                                        TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                            i.GamesPlayed,
                                            i.WinRate,
                                            "P1",
                                            "Win Percent"),
                                    XValue = i.ObjectiveCount,
                                    YValue = i.WinRate,
                                }).ToList(),
                    };

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids = new[] { mainMapRadGrid },
                            TeamObjectiveRTOSummaryRadHtmlCharts =
                                new[] { winRateByObjectiveCountRadHtmlChart },
                        },
                        TimeSpan.FromDays(30));
                }
                break;

                case "Cursed Hollow":
                {
                    var cursedHollowTributeCollectedWithTotalTeamTributes = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(
                                    j => j.TeamObjectiveType ==
                                         (int)TeamObjectiveType.BlackheartsBayGhostShipCapturedWithCoinCost)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var firstObjectiveRTOs = cursedHollowTributeCollectedWithTotalTeamTributes
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner)
                        .ToArray();
                    var winRateWithFirstObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First Tribute Collected",
                        GamesPlayed = firstObjectiveRTOs.Length,
                        Value = firstObjectiveRTOs.Length > 0
                            ? (decimal)firstObjectiveRTOs.Count(i => i) / firstObjectiveRTOs.Length
                            : 0,
                    };

                    var firstMainObjectiveRTOs = cursedHollowTributeCollectedWithTotalTeamTributes
                        .Where(i => i.TeamObjectiveRTO.Value % 3 == 0).GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner)
                        .ToArray();
                    var winRateWithFirstMainObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First Raven Lord Curse Collected",
                        GamesPlayed = firstMainObjectiveRTOs.Length,
                        Value = firstMainObjectiveRTOs.Length > 0
                            ? (decimal)firstMainObjectiveRTOs.Count(i => i) / firstMainObjectiveRTOs.Length
                            : 0,
                    };

                    var gamesThatEndWithObjective = cursedHollowTributeCollectedWithTotalTeamTributes
                        .Where(i => i.TeamObjectiveRTO.Value % 3 == 0).GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.Any(j => j.TeamObjectiveRTO.IsWinner) && i.Where(j => j.TeamObjectiveRTO.IsWinner)
                                    .OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Last().TeamObjectiveRTO.TimeSpan >
                                replayDictionary[i.Key].ReplayLength.Add(TimeSpan.FromSeconds(-120))).ToArray();
                    var gamesEndedByObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Percent of Games Ended with a Raven Lord Curse",
                        GamesPlayed = gamesThatEndWithObjective.Count(i => i),
                        Value = gamesThatEndWithObjective.Length > 0
                            ? (decimal)gamesThatEndWithObjective.Count(i => i) /
                              gamesThatEndWithObjective.Length
                            : 0,
                    };

                    var mainMapRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Raven Lord Curse",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = new List<TeamObjectiveRTOSummaryRadGridRow>
                        {
                            winRateWithFirstObjective,
                            winRateWithFirstMainObjective,
                            gamesEndedByObjective,
                        },
                    };

                    var gamesWonAndLostByObjectiveCount = cursedHollowTributeCollectedWithTotalTeamTributes
                        .GroupBy(i => i.TeamObjectiveRTO.IsWinner).ToDictionary(
                            i => i.Key,
                            i => i.GroupBy(j => j.ReplayID).Select(j => j.Count()).GroupBy(j => j)
                                .ToDictionary(j => j.Key, j => j.Count()));

                    if (gamesWonAndLostByObjectiveCount.Count != 2)
                    {
                        break;
                    }

                    // Also count win rate with 0 objectives
                    var rtosWithoutObjectiveWinsAndLosses = replayDictionary.Values
                        .Where(i => i.MapID == map.IdentifierId).Select(
                            i =>
                                new[] { true, false }
                                    .Select(
                                        j =>
                                            i.TeamObjectives.Any(
                                                k =>
                                                    k.IsWinner == j && k.TeamObjectiveType == (int)TeamObjectiveType
                                                        .CursedHollowTributeCollectedWithTotalTeamTributes)
                                                ? (bool?)null
                                                : j)
                                    .Where(j => j.HasValue)
                                    .ToDictionary(j => j!.Value, j => j!.Value)).ToArray();
                    foreach (var isWinner in gamesWonAndLostByObjectiveCount.Keys)
                    {
                        gamesWonAndLostByObjectiveCount[isWinner][0] =
                            rtosWithoutObjectiveWinsAndLosses.Count(i => i.ContainsKey(isWinner));
                    }

                    var winRateByObjectiveCountRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Tributes Collected in One Game",
                        XValueTitle = "Tributes Collected in One Game",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = gamesWonAndLostByObjectiveCount[true].Select(
                                i => new
                                {
                                    ObjectiveCount = i.Key,
                                    GamesPlayed =
                                        i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                            ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                            : 0),
                                    WinRate = i.Value > 0
                                        ? (decimal)i.Value /
                                          (i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                              ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                              : 0))
                                        : 0m,
                                })
                            .Where(i => i.GamesPlayed > 100 || i.ObjectiveCount == 0)
                            .OrderBy(i => i.ObjectiveCount).Select(
                                i => new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip =
                                        TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                            i.GamesPlayed,
                                            i.WinRate,
                                            "P1",
                                            "Win Percent"),
                                    XValue = i.ObjectiveCount,
                                    YValue = i.WinRate,
                                }).ToList(),
                    };

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids = new[] { mainMapRadGrid },
                            TeamObjectiveRTOSummaryRadHtmlCharts =
                                new[] { winRateByObjectiveCountRadHtmlChart },
                        },
                        TimeSpan.FromDays(30));
                }
                break;

                case "Dragon Shire":
                {
                    var dragonShireDragonKnightActivatedWithDragonDurationSecondsRTOs = replayDictionary
                        .SelectMany(
                            i =>
                                i.Value.TeamObjectives
                                    .Where(
                                        j => j.TeamObjectiveType == (int)TeamObjectiveType
                                            .DragonShireDragonKnightActivatedWithDragonDurationSeconds)
                                    .Select(
                                        j => new
                                        {
                                            ReplayID = i.Key,
                                            TeamObjectiveRTO = j,
                                        })).ToArray();

                    var firstObjectiveRTOs = dragonShireDragonKnightActivatedWithDragonDurationSecondsRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner)
                        .ToArray();
                    var winRateWithFirstObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First Dragon Knight",
                        GamesPlayed = firstObjectiveRTOs.Length,
                        Value = firstObjectiveRTOs.Length > 0
                            ? (decimal)firstObjectiveRTOs.Count(i => i) / firstObjectiveRTOs.Length
                            : 0,
                    };

                    var firstTwoObjectiveRTOs = dragonShireDragonKnightActivatedWithDragonDurationSecondsRTOs
                        .GroupBy(i => i.ReplayID)
                        .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Take(2).ToArray())
                        .Where(i => i.Length == 2).Select(
                            i => new
                            {
                                FirstTwoObjectiveWin = i.All(j => j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                FirstTwoObjectiveLoss = i.All(j => !j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                            }).ToArray();
                    var firstTwoObjectiveGamesPlayed =
                        firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin + i.FirstTwoObjectiveLoss);
                    var winRateWithFirstTwoObjectives = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First and Second Dragon Knight",
                        GamesPlayed = firstTwoObjectiveGamesPlayed,
                        Value = firstTwoObjectiveGamesPlayed > 0
                            ? (decimal)firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin) /
                              firstTwoObjectiveGamesPlayed
                            : 0,
                    };

                    var gamesThatEndWithObjective = dragonShireDragonKnightActivatedWithDragonDurationSecondsRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.Any(j => j.TeamObjectiveRTO.IsWinner) && i.Where(j => j.TeamObjectiveRTO.IsWinner)
                                    .OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Last().TeamObjectiveRTO.TimeSpan >
                                replayDictionary[i.Key].ReplayLength.Add(TimeSpan.FromSeconds(-90))).ToArray();
                    var gamesEndedByObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Percent of Games Ended with a Dragon Knight",
                        GamesPlayed = gamesThatEndWithObjective.Count(i => i),
                        Value = gamesThatEndWithObjective.Length > 0
                            ? (decimal)gamesThatEndWithObjective.Count(i => i) /
                              gamesThatEndWithObjective.Length
                            : 0,
                    };

                    var mainMapRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Dragon Knight Captures",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = new List<TeamObjectiveRTOSummaryRadGridRow>
                        {
                            winRateWithFirstObjective,
                            winRateWithFirstTwoObjectives,
                            gamesEndedByObjective,
                        },
                    };

                    var gamesWonAndLostByObjectiveCount =
                        dragonShireDragonKnightActivatedWithDragonDurationSecondsRTOs
                            .GroupBy(i => i.TeamObjectiveRTO.IsWinner).ToDictionary(
                                i => i.Key,
                                i => i.GroupBy(j => j.ReplayID).Select(j => j.Count()).GroupBy(j => j)
                                    .ToDictionary(j => j.Key, j => j.Count()));

                    if (gamesWonAndLostByObjectiveCount.Count != 2)
                    {
                        break;
                    }

                    // Also count win rate with 0 objectives
                    var rtosWithoutObjectiveWinsAndLosses = replayDictionary.Values
                        .Where(i => i.MapID == map.IdentifierId).Select(
                            i =>
                                new[] { true, false }
                                    .Select(
                                        j =>
                                            i.TeamObjectives.Any(
                                                k =>
                                                    k.IsWinner == j && k.TeamObjectiveType == (int)TeamObjectiveType
                                                        .DragonShireDragonKnightActivatedWithDragonDurationSeconds)
                                                ? (bool?)null
                                                : j)
                                    .Where(j => j.HasValue)
                                    .ToDictionary(j => j!.Value, j => j!.Value)).ToArray();
                    foreach (var isWinner in gamesWonAndLostByObjectiveCount.Keys)
                    {
                        gamesWonAndLostByObjectiveCount[isWinner][0] =
                            rtosWithoutObjectiveWinsAndLosses.Count(i => i.ContainsKey(isWinner));
                    }

                    var winRateByObjectiveCountRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Dragon Knight Captures in One Game",
                        XValueTitle = "Dragon Knight Captures in One Game",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = gamesWonAndLostByObjectiveCount[true].Select(
                                i => new
                                {
                                    ObjectiveCount = i.Key,
                                    GamesPlayed =
                                        i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                            ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                            : 0),
                                    WinRate = i.Value > 0
                                        ? (decimal)i.Value /
                                          (i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                              ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                              : 0))
                                        : 0m,
                                })
                            .Where(i => i.GamesPlayed > 100 || i.ObjectiveCount == 0)
                            .OrderBy(i => i.ObjectiveCount).Select(
                                i => new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip =
                                        TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                            i.GamesPlayed,
                                            i.WinRate,
                                            "P1",
                                            "Win Percent"),
                                    XValue = i.ObjectiveCount,
                                    YValue = i.WinRate,
                                }).ToList(),
                    };

                    var winPercentByObjectiveValue = dragonShireDragonKnightActivatedWithDragonDurationSecondsRTOs
                        .GroupBy(i => i.TeamObjectiveRTO.Value / 5 * 5).Where(i => i.Count() > 100).Select(
                            i =>
                                new
                                {
                                    ObjectiveValue = i.Key,
                                    GamesPlayed = i.Count(),
                                    WinPercent = (decimal)i.Count(j => j.TeamObjectiveRTO.IsWinner) / i.Count(),
                                }).OrderBy(i => i.ObjectiveValue).ToArray();

                    var winRateByObjectiveValueRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Dragon Knight Duration",
                        XValueTitle = "Dragon Knight Duration in Seconds",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = winPercentByObjectiveValue.Select(
                            i =>
                                new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip = TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                        i.GamesPlayed,
                                        i.WinPercent,
                                        "P1",
                                        "Win Percent"),
                                    XValue = i.ObjectiveValue,
                                    YValue = i.WinPercent,
                                }).ToList(),
                    };

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids = new[] { mainMapRadGrid },
                            TeamObjectiveRTOSummaryRadHtmlCharts = new[]
                            {
                                winRateByObjectiveCountRadHtmlChart, winRateByObjectiveValueRadHtmlChart,
                            },
                        },
                        TimeSpan.FromDays(30));
                }
                break;

                case "Garden of Terror":
                {
                    var gardenOfTerrorGardenTerrorActivatedWithGardenTerrorDurationSecondsRTOs = replayDictionary
                        .SelectMany(
                            i =>
                                i.Value.TeamObjectives
                                    .Where(
                                        j => j.TeamObjectiveType == (int)TeamObjectiveType
                                            .GardenOfTerrorGardenTerrorActivatedWithGardenTerrorDurationSeconds)
                                    .Select(
                                        j => new
                                        {
                                            ReplayID = i.Key,
                                            TeamObjectiveRTO = j,
                                        })).ToArray();

                    var firstObjectiveRTOs = gardenOfTerrorGardenTerrorActivatedWithGardenTerrorDurationSecondsRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner)
                        .ToArray();
                    var winRateWithFirstObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First Garden Terror",
                        GamesPlayed = firstObjectiveRTOs.Length,
                        Value = firstObjectiveRTOs.Length > 0
                            ? (decimal)firstObjectiveRTOs.Count(i => i) / firstObjectiveRTOs.Length
                            : 0,
                    };

                    var firstTwoObjectiveRTOs =
                        gardenOfTerrorGardenTerrorActivatedWithGardenTerrorDurationSecondsRTOs
                            .GroupBy(i => i.ReplayID)
                            .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Take(2).ToArray())
                            .Where(i => i.Length == 2).Select(
                                i => new
                                {
                                    FirstTwoObjectiveWin = i.All(j => j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                    FirstTwoObjectiveLoss = i.All(j => !j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                }).ToArray();
                    var firstTwoObjectiveGamesPlayed =
                        firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin + i.FirstTwoObjectiveLoss);
                    var winRateWithFirstTwoObjectives = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First and Second Garden Terror",
                        GamesPlayed = firstTwoObjectiveGamesPlayed,
                        Value = firstTwoObjectiveGamesPlayed > 0
                            ? (decimal)firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin) /
                              firstTwoObjectiveGamesPlayed
                            : 0,
                    };

                    var gamesThatEndWithObjective =
                        gardenOfTerrorGardenTerrorActivatedWithGardenTerrorDurationSecondsRTOs
                            .GroupBy(i => i.ReplayID).Select(
                                i =>
                                    i.Any(j => j.TeamObjectiveRTO.IsWinner) && i.Where(j => j.TeamObjectiveRTO.IsWinner)
                                        .OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Last().TeamObjectiveRTO
                                        .TimeSpan > replayDictionary[i.Key].ReplayLength
                                        .Add(TimeSpan.FromSeconds(-120))).ToArray();
                    var gamesEndedByObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Percent of Games Ended with a Garden Terror",
                        GamesPlayed = gamesThatEndWithObjective.Count(i => i),
                        Value = gamesThatEndWithObjective.Length > 0
                            ? (decimal)gamesThatEndWithObjective.Count(i => i) /
                              gamesThatEndWithObjective.Length
                            : 0,
                    };

                    var mainMapRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Garden Terror Captures",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = new List<TeamObjectiveRTOSummaryRadGridRow>
                        {
                            winRateWithFirstObjective,
                            winRateWithFirstTwoObjectives,
                            gamesEndedByObjective,
                        },
                    };

                    var gamesWonAndLostByObjectiveCount =
                        gardenOfTerrorGardenTerrorActivatedWithGardenTerrorDurationSecondsRTOs
                            .GroupBy(i => i.TeamObjectiveRTO.IsWinner).ToDictionary(
                                i => i.Key,
                                i => i.GroupBy(j => j.ReplayID).Select(j => j.Count()).GroupBy(j => j)
                                    .ToDictionary(j => j.Key, j => j.Count()));

                    if (gamesWonAndLostByObjectiveCount.Count != 2)
                    {
                        break;
                    }

                    // Also count win rate with 0 objectives
                    var rtosWithoutObjectiveWinsAndLosses = replayDictionary.Values
                        .Where(i => i.MapID == map.IdentifierId).Select(
                            i =>
                                new[] { true, false }.Select(
                                    j =>
                                        i.TeamObjectives.Any(
                                            k =>
                                                k.IsWinner == j && k.TeamObjectiveType == (int)TeamObjectiveType
                                                    .GardenOfTerrorGardenTerrorActivatedWithGardenTerrorDurationSeconds)
                                            ? (bool?)null
                                            : j)
                                    .Where(j => j.HasValue)
                                    .ToDictionary(j => j!.Value, j => j!.Value))
                        .ToArray();
                    foreach (var isWinner in gamesWonAndLostByObjectiveCount.Keys)
                    {
                        gamesWonAndLostByObjectiveCount[isWinner][0] =
                            rtosWithoutObjectiveWinsAndLosses.Count(i => i.ContainsKey(isWinner));
                    }

                    var winRateByObjectiveCountRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Garden Terror Captures in One Game",
                        XValueTitle = "Garden Terror Captures in One Game",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = gamesWonAndLostByObjectiveCount[true].Select(
                                i => new
                                {
                                    ObjectiveCount = i.Key,
                                    GamesPlayed =
                                        i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                            ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                            : 0),
                                    WinRate = i.Value > 0
                                        ? (decimal)i.Value /
                                          (i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                              ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                              : 0))
                                        : 0m,
                                })
                            .Where(i => i.GamesPlayed > 100 || i.ObjectiveCount == 0)
                            .OrderBy(i => i.ObjectiveCount).Select(
                                i => new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip =
                                        TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                            i.GamesPlayed,
                                            i.WinRate,
                                            "P1",
                                            "Win Percent"),
                                    XValue = i.ObjectiveCount,
                                    YValue = i.WinRate,
                                }).ToList(),
                    };

                    var winPercentByObjectiveValue =
                        gardenOfTerrorGardenTerrorActivatedWithGardenTerrorDurationSecondsRTOs
                            .GroupBy(i => i.TeamObjectiveRTO.Value / 5 * 5).Where(i => i.Count() > 100).Select(
                                i =>
                                    new
                                    {
                                        ObjectiveValue = i.Key,
                                        GamesPlayed = i.Count(),
                                        WinPercent = (decimal)i.Count(j => j.TeamObjectiveRTO.IsWinner) / i.Count(),
                                    }).OrderBy(i => i.ObjectiveValue).ToArray();

                    var winRateByObjectiveValueRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Garden Terror Duration",
                        XValueTitle = "Garden Terror Duration in Seconds",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = winPercentByObjectiveValue.Select(
                            i =>
                                new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip = TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                        i.GamesPlayed,
                                        i.WinPercent,
                                        "P1",
                                        "Win Percent"),
                                    XValue = i.ObjectiveValue,
                                    YValue = i.WinPercent,
                                }).ToList(),
                    };

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids = new[] { mainMapRadGrid },
                            TeamObjectiveRTOSummaryRadHtmlCharts = new[]
                            {
                                winRateByObjectiveCountRadHtmlChart, winRateByObjectiveValueRadHtmlChart,
                            },
                        },
                        TimeSpan.FromDays(30));
                }
                break;

                case "Infernal Shrines":
                {
                    var infernalShrinesInfernalShrineCapturedWithLosingScoreRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(
                                    j => j.TeamObjectiveType ==
                                         (int)TeamObjectiveType.InfernalShrinesInfernalShrineCapturedWithLosingScore)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var firstObjectiveRTOs = infernalShrinesInfernalShrineCapturedWithLosingScoreRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner)
                        .ToArray();
                    var winRateWithFirstObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First Punisher Summoned",
                        GamesPlayed = firstObjectiveRTOs.Length,
                        Value = firstObjectiveRTOs.Length > 0
                            ? (decimal)firstObjectiveRTOs.Count(i => i) / firstObjectiveRTOs.Length
                            : 0,
                    };

                    var firstTwoObjectiveRTOs = infernalShrinesInfernalShrineCapturedWithLosingScoreRTOs
                        .GroupBy(i => i.ReplayID)
                        .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Take(2).ToArray())
                        .Where(i => i.Length == 2).Select(
                            i => new
                            {
                                FirstTwoObjectiveWin = i.All(j => j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                FirstTwoObjectiveLoss = i.All(j => !j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                            }).ToArray();
                    var firstTwoObjectiveGamesPlayed =
                        firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin + i.FirstTwoObjectiveLoss);
                    var winRateWithFirstTwoObjectives = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First and Second Punisher Summoned",
                        GamesPlayed = firstTwoObjectiveGamesPlayed,
                        Value = firstTwoObjectiveGamesPlayed > 0
                            ? (decimal)firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin) /
                              firstTwoObjectiveGamesPlayed
                            : 0,
                    };

                    var gamesThatEndWithObjective = infernalShrinesInfernalShrineCapturedWithLosingScoreRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.Any(j => j.TeamObjectiveRTO.IsWinner) && i.Where(j => j.TeamObjectiveRTO.IsWinner)
                                    .OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Last().TeamObjectiveRTO.TimeSpan >
                                replayDictionary[i.Key].ReplayLength.Add(TimeSpan.FromSeconds(-90))).ToArray();
                    var gamesEndedByObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Percent of Games Ended with a Punisher",
                        GamesPlayed = gamesThatEndWithObjective.Count(i => i),
                        Value = gamesThatEndWithObjective.Length > 0
                            ? (decimal)gamesThatEndWithObjective.Count(i => i) /
                              gamesThatEndWithObjective.Length
                            : 0,
                    };

                    var mainMapRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Punishers Summoned",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = new List<TeamObjectiveRTOSummaryRadGridRow>
                        {
                            winRateWithFirstObjective,
                            winRateWithFirstTwoObjectives,
                            gamesEndedByObjective,
                        },
                    };

                    var gamesWonAndLostByObjectiveCount = infernalShrinesInfernalShrineCapturedWithLosingScoreRTOs
                        .GroupBy(i => i.TeamObjectiveRTO.IsWinner).ToDictionary(
                            i => i.Key,
                            i => i.GroupBy(j => j.ReplayID).Select(j => j.Count()).GroupBy(j => j)
                                .ToDictionary(j => j.Key, j => j.Count()));

                    if (gamesWonAndLostByObjectiveCount.Count != 2)
                    {
                        break;
                    }

                    // Also count win rate with 0 objectives
                    var rtosWithoutObjectiveWinsAndLosses = replayDictionary.Values
                        .Where(i => i.MapID == map.IdentifierId).Select(
                            i =>
                                new[] { true, false }
                                    .Select(
                                        j =>
                                            i.TeamObjectives.Any(
                                                k =>
                                                    k.IsWinner == j && k.TeamObjectiveType == (int)TeamObjectiveType
                                                        .InfernalShrinesInfernalShrineCapturedWithLosingScore)
                                                ? (bool?)null
                                                : j)
                                    .Where(j => j.HasValue)
                                    .ToDictionary(j => j!.Value, j => j!.Value)).ToArray();
                    foreach (var isWinner in gamesWonAndLostByObjectiveCount.Keys)
                    {
                        gamesWonAndLostByObjectiveCount[isWinner][0] =
                            rtosWithoutObjectiveWinsAndLosses.Count(i => i.ContainsKey(isWinner));
                    }

                    var winRateByObjectiveCountRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Punishers Summoned in One Game",
                        XValueTitle = "Punishers Summoned in One Game",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = gamesWonAndLostByObjectiveCount[true].Select(
                                i => new
                                {
                                    ObjectiveCount = i.Key,
                                    GamesPlayed =
                                        i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                            ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                            : 0),
                                    WinRate = i.Value > 0
                                        ? (decimal)i.Value /
                                          (i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                              ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                              : 0))
                                        : 0m,
                                })
                            .Where(i => i.GamesPlayed > 100 || i.ObjectiveCount == 0)
                            .OrderBy(i => i.ObjectiveCount).Select(
                                i => new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip =
                                        TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                            i.GamesPlayed,
                                            i.WinRate,
                                            "P1",
                                            "Win Percent"),
                                    XValue = i.ObjectiveCount,
                                    YValue = i.WinRate,
                                }).ToList(),
                    };

                    // Too confusing to explain to users, and users are probably more interested in punisher type related statistics anyway
                    /* var winPercentByObjectiveValue = infernalShrinesInfernalShrineCapturedWithLosingScoreRTOs.GroupBy(i => i.TeamObjectiveRTO.Value).Select(i => new { ObjectiveValue = 40 - i.Key, GamesPlayed = i.Count(), WinPercent = (decimal) i.Count(j => j.TeamObjectiveRTO.IsWinner) / i.Count() }).Where(i => i.GamesPlayed > 100).OrderBy(i => i.ObjectiveValue).ToArray();

                    var winRateByObjectiveValueRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart {
                        RadHtmlChartTitle = "Win Rate by Punisher Guardian Kill Score Over Losing Team",
                        XValueTitle = "Winning Punisher Guardian Kill Score Lead",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = winPercentByObjectiveValue.Select(i => new TeamObjectiveRTOSummaryRadHtmlChartItem {
                            Tooltip = TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(i.GamesPlayed, i.WinPercent, "P1", "Win Percent"),
                            XValue = i.ObjectiveValue,
                            YValue = i.WinPercent }).ToList() }; */

                    var infernalShrinesPunisherKilledWithPunisherTypeRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(
                                    j => j.TeamObjectiveType ==
                                         (int)TeamObjectiveType.InfernalShrinesPunisherKilledWithPunisherType)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var winRatePerPunisherTypeRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Win Rate by Punisher Type",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = infernalShrinesPunisherKilledWithPunisherTypeRTOs
                            .GroupBy(i => i.TeamObjectiveRTO.Value).Select(
                                i =>
                                    new TeamObjectiveRTOSummaryRadGridRow
                                    {
                                        RowTitle =
                                            ((TeamObjectiveInfernalShrinesPunisherType)i.Key)
                                            .GetTeamObjectiveInfernalShrinesPunisherTypeString(),
                                        GamesPlayed = i.Count(),
                                        Value = i.Any()
                                            ? (decimal)i.Count(j => j.TeamObjectiveRTO.IsWinner) / i.Count()
                                            : 0m,
                                    }).OrderByDescending(i => i.Value).ToList(),
                    };

                    var infernalShrinesPunisherKilledWithTypeAndDamageDoneRTOs = replayDictionary
                        .SelectMany(
                            i => i.Value.TeamObjectives.Where(
                                    j =>
                                        j.TeamObjectiveType ==
                                        (int)TeamObjectiveType.InfernalShrinesPunisherKilledWithPunisherType ||
                                        j.TeamObjectiveType ==
                                        (int)TeamObjectiveType.InfernalShrinesPunisherKilledWithSiegeDamageDone ||
                                        j.TeamObjectiveType ==
                                        (int)TeamObjectiveType.InfernalShrinesPunisherKilledWithHeroDamageDone)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).GroupBy(
                            i => new
                            {
                                i.ReplayID,
                                i.TeamObjectiveRTO.TimeSpan,
                            }).Where(i => i.Count() == 3).Select(
                            i => new
                            {
                                i.Key.ReplayID,
                                PunisherType =
                                    i.Single(
                                            j =>
                                                j.TeamObjectiveRTO.TeamObjectiveType ==
                                                (int)TeamObjectiveType.InfernalShrinesPunisherKilledWithPunisherType)
                                        .TeamObjectiveRTO.Value,
                                SiegeDamage =
                                    i.Single(
                                            j =>
                                                j.TeamObjectiveRTO.TeamObjectiveType == (int)TeamObjectiveType
                                                    .InfernalShrinesPunisherKilledWithSiegeDamageDone).TeamObjectiveRTO
                                        .Value,
                                HeroDamage = i.Single(
                                        j =>
                                            j.TeamObjectiveRTO.TeamObjectiveType ==
                                            (int)TeamObjectiveType.InfernalShrinesPunisherKilledWithHeroDamageDone)
                                    .TeamObjectiveRTO
                                    .Value,
                            }).ToArray();

                    var averageSiegeDamagePerPunisherTypeRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Average Siege Damage by Punisher Type",
                        ValueColumnHeaderText = "Average Siege Damage",
                        ValueFormatString = "N0",
                        RadGridRows = infernalShrinesPunisherKilledWithTypeAndDamageDoneRTOs
                            .GroupBy(i => i.PunisherType).Select(
                                i => new TeamObjectiveRTOSummaryRadGridRow
                                {
                                    RowTitle = ((TeamObjectiveInfernalShrinesPunisherType)i.Key)
                                        .GetTeamObjectiveInfernalShrinesPunisherTypeString(),
                                    GamesPlayed = i.Count(),
                                    Value = (int)i.Average(j => j.SiegeDamage),
                                }).OrderByDescending(i => i.Value).ToList(),
                    };

                    var averageHeroDamagePerPunisherTypeRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Average Hero Damage by Punisher Type",
                        ValueColumnHeaderText = "Average Hero Damage",
                        ValueFormatString = "N0",
                        RadGridRows = infernalShrinesPunisherKilledWithTypeAndDamageDoneRTOs
                            .GroupBy(i => i.PunisherType).Select(
                                i => new TeamObjectiveRTOSummaryRadGridRow
                                {
                                    RowTitle = ((TeamObjectiveInfernalShrinesPunisherType)i.Key)
                                        .GetTeamObjectiveInfernalShrinesPunisherTypeString(),
                                    GamesPlayed = i.Count(),
                                    Value = (int)i.Average(j => j.HeroDamage),
                                }).OrderByDescending(i => i.Value).ToList(),
                    };

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids =
                                new[]
                                {
                                    mainMapRadGrid,
                                    winRatePerPunisherTypeRadGrid,
                                    averageSiegeDamagePerPunisherTypeRadGrid,
                                    averageHeroDamagePerPunisherTypeRadGrid,
                                },
                            TeamObjectiveRTOSummaryRadHtmlCharts =
                                new[] { winRateByObjectiveCountRadHtmlChart },
                        },
                        TimeSpan.FromDays(30));
                }
                break;

                case "Sky Temple":
                {
                    var skyTempleShotsFiredWithSkyTempleShotsDamageRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(
                                    j => j.TeamObjectiveType ==
                                         (int)TeamObjectiveType.SkyTempleShotsFiredWithSkyTempleShotsDamage)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var firstObjectiveRTOs = skyTempleShotsFiredWithSkyTempleShotsDamageRTOs
                        .GroupBy(i => i.ReplayID).Where(i => i.Count() >= 3)
                        .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Take(3))
                        .Where(i => i.All(j => j.TeamObjectiveRTO.Value >= 18000))
                        .Select(i => i.Last().TeamObjectiveRTO.IsWinner).ToArray();
                    var winRateWithFirstObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate when Capturing 2nd Temple Phase (Bottom Temple)",
                        GamesPlayed = firstObjectiveRTOs.Length,
                        Value = firstObjectiveRTOs.Length > 0
                            ? (decimal)firstObjectiveRTOs.Count(i => i) / firstObjectiveRTOs.Length
                            : 0,
                    };

                    var firstTwoObjectiveRTOs = skyTempleShotsFiredWithSkyTempleShotsDamageRTOs
                        .GroupBy(i => i.ReplayID)
                        .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Take(2).ToArray())
                        .Where(i => i.Length == 2 && i.All(j => j.TeamObjectiveRTO.Value >= 18000)).Select(
                            i =>
                                new
                                {
                                    FirstTwoObjectiveWin = i.All(j => j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                    FirstTwoObjectiveLoss = i.All(j => !j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                }).ToArray();
                    var firstTwoObjectiveGamesPlayed =
                        firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin + i.FirstTwoObjectiveLoss);
                    var winRateWithFirstTwoObjectives = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate when Capturing Both Temples in 1st Temple Phase",
                        GamesPlayed = firstTwoObjectiveGamesPlayed,
                        Value = firstTwoObjectiveGamesPlayed > 0
                            ? (decimal)firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin) /
                              firstTwoObjectiveGamesPlayed
                            : 0,
                    };

                    var gamesThatEndWithObjective = skyTempleShotsFiredWithSkyTempleShotsDamageRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.Any(j => j.TeamObjectiveRTO.IsWinner) && i.Where(j => j.TeamObjectiveRTO.IsWinner)
                                    .OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Last().TeamObjectiveRTO.TimeSpan >
                                replayDictionary[i.Key].ReplayLength.Add(TimeSpan.FromSeconds(-90))).ToArray();
                    var gamesEndedByObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Percent of Games Ended with Temple Shots",
                        GamesPlayed = gamesThatEndWithObjective.Count(i => i),
                        Value = gamesThatEndWithObjective.Length > 0
                            ? (decimal)gamesThatEndWithObjective.Count(i => i) /
                              gamesThatEndWithObjective.Length
                            : 0,
                    };

                    var mainMapRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Temples Captured",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = new List<TeamObjectiveRTOSummaryRadGridRow>
                        {
                            winRateWithFirstObjective,
                            winRateWithFirstTwoObjectives,
                            gamesEndedByObjective,
                        },
                    };

                    var gamesWonAndLostByObjectiveCount = skyTempleShotsFiredWithSkyTempleShotsDamageRTOs
                        .GroupBy(i => i.TeamObjectiveRTO.IsWinner).ToDictionary(
                            i => i.Key,
                            i => i.GroupBy(j => j.ReplayID)
                                .Select(j => j.Sum(k => k.TeamObjectiveRTO.Value) / 10000 * 10000).GroupBy(j => j)
                                .ToDictionary(j => j.Key, j => j.Count()));

                    if (gamesWonAndLostByObjectiveCount.Count != 2)
                    {
                        break;
                    }

                    var winRateByObjectiveCountRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Total Temple Shot Damage in One Game",
                        XValueTitle = "Total Temple Shot Damage in One Game",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = gamesWonAndLostByObjectiveCount[true].Select(
                                i => new
                                {
                                    ObjectiveCount = i.Key,
                                    GamesPlayed =
                                        i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                            ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                            : 0),
                                    WinRate = i.Value > 0
                                        ? (decimal)i.Value /
                                          (i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                              ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                              : 0))
                                        : 0m,
                                })
                            .Where(i => i.GamesPlayed > 100 || i.ObjectiveCount == 0)
                            .OrderBy(i => i.ObjectiveCount).Select(
                                i => new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip =
                                        TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                            i.GamesPlayed,
                                            i.WinRate,
                                            "P1",
                                            "Win Percent"),
                                    XValue = i.ObjectiveCount,
                                    YValue = i.WinRate,
                                }).ToList(),
                    };

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids = new[] { mainMapRadGrid },
                            TeamObjectiveRTOSummaryRadHtmlCharts =
                                new[] { winRateByObjectiveCountRadHtmlChart },
                        },
                        TimeSpan.FromDays(30));
                }
                break;

                case "Tomb of the Spider Queen":
                {
                    var tombOfTheSpiderQueenSoulEatersSpawnedWithTeamScoreRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(
                                    j => j.TeamObjectiveType ==
                                         (int)TeamObjectiveType.TombOfTheSpiderQueenSoulEatersSpawnedWithTeamScore)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var firstObjectiveRTOs = tombOfTheSpiderQueenSoulEatersSpawnedWithTeamScoreRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner)
                        .ToArray();
                    var winRateWithFirstObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First Webweavers Spawned",
                        GamesPlayed = firstObjectiveRTOs.Length,
                        Value = firstObjectiveRTOs.Length > 0
                            ? (decimal)firstObjectiveRTOs.Count(i => i) / firstObjectiveRTOs.Length
                            : 0,
                    };

                    var firstTwoObjectiveRTOs = tombOfTheSpiderQueenSoulEatersSpawnedWithTeamScoreRTOs
                        .GroupBy(i => i.ReplayID)
                        .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Take(2).ToArray())
                        .Where(i => i.Length == 2).Select(
                            i => new
                            {
                                FirstTwoObjectiveWin = i.All(j => j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                FirstTwoObjectiveLoss = i.All(j => !j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                            }).ToArray();
                    var firstTwoObjectiveGamesPlayed =
                        firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin + i.FirstTwoObjectiveLoss);
                    var winRateWithFirstTwoObjectives = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First and Second Webweavers Spawned",
                        GamesPlayed = firstTwoObjectiveGamesPlayed,
                        Value = firstTwoObjectiveGamesPlayed > 0
                            ? (decimal)firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin) /
                              firstTwoObjectiveGamesPlayed
                            : 0,
                    };

                    var gamesThatEndWithObjective = tombOfTheSpiderQueenSoulEatersSpawnedWithTeamScoreRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.Any(j => j.TeamObjectiveRTO.IsWinner) && i.Where(j => j.TeamObjectiveRTO.IsWinner)
                                    .OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Last().TeamObjectiveRTO.TimeSpan >
                                replayDictionary[i.Key].ReplayLength.Add(TimeSpan.FromSeconds(-120))).ToArray();
                    var gamesEndedByObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Percent of Games Ended with a Webweaver Push",
                        GamesPlayed = gamesThatEndWithObjective.Count(i => i),
                        Value = gamesThatEndWithObjective.Length > 0
                            ? (decimal)gamesThatEndWithObjective.Count(i => i) /
                              gamesThatEndWithObjective.Length
                            : 0,
                    };

                    var mainMapRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Webweavers Spawned",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = new List<TeamObjectiveRTOSummaryRadGridRow>
                        {
                            winRateWithFirstObjective,
                            winRateWithFirstTwoObjectives,
                            gamesEndedByObjective,
                        },
                    };

                    var gamesWonAndLostByObjectiveCount = tombOfTheSpiderQueenSoulEatersSpawnedWithTeamScoreRTOs
                        .GroupBy(i => i.TeamObjectiveRTO.IsWinner).ToDictionary(
                            i => i.Key,
                            i => i.GroupBy(j => j.ReplayID).Select(j => j.Count()).GroupBy(j => j)
                                .ToDictionary(j => j.Key, j => j.Count()));

                    if (gamesWonAndLostByObjectiveCount.Count != 2)
                    {
                        break;
                    }

                    // Also count win rate with 0 objectives
                    var rtosWithoutObjectiveWinsAndLosses = replayDictionary.Values
                        .Where(i => i.MapID == map.IdentifierId).Select(
                            i =>
                                new[] { true, false }
                                    .Select(
                                        j =>
                                            !i.TeamObjectives.Any(
                                                k =>
                                                    k.IsWinner == j && k.TeamObjectiveType == (int)TeamObjectiveType
                                                        .TombOfTheSpiderQueenSoulEatersSpawnedWithTeamScore)
                                                ? j
                                                : (bool?)null)
                                    .Where(j => j.HasValue)
                                    .ToDictionary(j => j!.Value, j => j!.Value)).ToArray();
                    foreach (var isWinner in gamesWonAndLostByObjectiveCount.Keys)
                    {
                        gamesWonAndLostByObjectiveCount[isWinner][0] =
                            rtosWithoutObjectiveWinsAndLosses.Count(i => i.ContainsKey(isWinner));
                    }

                    var winRateByObjectiveCountRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Webweaver Waves Spawned in One Game",
                        XValueTitle = "Webweaver Waves Spawned in One Game",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = gamesWonAndLostByObjectiveCount[true].Select(
                                i => new
                                {
                                    ObjectiveCount = i.Key,
                                    GamesPlayed =
                                        i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                            ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                            : 0),
                                    WinRate = i.Value > 0
                                        ? (decimal)i.Value /
                                          (i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                              ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                              : 0))
                                        : 0m,
                                })
                            .Where(i => i.GamesPlayed > 100 || i.ObjectiveCount == 0)
                            .OrderBy(i => i.ObjectiveCount).Select(
                                i => new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip =
                                        TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                            i.GamesPlayed,
                                            i.WinRate,
                                            "P1",
                                            "Win Percent"),
                                    XValue = i.ObjectiveCount,
                                    YValue = i.WinRate,
                                }).ToList(),
                    };

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids = new[] { mainMapRadGrid },
                            TeamObjectiveRTOSummaryRadHtmlCharts =
                                new[] { winRateByObjectiveCountRadHtmlChart },
                        },
                        TimeSpan.FromDays(30));
                }
                break;

                case "Towers of Doom":
                {
                    var towersOfDoomAltarCapturedWithTeamTownsOwnedRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(
                                    j => j.TeamObjectiveType ==
                                         (int)TeamObjectiveType.TowersOfDoomAltarCapturedWithTeamTownsOwned)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var firstObjectiveRTOs = towersOfDoomAltarCapturedWithTeamTownsOwnedRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner)
                        .ToArray();
                    var winRateWithFirstObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First Altar Captured",
                        GamesPlayed = firstObjectiveRTOs.Length,
                        Value = firstObjectiveRTOs.Length > 0
                            ? (decimal)firstObjectiveRTOs.Count(i => i) / firstObjectiveRTOs.Length
                            : 0,
                    };

                    var firstTwoObjectiveRTOs = towersOfDoomAltarCapturedWithTeamTownsOwnedRTOs
                        .GroupBy(i => i.ReplayID)
                        .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Take(2).ToArray())
                        .Where(i => i.Length == 2).Select(
                            i => new
                            {
                                FirstTwoObjectiveWin = i.All(j => j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                FirstTwoObjectiveLoss = i.All(j => !j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                            }).ToArray();
                    var firstTwoObjectiveGamesPlayed =
                        firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin + i.FirstTwoObjectiveLoss);
                    var winRateWithFirstTwoObjectives = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First and Second Altar Captured",
                        GamesPlayed = firstTwoObjectiveGamesPlayed,
                        Value = firstTwoObjectiveGamesPlayed > 0
                            ? (decimal)firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin) /
                              firstTwoObjectiveGamesPlayed
                            : 0,
                    };

                    var mainMapRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Altar Captures",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = new List<TeamObjectiveRTOSummaryRadGridRow>
                        {
                            winRateWithFirstObjective,
                            winRateWithFirstTwoObjectives,
                        },
                    };

                    var winPercentByObjectiveValue = towersOfDoomAltarCapturedWithTeamTownsOwnedRTOs
                        .GroupBy(i => i.TeamObjectiveRTO.Value).Where(i => i.Count() > 100).Select(
                            i =>
                                new
                                {
                                    ObjectiveValue = i.Key,
                                    GamesPlayed = i.Count(),
                                    WinPercent = (decimal)i.Count(j => j.TeamObjectiveRTO.IsWinner) / i.Count(),
                                }).OrderBy(i => i.ObjectiveValue).ToArray();

                    var winRateByObjectiveValueRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Altar Capture Shots Fired",
                        XValueTitle = "Altar Capture Shots Fired",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = winPercentByObjectiveValue.Select(
                            i =>
                                new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip = TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                        i.GamesPlayed,
                                        i.WinPercent,
                                        "P1",
                                        "Win Percent"),
                                    XValue = i.ObjectiveValue,
                                    YValue = i.WinPercent,
                                }).ToList(),
                    };

                    var towersOfDoomSixTownEventStartWithEventDurationSecondsRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(
                                    j => j.TeamObjectiveType == (int)TeamObjectiveType
                                        .TowersOfDoomSixTownEventStartWithEventDurationSeconds)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var winPercentByObjectiveValue2 = towersOfDoomSixTownEventStartWithEventDurationSecondsRTOs
                        .GroupBy(i => i.TeamObjectiveRTO.Value / 5 * 5).Where(i => i.Count() > 10).Select(
                            i =>
                                new
                                {
                                    ObjectiveValue = i.Key,
                                    GamesPlayed = i.Count(),
                                    WinPercent = (decimal)i.Count(j => j.TeamObjectiveRTO.IsWinner) / i.Count(),
                                }).OrderBy(i => i.ObjectiveValue).ToArray();

                    var winRateByObjectiveValue2RadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Time Span Controlling All Keeps in Seconds",
                        XValueTitle = "Time Span Controlling All Keeps in Seconds",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = winPercentByObjectiveValue2.Select(
                            i =>
                                new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip = TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                        i.GamesPlayed,
                                        i.WinPercent,
                                        "P1",
                                        "Win Percent"),
                                    XValue = i.ObjectiveValue,
                                    YValue = i.WinPercent,
                                }).ToList(),
                    };

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids = new[] { mainMapRadGrid },
                            TeamObjectiveRTOSummaryRadHtmlCharts = new[]
                            {
                                winRateByObjectiveValueRadHtmlChart, winRateByObjectiveValue2RadHtmlChart,
                            },
                        },
                        TimeSpan.FromDays(30));
                }
                break;

                case "Braxis Holdout":
                {
                    var braxisHoldoutZergRushWithLosingZergStrengthRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(
                                    j => j.TeamObjectiveType ==
                                         (int)TeamObjectiveType.BraxisHoldoutZergRushWithLosingZergStrength)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var firstObjectiveRTOs = braxisHoldoutZergRushWithLosingZergStrengthRTOs
                        .GroupBy(i => i.ReplayID).Select(
                            i =>
                                i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner)
                        .ToArray();
                    var winRateWithFirstObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First Zerg Rush",
                        GamesPlayed = firstObjectiveRTOs.Length,
                        Value = firstObjectiveRTOs.Length > 0
                            ? (decimal)firstObjectiveRTOs.Count(i => i) / firstObjectiveRTOs.Length
                            : 0,
                    };

                    var firstTwoObjectiveRTOs = braxisHoldoutZergRushWithLosingZergStrengthRTOs
                        .GroupBy(i => i.ReplayID)
                        .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Take(2).ToArray())
                        .Where(i => i.Length == 2).Select(
                            i => new
                            {
                                FirstTwoObjectiveWin = i.All(j => j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                FirstTwoObjectiveLoss = i.All(j => !j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                            }).ToArray();
                    var firstTwoObjectiveGamesPlayed =
                        firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin + i.FirstTwoObjectiveLoss);
                    var winRateWithFirstTwoObjectives = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First and Second Zerg Rush",
                        GamesPlayed = firstTwoObjectiveGamesPlayed,
                        Value = firstTwoObjectiveGamesPlayed > 0
                            ? (decimal)firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin) /
                              firstTwoObjectiveGamesPlayed
                            : 0,
                    };

                    var mainMapRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Zerg Rushes",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = new List<TeamObjectiveRTOSummaryRadGridRow>
                        {
                            winRateWithFirstObjective,
                            winRateWithFirstTwoObjectives,
                        },
                    };

                    var winPercentByObjectiveValue = braxisHoldoutZergRushWithLosingZergStrengthRTOs
                        .GroupBy(i => (decimal)i.TeamObjectiveRTO.Value / 100).Where(i => i.Count() > 100)
                        .Select(
                            i => new
                            {
                                ObjectiveValue = i.Key,
                                GamesPlayed = i.Count(),
                                WinPercent = (decimal)i.Count(j => j.TeamObjectiveRTO.IsWinner) / i.Count(),
                            }).OrderByDescending(i => i.ObjectiveValue).ToArray();

                    var winRateByObjectiveValueRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Inferior Zerg Rush Strength",
                        XValueTitle = "Inferior Zerg Rush Strength",
                        XValueFormatString = "P1",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = winPercentByObjectiveValue.Select(
                            i =>
                                new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip = TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                        i.GamesPlayed,
                                        i.WinPercent,
                                        "P1",
                                        "Win Percent"),
                                    XValue = i.ObjectiveValue,
                                    YValue = i.WinPercent,
                                }).ToList(),
                    };

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids = new[] { mainMapRadGrid },
                            TeamObjectiveRTOSummaryRadHtmlCharts =
                                new[] { winRateByObjectiveValueRadHtmlChart },
                        },
                        TimeSpan.FromDays(30));
                }
                break;

                case "Warhead Junction":
                {
                    var warheadJunctionNukeLaunchRTOs = replayDictionary.SelectMany(
                        i =>
                            i.Value.TeamObjectives
                                .Where(j => j.TeamObjectiveType == (int)TeamObjectiveType.WarheadJunctionNukeLaunch)
                                .Select(
                                    j => new
                                    {
                                        ReplayID = i.Key,
                                        TeamObjectiveRTO = j,
                                    })).ToArray();

                    var firstObjectiveRTOs = warheadJunctionNukeLaunchRTOs.GroupBy(i => i.ReplayID).Select(
                        i =>
                            i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).First().TeamObjectiveRTO.IsWinner).ToArray();
                    var winRateWithFirstObjective = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First Nuke Launch",
                        GamesPlayed = firstObjectiveRTOs.Length,
                        Value = firstObjectiveRTOs.Length > 0
                            ? (decimal)firstObjectiveRTOs.Count(i => i) / firstObjectiveRTOs.Length
                            : 0,
                    };

                    var firstTwoObjectiveRTOs = warheadJunctionNukeLaunchRTOs.GroupBy(i => i.ReplayID)
                        .Select(i => i.OrderBy(j => j.TeamObjectiveRTO.TimeSpan).Take(2).ToArray())
                        .Where(i => i.Length == 2).Select(
                            i => new
                            {
                                FirstTwoObjectiveWin = i.All(j => j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                                FirstTwoObjectiveLoss = i.All(j => !j.TeamObjectiveRTO.IsWinner) ? 1 : 0,
                            }).ToArray();
                    var firstTwoObjectiveGamesPlayed =
                        firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin + i.FirstTwoObjectiveLoss);
                    var winRateWithFirstTwoObjectives = new TeamObjectiveRTOSummaryRadGridRow
                    {
                        RowTitle = "Win Rate with First and Second Nuke Launch",
                        GamesPlayed = firstTwoObjectiveGamesPlayed,
                        Value = firstTwoObjectiveGamesPlayed > 0
                            ? (decimal)firstTwoObjectiveRTOs.Sum(i => i.FirstTwoObjectiveWin) /
                              firstTwoObjectiveGamesPlayed
                            : 0,
                    };

                    var mainMapRadGrid = new TeamObjectiveRTOSummaryRadGrid
                    {
                        RadGridTitle = "Nuke Launches",
                        ValueColumnHeaderText = "Win Percent",
                        ValueFormatString = "P1",
                        RadGridRows = new List<TeamObjectiveRTOSummaryRadGridRow>
                        {
                            winRateWithFirstObjective,
                            winRateWithFirstTwoObjectives,
                        },
                    };

                    var gamesWonAndLostByObjectiveCount = warheadJunctionNukeLaunchRTOs
                        .GroupBy(i => i.TeamObjectiveRTO.IsWinner).ToDictionary(
                            i => i.Key,
                            i => i.GroupBy(j => j.ReplayID).Select(j => j.Count()).GroupBy(j => j)
                                .ToDictionary(j => j.Key, j => j.Count()));

                    if (gamesWonAndLostByObjectiveCount.Count != 2)
                    {
                        break;
                    }

                    // Also count win rate with 0 objectives
                    var rtosWithoutObjectiveWinsAndLosses = replayDictionary.Values
                        .Where(i => i.MapID == map.IdentifierId).Select(
                            i =>
                                new[] { true, false }
                                    .Select(
                                        j =>
                                            !i.TeamObjectives.Any(
                                                k =>
                                                    k.IsWinner == j && k.TeamObjectiveType ==
                                                    (int)TeamObjectiveType.WarheadJunctionNukeLaunch)
                                                ? j
                                                : (bool?)null)
                                    .Where(j => j.HasValue)
                                    .ToDictionary(j => j!.Value, j => j!.Value)).ToArray();
                    foreach (var isWinner in gamesWonAndLostByObjectiveCount.Keys)
                    {
                        gamesWonAndLostByObjectiveCount[isWinner][0] =
                            rtosWithoutObjectiveWinsAndLosses.Count(i => i.ContainsKey(isWinner));
                    }

                    var winRateByObjectiveCountRadHtmlChart = new TeamObjectiveRTOSummaryRadHtmlChart
                    {
                        RadHtmlChartTitle = "Win Rate by Nuke Launches in One Game",
                        XValueTitle = "Nuke Launches in One Game",
                        XValueFormatString = "N0",
                        YValueTitle = "Win Percent",
                        YValueFormatString = "P1",
                        RadHtmlChartItems = gamesWonAndLostByObjectiveCount[true].Select(
                                i => new
                                {
                                    ObjectiveCount = i.Key,
                                    GamesPlayed =
                                        i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                            ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                            : 0),
                                    WinRate = i.Value > 0
                                        ? (decimal)i.Value /
                                          (i.Value + (gamesWonAndLostByObjectiveCount[false].ContainsKey(i.Key)
                                              ? gamesWonAndLostByObjectiveCount[false][i.Key]
                                              : 0))
                                        : 0,
                                })
                            .Where(i => i.GamesPlayed > 100 || i.ObjectiveCount == 0)
                            .OrderBy(i => i.ObjectiveCount).Select(
                                i => new TeamObjectiveRTOSummaryRadHtmlChartItem
                                {
                                    Tooltip =
                                        TeamObjectiveRTOSummaryRadHtmlChartItem.FormatTooltipForHtml(
                                            i.GamesPlayed,
                                            i.WinRate,
                                            "P1",
                                            "Win Percent"),
                                    XValue = i.ObjectiveCount,
                                    YValue = i.WinRate,
                                }).ToList(),
                    };

                    _redisClient.TrySet(
                        "HOTSLogs:SitewideMapObjectives:" + gameMode + ":" + map.IdentifierId,
                        new TeamObjectiveRTOSummaryContainer
                        {
                            TeamObjectiveRTOSummaryRadGrids = new[] { mainMapRadGrid },
                            TeamObjectiveRTOSummaryRadHtmlCharts =
                                new[] { winRateByObjectiveCountRadHtmlChart },
                        },
                        TimeSpan.FromDays(30));
                }
                break;
            }
        }
    }

    public SitewideCharacterWinRateByTalentUpgradeEventAverageValue[]
        GetSitewideCharacterWinRateByTalentUpgradeEventAverageValue(
            DateTime datetimeBegin,
            DateTime datetimeEnd,
            int leagueId = -1,
            int gameMode = 8)
    {
        const string mySqlCommandText =
            @"select
                q.CharacterID,
                q.UpgradeEventType,
                q.AverageUpgradeEventValue,
                count(*) as GamesPlayed,
                sum(q.IsWinner) / count(*) as WinPercent
                from (select
                rcue.ReplayID,
                rcue.PlayerID,
                rc.CharacterID,
                rc.IsWinner,
                rcue.UpgradeEventType,
                round(sum(rcue.UpgradeEventValue * rcue.ReplayLengthPercent), 1) as AverageUpgradeEventValue
                from ReplayCharacterUpgradeEventReplayLengthPercent rcue
                join ReplayCharacter rc on rc.ReplayID = rcue.ReplayID and rc.PlayerID = rcue.PlayerID
                join Replay r on r.ReplayID = rc.ReplayID
                {1}
                where r.GameMode = @gameMode and r.TimestampReplay > @datetimeBegin and r.TimestampReplay < @datetimeEnd {0}
                group by
                rcue.ReplayID,
                rcue.PlayerID,
                rc.CharacterID,
                rc.IsWinner,
                rcue.UpgradeEventType) q
                group by
                q.CharacterID,
                q.UpgradeEventType,
                q.AverageUpgradeEventValue";

        var locDic = _dc.LocalizationAliases.ToDictionary(i => i.IdentifierId, i => i.PrimaryName);

        var sitewideCharacterWinRateByTalentUpgradeEventAverageValueListDictionary =
            new Dictionary<int, List<TalentUpgradeEventAverageValue>>();

        RetryIfTmpTableFull(ReadFromSql);

        void ReadFromSql()
        {
            using var mySqlConnection = new MySqlConnection(ConnectionString);
            mySqlConnection.Open();

            var leaderboardRankingJoin = GetLeaderboardRankingJoin(gameMode);

            var cmdText = string.Format(
                mySqlCommandText,
                leagueId != -1 ? "and lr.LeagueID = @leagueID" : string.Empty,
                leaderboardRankingJoin);
            using var mySqlCommand = new MySqlCommand(cmdText, mySqlConnection)
            {
                CommandTimeout = MMR.LongCommandTimeout,
            };
            mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
            mySqlCommand.Parameters.AddWithValue("@datetimeBegin", datetimeBegin);
            mySqlCommand.Parameters.AddWithValue("@datetimeEnd", datetimeEnd);
            if (leagueId != -1)
            {
                mySqlCommand.Parameters.AddWithValue("@leagueID", leagueId);
            }

            LogSqlCommand(mySqlCommand);
            using var mySqlDataReader = mySqlCommand.ExecuteReader();
            while (mySqlDataReader.Read())
            {
                var obj = new
                {
                    CharacterID = mySqlDataReader.GetInt32("CharacterID"),
                    UpgradeEventType = mySqlDataReader.GetInt32("UpgradeEventType"),
                    AverageUpgradeEventValue = mySqlDataReader.GetDecimal("AverageUpgradeEventValue"),
                    GamesPlayed = mySqlDataReader.GetInt64("GamesPlayed"),
                    WinPercent = mySqlDataReader.GetDecimal("WinPercent"),
                };

                if (!sitewideCharacterWinRateByTalentUpgradeEventAverageValueListDictionary
                        .ContainsKey(obj.CharacterID))
                {
                    sitewideCharacterWinRateByTalentUpgradeEventAverageValueListDictionary[obj.CharacterID] =
                        new List<TalentUpgradeEventAverageValue>();
                }

                sitewideCharacterWinRateByTalentUpgradeEventAverageValueListDictionary[obj.CharacterID].Add(
                    new TalentUpgradeEventAverageValue
                    {
                        UpgradeEventType = obj.UpgradeEventType,
                        AverageUpgradeEventValue = obj.AverageUpgradeEventValue,
                        GamesPlayed = (int)obj.GamesPlayed,
                        WinPercent = obj.WinPercent,
                    });
            }
        }

        return sitewideCharacterWinRateByTalentUpgradeEventAverageValueListDictionary.Select(
            i => new SitewideCharacterWinRateByTalentUpgradeEventAverageValue
            {
                TalentUpgradeEventAverageValueArray = i.Value.OrderBy(j => j.UpgradeEventType)
                    .ThenBy(j => j.AverageUpgradeEventValue).ToArray(),
                League = leagueId,
                GameMode = gameMode,
                Character = locDic[i.Key],
                LastUpdated = Now,
            }).OrderBy(i => i.Character).ToArray();
    }

    public SitewideCharacterStatisticWithCharacterLevel[] GetSitewideCharacterStatisticsByCharacterLevel(
        DateTime datetimeBegin,
        DateTime datetimeEnd,
        int leagueId = -1,
        int gameMode = 8)
    {
        const string mySqlCommandText =
            @"select rc.CharacterID, rc.CharacterLevel, count(*) as GamesPlayed, sum(rc.IsWinner) as GamesWon, SEC_TO_TIME(cast(AVG(TIME_TO_SEC(r.ReplayLength)) as unsigned)) as AverageLength
                from ReplayCharacter rc
                {1}
                join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
                {3}
                where r.GameMode = @gameMode and r.TimestampReplay >= @datetimeBegin and r.TimestampReplay < @datetimeEnd and rc.CharacterLevel > 1
                {0} {2}
                group by rc.CharacterID, rc.CharacterLevel";

        var locDic = _dc.LocalizationAliases.ToDictionary(i => i.IdentifierId, i => i.PrimaryName);

        var sitewideCharacterStatisticsWithCharacterLevel = new List<SitewideCharacterStatisticWithCharacterLevel>();

        RetryIfTmpTableFull(ReadFromSql);

        void ReadFromSql()
        {
            using var mySqlConnection = new MySqlConnection(ConnectionString);
            mySqlConnection.Open();

            var leaderboardRankingJoin = GetLeaderboardRankingJoin(gameMode);

            var cmdText = string.Format(
                mySqlCommandText,
                leagueId != -1 ? "and lr.LeagueID = @leagueID" : "",
                //gameMode == 3 ? "left join ReplayCharacter rcMirror on rcMirror.ReplayID = rc.ReplayID and rcMirror.PlayerID != rc.PlayerID and rcMirror.CharacterID = rc.CharacterID" : "",
                gameMode == 3 ? "left join replay_mirror rcMirror on rcMirror.ReplayID = rc.ReplayID" : "",
                gameMode == 3 ? "and rcMirror.ReplayID is null" : "",
                leaderboardRankingJoin);

            using var mySqlCommand = new MySqlCommand(cmdText, mySqlConnection)
            {
                CommandTimeout = MMR.LongCommandTimeout,
            };
            mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
            mySqlCommand.Parameters.AddWithValue("@datetimeBegin", datetimeBegin);
            mySqlCommand.Parameters.AddWithValue("@datetimeEnd", datetimeEnd);
            if (leagueId != -1)
            {
                mySqlCommand.Parameters.AddWithValue("@leagueID", leagueId);
            }

            LogSqlCommand(mySqlCommand);
            using var mySqlDataReader = mySqlCommand.ExecuteReader();
            while (mySqlDataReader.Read())
            {
                var obj = new
                {
                    AverageLength = mySqlDataReader["AverageLength"] is DBNull
                        ? (TimeSpan?)null
                        : mySqlDataReader.GetTimeSpan("AverageLength"),
                    GamesPlayed = mySqlDataReader.GetInt64("GamesPlayed"),
                    CharacterID = mySqlDataReader.GetInt32("CharacterID"),
                    CharacterLevel = mySqlDataReader.GetInt32("CharacterLevel"),
                    GamesWon = mySqlDataReader.GetDecimal("GamesWon"),
                };

                if (obj.AverageLength.HasValue)
                {
                    sitewideCharacterStatisticsWithCharacterLevel.Add(
                        new SitewideCharacterStatisticWithCharacterLevel
                        {
                            HeroPortraitURL = locDic[obj.CharacterID].PrepareForImageURL(),
                            Character = locDic[obj.CharacterID],
                            CharacterLevel = obj.CharacterLevel,
                            GamesPlayed = (int)obj.GamesPlayed,
                            AverageLength = obj.AverageLength.Value,
                            WinPercent = obj.GamesWon / obj.GamesPlayed,
                        });
                }
            }
        }

        return sitewideCharacterStatisticsWithCharacterLevel.ToArray();
    }

    public SitewideCharacterGameTimeWinRates[] GetSitewideCharacterStatisticsByGameTime(
        DateTime datetimeBegin,
        DateTime datetimeEnd,
        int leagueId = -1,
        int gameMode = 8)
    {
        const string mySqlCommandText =
            @"select rc.CharacterID, count(*) as GamesPlayed, sum(rc.IsWinner) as GamesWon, minute(r.ReplayLength) as ReplayLengthMinute
                from ReplayCharacter rc
                {1}
                join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
                {3}
                where r.GameMode = @gameMode and r.TimestampReplay >= @datetimeBegin and r.TimestampReplay < @datetimeEnd and (rc.CharacterLevel >= 5 or r.GameMode = 7 or r.GameMode >= 1000) {0} {2}
                group by rc.CharacterID, ReplayLengthMinute
                having ReplayLengthMinute >= 10 and ReplayLengthMinute <= 30
                order by rc.CharacterID, ReplayLengthMinute";

        var locDic = _dc.LocalizationAliases.ToDictionary(i => i.IdentifierId, i => i.PrimaryName);

        var sitewideCharacterGameTimeWinRates = new List<SitewideCharacterGameTimeWinRates>();

        RetryIfTmpTableFull(ReadFromSql);

        void ReadFromSql()
        {
            using var mySqlConnection = new MySqlConnection(ConnectionString);
            mySqlConnection.Open();

            var leaderboardRankingJoin = GetLeaderboardRankingJoin(gameMode);

            var cmdText = string.Format(
                mySqlCommandText,
                leagueId != -1 ? "and lr.LeagueID = @leagueID" : "",
                //gameMode == 3 ? "left join ReplayCharacter rcMirror on rcMirror.ReplayID = rc.ReplayID and rcMirror.PlayerID != rc.PlayerID and rcMirror.CharacterID = rc.CharacterID" : "",
                gameMode == 3 ? "left join replay_mirror rcMirror on rcMirror.ReplayID = rc.ReplayID" : "",
                gameMode == 3 ? "and rcMirror.ReplayID is null" : "",
                leaderboardRankingJoin);
            using var mySqlCommand = new MySqlCommand(cmdText, mySqlConnection)
            {
                CommandTimeout = MMR.LongCommandTimeout,
            };
            mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
            mySqlCommand.Parameters.AddWithValue("@datetimeBegin", datetimeBegin);
            mySqlCommand.Parameters.AddWithValue("@datetimeEnd", datetimeEnd);
            if (leagueId != -1)
            {
                mySqlCommand.Parameters.AddWithValue("@leagueID", leagueId);
            }

            var currentSitewideCharacterGameTimeWinRates = new SitewideCharacterGameTimeWinRates
            {
                League = leagueId,
                LastUpdated = Now,
            };
            sitewideCharacterGameTimeWinRates.Add(currentSitewideCharacterGameTimeWinRates);
            var currentSitewideCharacterGameTimeWinRateList = new List<SitewideCharacterGameTimeWinRate>();
            LogSqlCommand(mySqlCommand);
            using (var mySqlDataReader = mySqlCommand.ExecuteReader())
            {
                while (mySqlDataReader.Read())
                {
                    var obj = new
                    {
                        CharacterID = mySqlDataReader.GetInt32("CharacterID"),
                        GamesPlayed = mySqlDataReader.GetInt64("GamesPlayed"),
                        ReplayLengthMinute = mySqlDataReader.GetInt32("ReplayLengthMinute"),
                        GamesWon = mySqlDataReader.GetDecimal("GamesWon"),
                    };

                    if (currentSitewideCharacterGameTimeWinRates.Character == null)
                    // First result; initialize container
                    {
                        currentSitewideCharacterGameTimeWinRates.Character = locDic[obj.CharacterID];
                    }
                    else if (currentSitewideCharacterGameTimeWinRates.Character != locDic[obj.CharacterID])
                    {
                        // New hero in the query results
                        currentSitewideCharacterGameTimeWinRates.GameTimeWinRates =
                            currentSitewideCharacterGameTimeWinRateList.ToArray();
                        currentSitewideCharacterGameTimeWinRates = new SitewideCharacterGameTimeWinRates
                        {
                            League = leagueId,
                            Character = locDic[obj.CharacterID],
                            LastUpdated = Now,
                        };
                        sitewideCharacterGameTimeWinRates.Add(currentSitewideCharacterGameTimeWinRates);
                        currentSitewideCharacterGameTimeWinRateList = new List<SitewideCharacterGameTimeWinRate>();
                    }

                    // Add the result to the list
                    currentSitewideCharacterGameTimeWinRateList.Add(
                        new SitewideCharacterGameTimeWinRate
                        {
                            GameTimeMinuteBegin = obj.ReplayLengthMinute,
                            GamesPlayed = (int)obj.GamesPlayed,
                            WinPercent = obj.GamesWon / obj.GamesPlayed,
                        });
                }
            }

            // Add the last set to the list
            currentSitewideCharacterGameTimeWinRates.GameTimeWinRates =
                currentSitewideCharacterGameTimeWinRateList.ToArray();
        }

        return sitewideCharacterGameTimeWinRates.ToArray();
    }

    private string GetLeaderboardRankingJoin(int gameMode)
    {
        return !IsEventGameMode(gameMode)
            ? "join LeaderboardRanking lr on lr.PlayerID = rc.PlayerID and (lr.GameMode = r.gameMode or (r.gameMode in (7,9) and lr.GameMode = 3))"
            : null;
    }

    public async Task<SitewideCharacterWinPercentVsOtherCharacters>
        GetSitewideCharacterWinPercentAndOtherCharactersAsync(
            DateTime datetimeBegin,
            int leagueId,
            int gameMode,
            int characterId,
            string teamRelationship)
    {
        var locDic = _dc.LocalizationAliases.ToDictionary(i => i.IdentifierId, i => i.PrimaryName);

        var sitewideCharacterWinPercentAndOtherCharacters = new SitewideCharacterWinPercentVsOtherCharacters
        {
            Character = locDic[characterId],
            League = leagueId,
            GameMode = gameMode,
            LastUpdated = Now,
        };

        var sitewideCharacterWinPercentAndOtherCharactersList = new List<SitewideCharacterStatistic>();

        const string mySqlCommandText =
            @"select
                rc.CharacterID,
                count(*) as GamesPlayed,
                sum(rcTeam.IsWinner) as GamesWon
                from ReplayCharacter rc
                join Replay r on r.ReplayID = rc.ReplayID
                join ReplayCharacter rcTeam on rcTeam.ReplayID = rc.ReplayID and rcTeam.IsWinner {0} rc.IsWinner
                {2}
                {4}
                where
                rcTeam.CharacterID = @characterID and
                rc.CharacterID != rcTeam.CharacterID and
                (rcTeam.CharacterLevel >= 5 or r.GameMode = 7 or r.GameMode >= 1000) and
                r.GameMode = @gameMode and
                r.TimestampReplay > @datetimeBegin
                {1} {3}
                group by rc.CharacterID";

        await RetryIfTmpTableFull(ReadFromSql);

        async Task ReadFromSql()
        {
            await using var mySqlConnection = new MySqlConnection(ConnectionString);
            await mySqlConnection.OpenAsync(_token);

            var leaderboardRankingJoin = !IsEventGameMode(gameMode)
                ? "join LeaderboardRanking lr on lr.PlayerID = rcTeam.PlayerID and (lr.GameMode = r.GameMode or (r.gameMode in (7,9) and lr.GameMode = 3))"
                : null;

            var cmdText = string.Format(
                mySqlCommandText,
                teamRelationship,
                leagueId != -1 ? "and lr.LeagueID = @leagueID" : null,
                //gameMode == 3 ? "left join ReplayCharacter rcMirror on rcMirror.ReplayID = rcTeam.ReplayID and rcMirror.PlayerID != rcTeam.PlayerID and rcMirror.CharacterID = rcTeam.CharacterID" : null,
                gameMode == 3 ? "left join replay_mirror rcMirror on rcMirror.ReplayID = rcTeam.ReplayID" : null,
                gameMode == 3 ? "and rcMirror.ReplayID is null" : null,
                leaderboardRankingJoin);

            await using var mySqlCommand =
                new MySqlCommand(cmdText, mySqlConnection) { CommandTimeout = MMR.LongCommandTimeout };
            mySqlCommand.Parameters.AddWithValue("@characterID", characterId);
            mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
            mySqlCommand.Parameters.AddWithValue("@datetimeBegin", datetimeBegin);
            if (leagueId != -1)
            {
                mySqlCommand.Parameters.AddWithValue("@leagueID", leagueId);
            }

            LogSqlCommand(mySqlCommand);
            await using var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(_token);
            while (await mySqlDataReader.ReadAsync(_token))
            {
                var obj = new
                {
                    GamesPlayed = mySqlDataReader.GetInt64("GamesPlayed"),
                    CharacterID = mySqlDataReader.GetInt32("CharacterID"),
                    GamesWon = mySqlDataReader.GetDecimal("GamesWon"),
                };

                sitewideCharacterWinPercentAndOtherCharactersList.Add(
                    new SitewideCharacterStatistic
                    {
                        HeroPortraitURL = locDic[obj.CharacterID].PrepareForImageURL(),
                        Character = locDic[obj.CharacterID],
                        GamesPlayed = (int)obj.GamesPlayed,
                        WinPercent = obj.GamesWon / obj.GamesPlayed,
                    });
            }
        }

        sitewideCharacterWinPercentAndOtherCharacters.SitewideCharacterStatisticArray =
            sitewideCharacterWinPercentAndOtherCharactersList.OrderByDescending(i => i.WinPercent).ToArray();

        return sitewideCharacterWinPercentAndOtherCharacters;
    }

    public int CheckDupes(DateTime startDate, DateTime endDate)
    {
        var dupes = new List<(int, int)>();
        const string cmdText = @"
select rc.PlayerID, r.ReplayLength, r.TimestampReplay, r.ReplayID
from replaycharacter rc
inner join replay r on r.replayid=rc.replayid
where r.timestampreplay>=@p1 and r.timestampreplay<@p2
order by rc.playerid, r.timestampreplay, r.replayid";
        using var mySqlConnection = new MySqlConnection(ConnectionString);
        mySqlConnection.Open();

        using (var mySqlCommand =
               new MySqlCommand(cmdText, mySqlConnection) { CommandTimeout = MMR.LongCommandTimeout })
        {
            mySqlCommand.Parameters.AddWithValue("@p1", startDate);
            mySqlCommand.Parameters.AddWithValue("@p2", endDate);

            LogSqlCommand(mySqlCommand);
            using var mySqlDataReader = mySqlCommand.ExecuteReader();
            int previousPlayerId = default;
            DateTime previousTimestampReplay = default;
            int previousReplayId = default;
            while (mySqlDataReader.Read())
            {
                var obj = new
                {
                    PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                    ReplayLength = mySqlDataReader.GetTimeSpan("ReplayLength"),
                    TimestampReplay = mySqlDataReader.GetDateTime("TimestampReplay"),
                    ReplayID = mySqlDataReader.GetInt32("ReplayID"),
                };

                if (obj.PlayerID == previousPlayerId)
                {
                    if (previousTimestampReplay + TimeSpan.FromSeconds(15) > obj.TimestampReplay)
                    {
                        dupes.Add((obj.ReplayID, previousReplayId));
                    }
                    else
                    {
                        previousTimestampReplay = obj.TimestampReplay;
                        previousReplayId = obj.ReplayID;
                    }
                }
                else
                {
                    previousTimestampReplay = default;
                    previousReplayId = default;
                }

                previousPlayerId = obj.PlayerID;
            }
        }

        if (!dupes.Any())
        {
            return 0;
        }

        var distinctDupes = dupes.Distinct().ToList();

        var joinStr = string.Join(",", distinctDupes.Select(x => x.ToString()));
        var cmdInsert = "INSERT IGNORE INTO replay_dups2 VALUES " + joinStr;
        using var mySqlInsertCommand =
            new MySqlCommand(cmdInsert, mySqlConnection) { CommandTimeout = MMR.LongCommandTimeout };
        mySqlInsertCommand.ExecuteNonQuery();

        return distinctDupes.Count;
    }

    private string ExcludeStormTalentsIfNecessary(bool includeStormTalents, string talentSelection)
    {
        if (includeStormTalents)
        {
            return talentSelection;
        }

        var a = talentSelection.Split(',');
        var b = a.Take(6);
        var c = string.Join(",", b);
        return c;
    }

    public void RefreshCaches()
    {
        _locDic =
            _dc.LocalizationAliases
                .ToDictionary(i => i.IdentifierId, i => i.PrimaryName);
        _talentsCache = _dc.HeroTalentInformations.ToList();
    }

    private async Task RetryIfTmpTableFull(Func<Task> act)
    {
        await TmpTableFullRetryHandler.RetryIfTmpTableFull(act, OnTmpTableFullRetry, _token);
    }

    private void RetryIfTmpTableFull(Action act)
    {
        TmpTableFullRetryHandler.RetryIfTmpTableFull(act, OnTmpTableFullRetry, _token);
    }

    private void OnTmpTableFullRetry()
    {
        TmpTableFullRetry?.Invoke(this, EventArgs.Empty);
    }
}
