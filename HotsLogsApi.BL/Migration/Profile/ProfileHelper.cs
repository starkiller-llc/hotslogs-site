using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.Helpers;
using Microsoft.EntityFrameworkCore;
using ServiceStackReplacement;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Player = Heroes.DataAccessLayer.Models.Player;
using Heroes.DataAccessLayer.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HotsLogsApi.BL.Migration.Profile;

public class ProfileHelper
{
    private const int RedisPlayerProfileCacheExpireInMinutes = 10;
    private const string PlayerProfilePlayerRelationshipQueryTypeFriends = "=";
    private const string PlayerProfilePlayerRelationshipQueryTypeRivals = "!=";

    private const string PlayerProfilePlayerRelationshipQuery =
        @"select p.PlayerID, p.`Name` as PlayerName, pa.FavoriteCharacter, count(*) as GamesPlayedWith, sum(rc.IsWinner) / count(*) as WinPercent, lr.CurrentMMR
            from ReplayCharacter rc
            join ReplayCharacter rcPlayedWith
                on rcPlayedWith.ReplayID = rc.ReplayID
                and rcPlayedWith.PlayerID != rc.PlayerID
                and rcPlayedWith.IsWinner {0} rc.IsWinner
            join Replay r on r.ReplayID = rc.ReplayID
            join Player p on p.PlayerID = rcPlayedWith.PlayerID
            join PlayerAggregate pa on pa.PlayerID = p.PlayerID and pa.GameMode = {2}
            join LeaderboardRanking lr on lr.PlayerID = p.PlayerID and (lr.GameMode = {2} or ({2} = 7 and lr.GameMode = 3))
            left join LeaderboardOptOut l on l.PlayerID = p.PlayerID
            where r.TimestampReplay >= {{0}} and r.TimestampReplay < {{1}} and l.PlayerID is null and rc.PlayerID in ({3}) {1}
            group by p.PlayerID, PlayerName, lr.CurrentMMR
            having count(*) >= {4}
            order by count(*) desc";

    private readonly IServiceProvider _svcp;
    private readonly MyDbWrapper _redisClient;
    private readonly EventHelper _eventHelper;

    public ProfileHelper(IServiceProvider svcp, MyDbWrapper redisClient, EventHelper eventHelper)
    {
        _svcp = svcp;
        _redisClient = redisClient;
        _eventHelper = eventHelper;
    }

    public PlayerProfile GetPlayerProfile(
    Player player,
    GameMode? gameMode = null,
    DateTime? dateTimeStart = null,
    DateTime? dateTimeEnd = null,
    bool forceCacheReset = false)
    {
        const int timeout = 30;

        dateTimeStart ??= DateTime.MinValue.ToUniversalTime();

        dateTimeEnd ??= DateTime.UtcNow;

        var redisKey = "HOTSLogs:PlayerProfileV18:" + player.PlayerId + ":" + (gameMode ?? 0) +
                       ":" + dateTimeStart.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ":" +
                       dateTimeEnd.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var redisKeyCachingInProgress = redisKey + ":CachingInProgress";

        if (!forceCacheReset)
        {
            // Let's use Redis to coordinate querying the player profile - ideally if 10 people request this profile, we will only run the query once
            while (_redisClient.ContainsKey(redisKeyCachingInProgress))
            {
                // Verify the 'Caching in Progress' key is valid and will expire properly
                var ttl = _redisClient.GetTimeToLive(redisKeyCachingInProgress);
                if ((!ttl.HasValue &&
                     _redisClient.ContainsKey(redisKeyCachingInProgress)) ||
                    ttl is { TotalSeconds: > timeout * 2 })
                {
                    _redisClient.Remove(redisKeyCachingInProgress);
                    throw new Exception(
                        $"Error in Profile: Caching In Progress: Invalid Time to Live: PlayerID: {player.PlayerId}, Time to Live: {ttl}");
                }

                // TODO: Get rid of this ancient relic, don't Thread.Sleep in a web app!!! -- Aviad
                Thread.Sleep(100);
            }

            if (_redisClient.ContainsKey(redisKey))
            {
                return _redisClient.Get<PlayerProfile>(redisKey);
            }
        }

        // Flag Redis to tell users we are currently querying this profile
        _redisClient.TrySet(
            redisKeyCachingInProgress,
            true,
            TimeSpan.FromSeconds(timeout));

        // Get any player alts
        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        var playerIdAlts = player.GetPlayerIdAlts(dc.Database.GetConnectionString());
        playerIdAlts.Add(player.PlayerId);
        var playerIdAltsArray = playerIdAlts.ToArray();

        var gameModes = gameMode.HasValue ? new[] { (int)gameMode.Value } : DataHelper.GameModeWithMMR;

        // If this is a player's Event Profile, get a list of child EventID's
        if (gameMode.HasValue && (int)gameMode.Value > 1000)
        {
            var eventId = (int)gameMode.Value;
            var eventEntity = dc.Events
                .Include(x => x.EventIdparentNavigation)
                .Single(i => i.EventId == eventId);
            gameModes = _eventHelper.GetChildEvents(dc, eventEntity, includeDisabledEvents: false)
                .Select(i => i.EventId).ToArray();
        }

        var selectedPlayerProfile = new PlayerProfile
        {
            PlayerID = player.PlayerId,
            BattleNetRegionId = player.BattleNetRegionId,
            PlayerName = player.Name,
            LeaderboardRankings = player.LeaderboardRankings
                .Where(i => gameModes.Contains(i.GameMode))
                .Select(
                    i => new PlayerLeaderboardRanking
                    {
                        GameMode = i.GameMode,
                        CurrentMMR = i.CurrentMmr,
                        LeagueID = i.LeagueId,
                        LeagueName = i.League?.LeagueName,
                        LeagueRank = i.LeagueRank,
                        LeagueRequiredGames = i.League?.RequiredGames,
                    }).OrderByDescending(i => i.GameMode).ToArray(),
            PlayerProfileMMRMilestonesV3 = player.PlayerMmrMilestoneV3s
                .Where(i => i.MilestoneDate >= dateTimeStart.Value && i.MilestoneDate < dateTimeEnd.Value)
                .OrderBy(i => i.MilestoneDate)
                .Select(
                    i => new PlayerProfileMMRMilestoneV3
                    {
                        GameMode = i.GameMode,
                        MilestoneDate = i.MilestoneDate,
                        MMRRating = i.Mmrrating,
                    })
                .ToArray(),
            FilterGameMode = gameMode.HasValue ? (int)gameMode.Value : null,
            FilterDateTimeStart = dateTimeStart.Value,
            FilterDateTimeEnd = dateTimeEnd.Value,
            LastUpdated = DateTime.UtcNow,
        };

        var dataArgs = new ProfileDataArgs
        {
            PlayerIdAlts = playerIdAltsArray,
            GameModes = gameModes,
            GameMode = gameMode,
            DateStart = dateTimeStart.Value,
            DateEnd = dateTimeEnd.Value,
            MapId = null,
        };

        // Use multiple threads to query different parts of a player's profile
        // This speeds up profile generation significantly
        var taskList = new List<Task>
        {
            Task.Run(() => selectedPlayerProfile.TotalGamesPlayed = FillTotalGamesPlayed(dataArgs)),
            Task.Run(() => selectedPlayerProfile.OverallMVPPercent = FillOverallMvpPercent(dataArgs)),
            Task.Run(() => selectedPlayerProfile.TotalTimePlayed = FillTotalTimePlayed(dataArgs)),
            Task.Run(() => selectedPlayerProfile.PlayerProfileCharacterStatistics = FillCharacterStats(dataArgs)),
            Task.Run(() => selectedPlayerProfile.PlayerProfileMapStatistics = FillMapStats(dataArgs)),
            Task.Run(
                () => selectedPlayerProfile.PlayerProfileCharacterWinPercentVsOtherCharacters =
                    FillWinRateWithOrAgainst("!=", dataArgs)),
            Task.Run(
                () => selectedPlayerProfile.PlayerProfileCharacterWinPercentWithOtherCharacters =
                    FillWinRateWithOrAgainst("=", dataArgs)),
            Task.Run(() => selectedPlayerProfile.PlayerProfileGameTimeWinRate = FillWinRateByGameTime(dataArgs)),
            Task.Run(
                () => selectedPlayerProfile.PlayerProfileFriends = FillRelationships(
                    PlayerProfilePlayerRelationshipQueryTypeFriends,
                    dataArgs)),
            Task.Run(
                () => selectedPlayerProfile.PlayerProfileRivals = FillRelationships(
                    PlayerProfilePlayerRelationshipQueryTypeRivals,
                    dataArgs)),
            Task.Run(() => selectedPlayerProfile.PlayerProfileSharedReplays = FillSharedReplays(dataArgs)),
            Task.Run(() => SetPlayerMmr(selectedPlayerProfile)),
            Task.Run(() => selectedPlayerProfile.Reputation = GetReputation(selectedPlayerProfile)),
        };

        foreach (var task in taskList)
        {
            task.Wait();
        }

        taskList.Clear();

        // Calculate Player Profile Character Role and Role Statistics
        var heroRoleConcurrentDictionary =
            Global.GetHeroRoleConcurrentDictionary();
        selectedPlayerProfile.PlayerProfileCharacterRoleStatistics = DataHelper.HeroRoles.Select(
            i => new PlayerProfileCharacterRoleStatistic
            {
                Role = i,
                GamesPlayed = selectedPlayerProfile.PlayerProfileCharacterStatistics.Where(
                    j => heroRoleConcurrentDictionary.ContainsKey(j.Character) &&
                         heroRoleConcurrentDictionary[j.Character] == i).Sum(j => j.GamesPlayed),
                WinPercent = 0,
            }).ToArray();

        selectedPlayerProfile.OverallWinPercent =
            (selectedPlayerProfile.PlayerProfileCharacterStatistics.Sum(i => i.GamesPlayed * i.WinPercent) /
             (selectedPlayerProfile.TotalGamesPlayed != 0 ? selectedPlayerProfile.TotalGamesPlayed : 1));

        foreach (var playerProfileCharacterRoleStatistic in
                 selectedPlayerProfile
                     .PlayerProfileCharacterRoleStatistics.Where(i => i.GamesPlayed > 0))
        {
            playerProfileCharacterRoleStatistic.WinPercent = playerProfileCharacterRoleStatistic.GamesPlayed == 0
                ? 0
                : selectedPlayerProfile
                    .PlayerProfileCharacterStatistics
                    .Where(
                        i => heroRoleConcurrentDictionary.ContainsKey(i.Character) &&
                             heroRoleConcurrentDictionary[i.Character] == playerProfileCharacterRoleStatistic.Role)
                    .Sum(j => j.GamesPlayed * j.WinPercent) / playerProfileCharacterRoleStatistic.GamesPlayed;
        }

        var localizationAliasesIdentifierIdDictionary =
            Global.GetLocalizationAliasesIdentifierIDDictionary();

        foreach (var playerProfileMapStatistic in selectedPlayerProfile
                     .PlayerProfileMapStatistics)
        {
            taskList.Add(
                Task.Run(
                    () =>
                    {
                        int? mapId = localizationAliasesIdentifierIdDictionary.ContainsKey(
                            playerProfileMapStatistic.Map)
                            ? localizationAliasesIdentifierIdDictionary[playerProfileMapStatistic.Map]
                            : null;
                        var newDataArgs = new ProfileDataArgs
                        {
                            GameMode = dataArgs.GameMode,
                            DateEnd = dataArgs.DateEnd,
                            DateStart = dataArgs.DateStart,
                            GameModes = dataArgs.GameModes,
                            MapId = mapId,
                            PlayerIdAlts = dataArgs.PlayerIdAlts,
                        };
                        return playerProfileMapStatistic.MapDetailStatistics = FillCharacterStats(newDataArgs);
                    }));
        }

        foreach (var task in taskList)
        {
            task.Wait();
        }

        _redisClient.TrySet(
            redisKey,
            selectedPlayerProfile,
            TimeSpan.FromMinutes(RedisPlayerProfileCacheExpireInMinutes));

        // Remove the 'In Progress' Redis key
        _redisClient.Remove(redisKeyCachingInProgress);

        return selectedPlayerProfile;
    }

    private int FillTotalGamesPlayed(ProfileDataArgs dataArgs)
    {
        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        IEnumerable<ReplayCharacter> q = dc.ReplayCharacters
            .Include(x => x.Replay)
            .Where(x => dataArgs.PlayerIdAlts.Contains(x.PlayerId))
            .Where(x => x.Replay.TimestampReplay >= dataArgs.DateStart && x.Replay.TimestampReplay < dataArgs.DateEnd);
        if (dataArgs.GameModes.Length > 0)
        {
            q = q.Where(x => dataArgs.GameModes.Contains(x.Replay.GameMode));
        }

        var totalGames = q.Count();
        return totalGames;
    }

    private TimeSpan FillTotalTimePlayed(ProfileDataArgs dataArgs)
    {
        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        IEnumerable<ReplayCharacter> q = dc.ReplayCharacters
            .Include(x => x.Replay)
            .Where(x => dataArgs.PlayerIdAlts.Contains(x.PlayerId))
            .Where(x => x.Replay.TimestampReplay >= dataArgs.DateStart && x.Replay.TimestampReplay < dataArgs.DateEnd);
        if (dataArgs.GameModes.Length > 0)
        {
            q = q.Where(x => dataArgs.GameModes.Contains(x.Replay.GameMode));
        }

        var totalTimeInSec = q.Sum(x => x.Replay.ReplayLength.TotalSeconds);
        return TimeSpan.FromSeconds(totalTimeInSec);
    }

    private PlayerProfileCharacterStatistic[] FillCharacterStats(ProfileDataArgs dataArgs)
    {
        const string mySqlCommandText =
            @"select rc.CharacterID,
                    max(rc.CharacterLevel) as CharacterLevel,
                    count(*) as GamesPlayed,
                    SEC_TO_TIME(CAST(AVG(TIME_TO_SEC(r.ReplayLength)) AS UNSIGNED)) as AverageLength,
                    sum(rc.IsWinner) / count(*) as WinPercent
                    from ReplayCharacter rc
                    join Replay r on r.ReplayID = rc.ReplayID
                    where r.TimestampReplay >= {{0}} and r.TimestampReplay < {{1}} and rc.PlayerID in ({2}) {0} {1}
                    group by rc.CharacterID
                    having AverageLength is not null
                    order by WinPercent desc, CharacterID";

        var locDic = Global.GetLocalizationAliasesPrimaryNameDictionary();

        var sqlFragment = dataArgs.GameModes is { Length: > 0 }
            ? "and r.GameMode in (" + string.Join(",", dataArgs.GameModes) + ")"
            : null;
        var cmdText = string.Format(
            mySqlCommandText,
            dataArgs.MapId.HasValue ? $"and r.MapID = {dataArgs.MapId}" : null,
            sqlFragment,
            string.Join(",", dataArgs.PlayerIdAlts));

        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        var playerProfileCharacterStatistics = dc.ProfileCharacterStatCustoms
            .FromSqlRaw(cmdText, dataArgs.DateStart, dataArgs.DateEnd)
            .Select(
                r => new PlayerProfileCharacterStatistic
                {
                    HeroPortraitURL =
                        locDic.ContainsKey(r.CharacterID)
                            ? Global.HeroPortraitImages[locDic[r.CharacterID]]
                            : "/Images/Heroes/Portraits/Unknown.png",
                    Character =
                        locDic.ContainsKey(r.CharacterID)
                            ? locDic[r.CharacterID]
                            : "Unknown",
                    CharacterLevel = r.CharacterLevel,
                    GamesPlayed = (int)r.GamesPlayed,
                    AverageLength = r.AverageLength,
                    WinPercent = r.WinPercent,
                })
            .ToArray();

        return playerProfileCharacterStatistics;
    }

    private PlayerProfileMapStatistic[] FillMapStats(ProfileDataArgs dataArgs)
    {
        const string mySqlCommandText =
            @"select r.MapID,
                    count(*) as GamesPlayed,
                    SEC_TO_TIME(CAST(AVG(TIME_TO_SEC(r.ReplayLength)) AS UNSIGNED)) as AverageLength,
                    sum(rc.IsWinner) / count(*) as WinPercent
                    from ReplayCharacter rc
                    join Replay r on r.ReplayID = rc.ReplayID
                    where r.TimestampReplay >= {{0}} and r.TimestampReplay < {{1}} and rc.PlayerID in ({1}) {0}
                    group by r.MapID
                    order by WinPercent desc";

        var locDic = Global.GetLocalizationAliasesPrimaryNameDictionary();

        var sqlFragment = dataArgs.GameModes is { Length: > 0 }
            ? $"and r.GameMode in ({string.Join(",", dataArgs.GameModes)})"
            : null;
        var commandText = string.Format(
            mySqlCommandText,
            sqlFragment,
            string.Join(",", dataArgs.PlayerIdAlts));

        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        var playerProfileMapStatistics = dc.ProfileMapStatCustoms
            .FromSqlRaw(commandText, dataArgs.DateStart, dataArgs.DateEnd)
            .Select(
                r => new PlayerProfileMapStatistic
                {
                    Map = locDic.ContainsKey(r.MapID) ? locDic[r.MapID] : "Unknown",
                    GamesPlayed = (int)r.GamesPlayed,
                    AverageLength = r.AverageLength,
                    WinPercent = r.WinPercent,
                }).ToArray();

        return playerProfileMapStatistics;
    }

    private decimal FillOverallMvpPercent(ProfileDataArgs dataArgs)
    {
        //const string overallMvpPercentQuery =
        //    @"select case when q.GamesPlayed > 0 then q.GamesWithMVP / q.GamesPlayed else 0 end as MVPPercent from
        //        (select
        //        count(*) as GamesPlayed,
        //        sum(case when rcma.PlayerID in ({0}) then 1 else 0 end) as GamesWithMVP
        //        from ReplayCharacter rc
        //        join Replay r on r.ReplayID = rc.ReplayID
        //        join ReplayCharacterMatchAward rcma on rcma.ReplayID = rc.ReplayID and rcma.MatchAwardType = 1
        //        where rc.PlayerID in ({0}) {1} and r.TimestampReplay > @DateTimeStart and r.TimestampReplay < @DateTimeEnd) q";

        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        IEnumerable<ReplayCharacter> q = dc.ReplayCharacters
            .Include(x => x.Replay)
            .Include(x => x.ReplayCharacterMatchAwards)
            .Where(x => dataArgs.PlayerIdAlts.Contains(x.PlayerId))
            .Where(x => x.Replay.TimestampReplay >= dataArgs.DateStart && x.Replay.TimestampReplay < dataArgs.DateEnd);
        if (dataArgs.GameModes.Length > 0)
        {
            q = q.Where(x => dataArgs.GameModes.Contains(x.Replay.GameMode));
        }

        // ReSharper disable PossibleMultipleEnumeration
        var totalGames = q.Count();
        var mvpGames = q.Count(x => x.ReplayCharacterMatchAwards.Any(z => z.MatchAwardType == 1));
        // ReSharper restore PossibleMultipleEnumeration
        var mvpPct = totalGames == 0 ? 0 : 1.0m * mvpGames / totalGames;

        return mvpPct;
    }

    private PlayerProfilePlayerRelationship[] FillRelationships(
        string relationshipType,
        ProfileDataArgs dataArgs,
        int gamesPlayedTogetherRequirement = 5)
    {
        var locDic = Global.GetLocalizationAliasesPrimaryNameDictionary();

        var commandText = string.Format(
            PlayerProfilePlayerRelationshipQuery,
            relationshipType,
            dataArgs.GameMode.HasValue ? "and r.GameMode = " + (int)dataArgs.GameMode.Value : null,
            dataArgs.GameMode.HasValue ? (int)dataArgs.GameMode.Value : (int)GameMode.QuickMatch,
            string.Join(",", dataArgs.PlayerIdAlts),
            gamesPlayedTogetherRequirement);

        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        var stats = dc.ProfilePlayerRelationshipCustoms
            .FromSqlRaw(commandText, dataArgs.DateStart, dataArgs.DateEnd)
            .Select(
                r => new PlayerProfilePlayerRelationship
                {
                    PlayerID = r.PlayerID,
                    HeroPortraitURL =
                        locDic.ContainsKey(r.FavoriteCharacter)
                            ? Global.HeroPortraitImages[locDic[r.FavoriteCharacter]]
                            : "/Images/Heroes/Portraits/Unknown.png",
                    PlayerName = r.PlayerName,
                    FavoriteHero =
                        locDic.ContainsKey(r.FavoriteCharacter)
                            ? locDic[r.FavoriteCharacter]
                            : "Unknown",
                    GamesPlayedWith = (int)r.GamesPlayedWith,
                    WinPercent = r.WinPercent,
                    CurrentMMR = r.CurrentMMR,
                })
            .ToArray();

        return stats;
    }

    private ReplaySharePOCO[] FillSharedReplays(ProfileDataArgs dataArgs)
    {
        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        var distinctPlayerRegions = dc.Players.Where(x => dataArgs.PlayerIdAlts.Contains(x.PlayerId))
            .Select(x => x.BattleNetRegionId).Distinct()
            .ToList();

        var sharedReplaysList = new List<ReplaySharePOCO>();

        foreach (var selectedBattleNetRegionId in distinctPlayerRegions)
        {
            if (_redisClient.ContainsKey("HOTSLogs:ReplaySharesV2:" + selectedBattleNetRegionId))
            {
                sharedReplaysList.AddRange(
                    _redisClient.Get<ReplayShares>("HOTSLogs:ReplaySharesV2:" + selectedBattleNetRegionId)
                        .ReplaySharesList);
            }
        }

        var sharedReplaysDictionary = sharedReplaysList.GroupBy(i => i.ReplayID)
            .Select(i => i.OrderByDescending(j => j.TimestampReplay).First())
            .ToDictionary(i => i.ReplayID, i => i);
        sharedReplaysList.Clear();

        if (sharedReplaysDictionary.Keys.Count == 0)
        {
            return sharedReplaysList.ToArray();
        }

        IEnumerable<ReplayCharacter> q = dc.ReplayCharacters
            .Include(r => r.Replay)
            .Where(r => r.Replay.TimestampReplay >= dataArgs.DateStart && r.Replay.TimestampReplay < dataArgs.DateEnd)
            .Where(r => sharedReplaysDictionary.Keys.Contains(r.ReplayId))
            .Where(r => dataArgs.PlayerIdAlts.Contains(r.PlayerId));

        if (dataArgs.GameMode is not null)
        {
            q = q.Where(r => r.Replay.GameMode == (int)dataArgs.GameMode);
        }

        var replayIds = q
            .Select(r => r.ReplayId)
            .Distinct()
            .Select(r => sharedReplaysDictionary[r]).ToList();

        sharedReplaysList.AddRange(replayIds);

        return sharedReplaysList.ToArray();
    }

    private SitewideCharacterGameTimeWinRate[] FillWinRateByGameTime(ProfileDataArgs dataArgs)
    {
        const string playerProfileWinRateByGameTimeQuery =
            @"select count(*) as GamesPlayed, sum(rc.IsWinner) as GamesWon, minute(r.ReplayLength) as ReplayLengthMinute
                from ReplayCharacter rc
                join Replay r on r.ReplayID = rc.ReplayID
                where r.TimestampReplay >= {{0}} and r.TimestampReplay < {{1}} and rc.PlayerID in ({1}) {0}
                group by ReplayLengthMinute
                having ReplayLengthMinute >= 10 and ReplayLengthMinute <= 30
                order by ReplayLengthMinute";

        var sqlFragment = dataArgs.GameModes is { Length: > 0 }
            ? "and r.GameMode in (" + string.Join(",", dataArgs.GameModes) + ")"
            : null;
        var cmdText = string.Format(
            playerProfileWinRateByGameTimeQuery,
            sqlFragment,
            string.Join(",", dataArgs.PlayerIdAlts));

        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        var playerProfileGameTimeWinRateList = dc.ProfileGameTimeWinRateCustoms
            .FromSqlRaw(cmdText, dataArgs.DateStart, dataArgs.DateEnd)
            .Select(
                r => new SitewideCharacterGameTimeWinRate
                {
                    GameTimeMinuteBegin = r.ReplayLengthMinute,
                    GamesPlayed = (int)r.GamesPlayed,
                    WinPercent = r.GamesPlayed == 0 ? 0 : r.GamesWon / r.GamesPlayed,
                })
            .ToArray();

        return playerProfileGameTimeWinRateList;
    }

    private PlayerProfileCharacterStatistic[] FillWinRateWithOrAgainst(
        string relationshipType,
        ProfileDataArgs dataArgs)
    {
        const string playerProfileWinPercentAndOtherCharactersQuery =
            @"select rcPlayedWith.CharacterID, count(*) as GamesPlayed, sum(rc.IsWinner) as GamesWon
                from ReplayCharacter rc
                join Replay r on r.ReplayID = rc.ReplayID
                join ReplayCharacter rcPlayedWith on rcPlayedWith.ReplayID = rc.ReplayID and rcPlayedWith.IsWinner {2} rc.IsWinner and rcPlayedWith.PlayerID != rc.PlayerID
                where r.TimestampReplay >= {{0}} and r.TimestampReplay < {{1}} and rc.PlayerID in ({1}) {0}
                group by rcPlayedWith.CharacterID";

        var locDic = Global.GetLocalizationAliasesPrimaryNameDictionary();

        var sqlFragment = dataArgs.GameModes is { Length: > 0 }
            ? "and r.GameMode in (" + string.Join(",", dataArgs.GameModes) + ")"
            : null;
        var cmdText = string.Format(
            playerProfileWinPercentAndOtherCharactersQuery,
            sqlFragment,
            string.Join(",", dataArgs.PlayerIdAlts),
            relationshipType);

        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        var stats = dc.ProfilePlayerStatCustoms
            .FromSqlRaw(cmdText, dataArgs.DateStart, dataArgs.DateEnd)
            .Select(
                r => new PlayerProfileCharacterStatistic
                {
                    HeroPortraitURL = locDic.ContainsKey(r.CharacterID)
                        ? Global.HeroPortraitImages[locDic[r.CharacterID]]
                        : "/Images/Heroes/Portraits/Unknown.png",
                    Character = locDic.ContainsKey(r.CharacterID)
                        ? locDic[r.CharacterID]
                        : "Unknown",
                    GamesPlayed = (int)r.GamesPlayed,
                    WinPercent = r.GamesPlayed == 0 ? 0 : r.GamesWon / r.GamesPlayed,
                })
            .OrderByDescending(i => i.WinPercent).ToArray();

        return stats;
    }

    private int GetReputation(PlayerProfile profile)
    {
        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        var q = dc.Reputations
            .Where(r => r.PlayerId == profile.PlayerID)
            .Select(r => r.Reputation1)
            .SingleOrDefault();
        return q;
    }

    private void SetPlayerMmr(PlayerProfile playerProfile)
    {
        /* Sets CurrentMMR using PlayerMMRMilestoneV3 table, instead of LeaderboardRankings
         * Users are confused why CurrentMMR doesn't match up with their Match History or MMR Milestones
         * This is because the Leaderboard updates slower than MMR Milestones, but let's just use MMR Milestones instead
         */

        using var scope = _svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        foreach (var leaderboardRanking in playerProfile.LeaderboardRankings.Where(i => i.CurrentMMR.HasValue))
        {
            var q = dc.PlayerMmrMilestoneV3s
                .Where(r => r.PlayerId == playerProfile.PlayerID && r.GameMode == leaderboardRanking.GameMode)
                .OrderByDescending(r => r.MilestoneDate)
                .Select(r => r.Mmrrating)
                .FirstOrDefault();

            leaderboardRanking.CurrentMMR = q;
        }
    }
}
