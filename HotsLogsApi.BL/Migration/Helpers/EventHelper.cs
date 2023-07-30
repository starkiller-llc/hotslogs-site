using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HotsLogsApi.BL.Migration.Helpers;

public class EventHelper
{
    public const int RedisPlayerProfileCacheExpireInMinutes = 10; // TODO: get rid of public constant

    private readonly HeroesdataContext _dc;
    private readonly MyDbWrapper _redisClient;
    private const int MaxReplayIDsForQuery = 2000;

    public EventHelper(HeroesdataContext dc, MyDbWrapper redisClient)
    {
        _dc = dc;
        this._redisClient = redisClient;
    }

    public List<Event> GetChildEvents(
        HeroesdataContext heroesEntity,
        Event eventEntity,
        bool includeDisabledEvents)
    {
        var eventChildrenList =
            new List<Event>();

        if (includeDisabledEvents || eventEntity.IsEnabled != 0)
        {
            eventChildrenList.Add(eventEntity);
        }

        for (var i = 0; i < eventChildrenList.Count && i < 1000; i++)
        {
            var ent = eventChildrenList[i];
            var children = heroesEntity.Events.Where(
                x => x.EventIdparent == ent.EventId && (includeDisabledEvents || x.IsEnabled != 0)).ToList();
            eventChildrenList.AddRange(children);
        }

        return eventChildrenList;
    }

    public TeamProfile GetEventProfile(
        Event eventEntity,
        bool forceCacheReset = false)
    {
        TeamProfile selectedEventProfile;

        var redisKey = $"HOTSLogs:EventProfileV12:{eventEntity.EventId}";
        if (!forceCacheReset && _redisClient.ContainsKey(redisKey))
        {
            selectedEventProfile = _redisClient.Get<TeamProfile>(redisKey);
        }
        else
        {
            // Gather all children events
            var eventChildrenList = GetChildEvents(
                _dc,
                eventEntity,
                includeDisabledEvents: false);

            selectedEventProfile = new TeamProfile
            {
                ID = eventEntity.EventId,
                Name = eventEntity.EventName,
                LastUpdated = DateTime.UtcNow,
            };

            // Query Event Profile Replay
            const string mySqlCommandTeamProfileReplay =
                @"select
                                r.ReplayID,
                                r.GameMode,
                                laMap.PrimaryName as Map,
                                r.ReplayLength,
                                r.TimestampReplay
                                from Replay r use index (IX_GameMode)
                                join LocalizationAlias laMap on laMap.IdentifierID = r.MapID
                                where r.GameMode in ({0})
                                order by r.TimestampReplay desc";

            var cmdText = string.Format(
                mySqlCommandTeamProfileReplay,
                string.Join(",", eventChildrenList.Select(i => i.EventId)));

            var teamProfileReplaysDictionary = _dc.TeamProfileReplayCustoms.FromSqlRaw(cmdText)
                .ToDictionary(
                    r => r.ReplayID,
                    x => new TeamProfileReplay
                    {
                        RID = x.ReplayID,
                        GM = x.GameMode,
                        M = x.Map,
                        RL = x.ReplayLength,
                        TR = x.TimestampReplay,
                    });

            // Query Event Profile Replay Character and Replay Character Score Result
            const string mySqlCommandTeamProfileReplayCharacterAndReplayCharacterScoreResult =
                @"select
                                rc.ReplayID,
                                coalesce(pa.PlayerIDMain, rc.PlayerID) as PlayerID,
                                rc.IsAutoSelect,
                                la.PrimaryName as `Character`,
                                rc.CharacterLevel,
                                rc.IsWinner,
                                rc.MMRBefore,
                                rc.MMRChange,
                                rcsr.Takedowns,
                                rcsr.SoloKills,
                                rcsr.Assists,
                                rcsr.Deaths,
                                rcsr.HeroDamage,
                                rcsr.SiegeDamage,
                                rcsr.StructureDamage,
                                rcsr.MinionDamage,
                                rcsr.CreepDamage,
                                rcsr.SummonDamage,
                                rcsr.TimeCCdEnemyHeroes,
                                rcsr.Healing,
                                rcsr.SelfHealing,
                                rcsr.DamageTaken,
                                rcsr.ExperienceContribution,
                                rcsr.TownKills,
                                rcsr.TimeSpentDead,
                                rcsr.MercCampCaptures,
                                rcsr.WatchTowerCaptures,
                                rcsr.MetaExperience
                                from ReplayCharacter rc
                                join Replay r use index (IX_GameMode) on r.ReplayID = rc.ReplayID
                                join LocalizationAlias la on la.IdentifierID = rc.CharacterID
                                left join ReplayCharacterScoreResult rcsr on rcsr.ReplayID = rc.ReplayID and rcsr.PlayerID = rc.PlayerID
                                left join PlayerAlt pa on pa.PlayerIDAlt = rc.PlayerID
                                where r.GameMode in ({0})";

            var cmdText2 = string.Format(
                mySqlCommandTeamProfileReplayCharacterAndReplayCharacterScoreResult,
                string.Join(",", eventChildrenList.Select(i => i.EventId)));

            var q = _dc.TeamProfileReplayCharacterCustoms.FromSqlRaw(cmdText2)
                .Select(
                    x => new TeamProfileReplayCharacter
                    {
                        RID = x.ReplayID,
                        PID = x.PlayerID,
                        IsAS = x.IsAutoSelect == 1,
                        C = x.Character,
                        CL = x.CharacterLevel,
                        IsW = x.IsWinner == 1,
                        MMRB = x.MMRBefore,
                        MMRC = x.MMRChange,
                        SR = !x.Takedowns.HasValue
                            ? null
                            : new TeamProfileReplayCharacterScoreResult
                            {
                                T = x.Takedowns.Value,
                                S = x.SoloKills!.Value,
                                A = x.Assists!.Value,
                                D = x.Deaths!.Value,
                                HD = x.HeroDamage!.Value,
                                SiD = x.SiegeDamage!.Value,
                                StD = x.StructureDamage!.Value,
                                MD = x.MinionDamage!.Value,
                                CD = x.CreepDamage!.Value,
                                SuD = x.SummonDamage!.Value,
                                TCCdEH = x.TimeCCdEnemyHeroes,
                                H = x.Healing,
                                SH = x.SelfHealing!.Value,
                                DT = x.DamageTaken,
                                EC = x.ExperienceContribution!.Value,
                                TK = x.TownKills!.Value,
                                TSD = x.TimeSpentDead!.Value,
                                MCC = x.MercCampCaptures!.Value,
                                WTC = x.WatchTowerCaptures!.Value,
                                ME = x.MetaExperience!.Value,
                            },
                    }).ToList();

            foreach (var e in q)
            {
                teamProfileReplaysDictionary[e.RID].RCs.Add(e);
            }

            // Query Replay Character Upgrade Events
            const string mySqlCommandTeamProfilePlayerAverageReplayCharacterUpgradeEvents =
                @"select
                                coalesce(pa.PlayerIDMain, rcue.PlayerID) as PlayerID,
                                rcue.UpgradeEventType,
                                rcue.UpgradeEventValue,
                                avg(rcue.ReplayLengthPercent) as ReplayLengthPercent,
                                count(*) as GamesPlayed,
                                sum(rc.IsWinner) as GamesWon
                                from ReplayCharacterUpgradeEventReplayLengthPercent rcue
                                join ReplayCharacter rc on rc.ReplayID = rcue.ReplayID and rc.PlayerID = rcue.PlayerID
                                join Replay r use index (IX_GameMode) on r.ReplayID = rcue.ReplayID
                                left join PlayerAlt pa on pa.PlayerIDAlt = rcue.PlayerID
                                where r.GameMode in ({0})
                                group by rcue.PlayerID, rcue.UpgradeEventType, rcue.UpgradeEventValue";

            var
                playerAverageReplayCharacterUpgradeEventReplayLengthPercentsList =
                    new List<TeamProfilePlayerAverageReplayCharacterUpgradeEventReplayLengthPercents>();

            var cmdText3 = string.Format(
                mySqlCommandTeamProfilePlayerAverageReplayCharacterUpgradeEvents,
                string.Join(",", eventChildrenList.Select(i => i.EventId)));

            var q2 = _dc.UpgradeCustoms.FromSqlRaw(cmdText3).Select(
                    x => new TeamProfilePlayerAverageReplayCharacterUpgradeEventReplayLengthPercents
                    {
                        PID = (int)x.PlayerID,
                        T = x.UpgradeEventType,
                        V = x.UpgradeEventValue,
                        P = x.ReplayLengthPercent,
                        GP = (int)x.GamesPlayed,
                        GW = (int)x.GamesWon,
                    })
                .ToList();

            playerAverageReplayCharacterUpgradeEventReplayLengthPercentsList.AddRange(q2);

            selectedEventProfile.PlayerAverageReplayCharacterUpgradeEventReplayLengthPercents =
                playerAverageReplayCharacterUpgradeEventReplayLengthPercentsList.ToArray();

            // Query Team Players
            var playerDictionary = new Dictionary<int, bool>();
            foreach (var teamProfileReplay in teamProfileReplaysDictionary)
            {
                foreach (var replayCharacter in teamProfileReplay.Value.RCs)
                {
                    playerDictionary[replayCharacter.PID] = true;
                }
            }

            selectedEventProfile.Players = GetTeamProfilePlayerInfo(playerDictionary.Keys.ToArray());

            // Query Team Profile Replay Periodic XP Breakdown
            // TODO: Do this later

            selectedEventProfile.Replays = teamProfileReplaysDictionary.Values.ToArray();

            _redisClient.TrySet(
                redisKey,
                selectedEventProfile,
                TimeSpan.FromMinutes(RedisPlayerProfileCacheExpireInMinutes));
        }

        return selectedEventProfile;
    }

    public TeamProfile GetTeamProfile(
        int[] playerKeys,
        int[] gameModes = null,
        DateTime? dateTimeStart = null,
        DateTime? dateTimeEnd = null,
        int? requiredPlayerId = null,
        int gamesPlayedRequired = 5,
        int? teamGamePartySize = null,
        bool forceCacheReset = false)
    {
        dateTimeStart ??= DateTime.MinValue.ToUniversalTime();
        dateTimeEnd ??= DateTime.UtcNow;

        if (gameModes == null || gameModes.Length == 0 || (gameModes.Length == 1 && gameModes.Single() == 0))
        {
            gameModes = DataHelper.GameModeWithStatistics;
        }

        teamGamePartySize ??= playerKeys.Length >= 3 ? 3 : playerKeys.Length;

        TeamProfile teamProfile;

        var redisKey =
            "HOTSLogs:TeamProfileV12:" +
            $"{requiredPlayerId ?? -1}:" +
            $"{string.Join(",", playerKeys.OrderBy(i => i))}:" +
            $"{(gameModes is { Length: > 0 } ? string.Join(",", gameModes.OrderBy(i => i)) : "0")}:" +
            $"{dateTimeStart.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}:" +
            $"{dateTimeEnd.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}:" +
            $"{gamesPlayedRequired}:{teamGamePartySize.Value}";

        if (!forceCacheReset && _redisClient.ContainsKey(redisKey))
        {
            teamProfile = _redisClient.Get<TeamProfile>(redisKey);
        }
        else
        {
            teamProfile = new TeamProfile
            {
                ID = requiredPlayerId ?? 0,
                GamesPlayedRequired = gamesPlayedRequired,
                TeamGamePartySize = teamGamePartySize,
                LastUpdated = DateTime.UtcNow,
                // Query Team Players
                Players = GetTeamProfilePlayerInfo(playerKeys),
                FilterDateTimeStart = dateTimeStart.Value,
                FilterDateTimeEnd = dateTimeEnd.Value,
            };

            // Query Team Profile Replay
            const string mySqlCommandTeamProfileReplay =
                @"select
                                r.ReplayID,
                                r.GameMode,
                                laMap.PrimaryName as Map,
                                r.ReplayLength,
                                r.TimestampReplay
                                from Replay r
                                join ReplayCharacter rc on rc.ReplayID = r.ReplayID
                                join ReplayCharacterScoreResult rcsr on rcsr.ReplayID = rc.ReplayID and rcsr.PlayerID = rc.PlayerID
                                {3}
                                join LocalizationAlias laMap on laMap.IdentifierID = r.MapID
                                where r.TimestampReplay >= {{0}} and r.TimestampReplay < {{1}} and rcsr.PlayerID in ({0}) {2}
                                group by r.ReplayID
                                having count(*) >= {1}
                                order by r.TimestampReplay desc";

            var gameModeTerm = gameModes is { Length: > 0 }
                ? $"and r.GameMode in ({string.Join(",", gameModes)})"
                : null;
            var playerIdTerm = requiredPlayerId.HasValue
                ? $"join ReplayCharacter rcRequiredPlayer on rcRequiredPlayer.ReplayID = r.ReplayID and rcRequiredPlayer.PlayerID = {requiredPlayerId.Value}"
                : null;
            var playerIds = string.Join(",", playerKeys);

            var cmdText = string.Format(
                mySqlCommandTeamProfileReplay,
                playerIds,
                teamProfile.TeamGamePartySize.Value,
                gameModeTerm,
                playerIdTerm);

            var q3 = _dc.TeamProfileCustoms
                .FromSqlRaw(cmdText, dateTimeStart, dateTimeEnd).ToList();
            var teamProfileReplaysDictionary = q3
                .ToDictionary(
                    x => x.ReplayID,
                    x => new TeamProfileReplay
                    {
                        GM = x.GameMode,
                        M = x.Map,
                        RL = x.ReplayLength,
                        TR = x.TimestampReplay,
                        RID = x.ReplayID,
                    });

            if (teamProfileReplaysDictionary.Count > 0)
            {
                // Query Team Profile Replay Character and Replay Character Score Result
                const string mySqlCommandTeamProfileReplayCharacterAndReplayCharacterScoreResult =
                    @"select
                                rc.ReplayID,
                                coalesce(pa.PlayerIDMain, rc.PlayerID) as PlayerID,
                                rc.IsAutoSelect,
                                la.PrimaryName as `Character`,
                                rc.CharacterLevel,
                                rc.IsWinner,
                                rc.MMRBefore,
                                rc.MMRChange,
                                rcsr.Takedowns,
                                rcsr.SoloKills,
                                rcsr.Assists,
                                rcsr.Deaths,
                                rcsr.HeroDamage,
                                rcsr.SiegeDamage,
                                rcsr.StructureDamage,
                                rcsr.MinionDamage,
                                rcsr.CreepDamage,
                                rcsr.SummonDamage,
                                rcsr.TimeCCdEnemyHeroes,
                                rcsr.Healing,
                                rcsr.SelfHealing,
                                rcsr.DamageTaken,
                                rcsr.ExperienceContribution,
                                rcsr.TownKills,
                                rcsr.TimeSpentDead,
                                rcsr.MercCampCaptures,
                                rcsr.WatchTowerCaptures,
                                rcsr.MetaExperience
                                from ReplayCharacter rc
                                join LocalizationAlias la on la.IdentifierID = rc.CharacterID
                                join ReplayCharacterScoreResult rcsr on rcsr.ReplayID = rc.ReplayID and rcsr.PlayerID = rc.PlayerID
                                left join PlayerAlt pa on pa.PlayerIDAlt = rc.PlayerID
                                where rc.PlayerID in ({0}) and rc.ReplayID in ({1})";

                teamProfile.IsTruncated = teamProfileReplaysDictionary.Keys.Count > MaxReplayIDsForQuery;

                var replayKeys = teamProfileReplaysDictionary.Keys
                    .OrderByDescending(i => i)
                    .Take(MaxReplayIDsForQuery);

                var replayIds = string.Join(",", replayKeys);

                var cmdText2 = string.Format(
                    mySqlCommandTeamProfileReplayCharacterAndReplayCharacterScoreResult,
                    playerIds,
                    replayIds);

                var q = _dc.TeamProfileReplayCharacterCustoms.FromSqlRaw(cmdText2)
                    .Select(
                        x => new TeamProfileReplayCharacter
                        {
                            RID = x.ReplayID,
                            PID = x.PlayerID,
                            IsAS = x.IsAutoSelect == 1,
                            C = x.Character,
                            CL = x.CharacterLevel,
                            IsW = x.IsWinner == 1,
                            MMRB = x.MMRBefore,
                            MMRC = x.MMRChange,
                            SR = !x.Takedowns.HasValue
                                ? null
                                : new TeamProfileReplayCharacterScoreResult
                                {
                                    T = x.Takedowns.Value,
                                    S = x.SoloKills!.Value,
                                    A = x.Assists!.Value,
                                    D = x.Deaths!.Value,
                                    HD = x.HeroDamage!.Value,
                                    SiD = x.SiegeDamage!.Value,
                                    StD = x.StructureDamage!.Value,
                                    MD = x.MinionDamage!.Value,
                                    CD = x.CreepDamage!.Value,
                                    SuD = x.SummonDamage!.Value,
                                    TCCdEH = x.TimeCCdEnemyHeroes,
                                    H = x.Healing,
                                    SH = x.SelfHealing!.Value,
                                    DT = x.DamageTaken,
                                    EC = x.ExperienceContribution!.Value,
                                    TK = x.TownKills!.Value,
                                    TSD = x.TimeSpentDead!.Value,
                                    MCC = x.MercCampCaptures!.Value,
                                    WTC = x.WatchTowerCaptures!.Value,
                                    ME = x.MetaExperience!.Value,
                                },
                        }).ToList();

                foreach (var e in q)
                {
                    teamProfileReplaysDictionary[e.RID].RCs.Add(e);
                }

                // Remove players who haven't played enough games together, and remove games who don't contain any of these players
                if (teamProfile.GamesPlayedRequired > 0)
                {
                    bool isAnyReplayCharactersRemoved;

                    do
                    {
                        var replayCharacters =
                            new List<TeamProfileReplayCharacter>();
                        foreach (var replay in
                                 teamProfileReplaysDictionary)
                        {
                            replayCharacters.AddRange(replay.Value.RCs);
                        }

                        var replayCharactersGroupedByPlayerId = replayCharacters
                            .GroupBy(i => i.PID)
                            .Where(i => i.Count() >= teamProfile.GamesPlayedRequired)
                            .ToDictionary(i => i.Key, _ => true);

                        var replaysWithReplayCharactersToRemove =
                            teamProfileReplaysDictionary.Where(
                                    i => i.Value.RCs.Any(j => !replayCharactersGroupedByPlayerId.ContainsKey(j.PID)))
                                .ToArray();

                        isAnyReplayCharactersRemoved = replaysWithReplayCharactersToRemove.Length > 0;

                        foreach (var replay in
                                 replaysWithReplayCharactersToRemove)
                        {
                            replay.Value.RCs = replay.Value.RCs.Where(
                                i => replayCharactersGroupedByPlayerId.ContainsKey(i.PID)).ToList();
                        }

                        foreach (var replayId in teamProfileReplaysDictionary.Keys.ToArray())
                        {
                            if (teamProfileReplaysDictionary[replayId].RCs.Count < teamGamePartySize.Value)
                            {
                                teamProfileReplaysDictionary.Remove(replayId);
                            }
                        }
                    } while (isAnyReplayCharactersRemoved);
                }

                // Query Replay Character Upgrade Events
                const string mySqlCommandTeamProfilePlayerAverageReplayCharacterUpgradeEvents =
                    @"select
                                coalesce(pa.PlayerIDMain, rcue.PlayerID) as PlayerID,
                                rcue.UpgradeEventType,
                                rcue.UpgradeEventValue,
                                avg(rcue.ReplayLengthPercent) as ReplayLengthPercent,
                                count(*) as GamesPlayed,
                                sum(rc.IsWinner) as GamesWon
                                from ReplayCharacterUpgradeEventReplayLengthPercent rcue
                                join ReplayCharacter rc on rc.ReplayID = rcue.ReplayID and rc.PlayerID = rcue.PlayerID
                                join Replay r on r.ReplayID = rcue.ReplayID
                                left join PlayerAlt pa on pa.PlayerIDAlt = rcue.PlayerID
                                where rcue.PlayerID in ({0}) and rcue.ReplayID in ({1})
                                group by rcue.PlayerID, rcue.UpgradeEventType, rcue.UpgradeEventValue";

                var
                    playerAverageReplayCharacterUpgradeEventReplayLengthPercentsList =
                        new List<TeamProfilePlayerAverageReplayCharacterUpgradeEventReplayLengthPercents>();

                if (teamProfileReplaysDictionary.Keys.Count > 0)
                {
                    var commandText = string.Format(
                        mySqlCommandTeamProfilePlayerAverageReplayCharacterUpgradeEvents,
                        playerIds,
                        string.Join(",", teamProfileReplaysDictionary.Keys));

                    var q2 = _dc.UpgradeCustoms.FromSqlRaw(commandText).Select(
                            x => new TeamProfilePlayerAverageReplayCharacterUpgradeEventReplayLengthPercents
                            {
                                PID = (int)x.PlayerID,
                                T = x.UpgradeEventType,
                                V = x.UpgradeEventValue,
                                P = x.ReplayLengthPercent,
                                GP = (int)x.GamesPlayed,
                                GW = (int)x.GamesWon,
                            })
                        .ToList();

                    playerAverageReplayCharacterUpgradeEventReplayLengthPercentsList.AddRange(q2);
                }

                teamProfile.PlayerAverageReplayCharacterUpgradeEventReplayLengthPercents =
                    playerAverageReplayCharacterUpgradeEventReplayLengthPercentsList.ToArray();
            }

            {
                // Query Team Profile Replay Periodic XP Breakdown
                // TODO: Do this later
            }

            teamProfile.Replays = teamProfileReplaysDictionary.Values.ToArray();

            _redisClient.TrySet(
                redisKey,
                teamProfile,
                TimeSpan.FromMinutes(RedisPlayerProfileCacheExpireInMinutes));
        }

        return teamProfile;
    }

    public Dictionary<int, TeamProfilePlayer> GetTeamProfilePlayerInfo(int[] playerIDs)
    {
        var teamProfilePlayerDictionary = new Dictionary<int, TeamProfilePlayer>();

        if (playerIDs == null || playerIDs.Length <= 0)
        {
            return teamProfilePlayerDictionary;
        }

        var cmdText = $@"select
                    coalesce(pMain.PlayerID, p.PlayerID) as PlayerID,
                    coalesce(pMain.`Name`, p.`Name`) as `Name`,
                    coalesce(pMain.BattleTag, p.BattleTag) as BattleTag,
                    case when coalesce(looMain.PlayerID, loo.PlayerID) is not null then true else false end as IsLOO
                    from Player p
                    left join LeaderboardOptOut loo on loo.PlayerID = p.PlayerID
                    left join PlayerAlt pa on pa.PlayerIDAlt = p.PlayerID
                    left join Player pMain on pMain.PlayerID = pa.PlayerIDMain
                    left join LeaderboardOptOut looMain on looMain.PlayerID = pMain.PlayerID
                    where p.PlayerID in ({string.Join(",", playerIDs)})";

        teamProfilePlayerDictionary = _dc.TeamProfilePlayerCustoms.FromSqlRaw(cmdText)
            .ToDictionary(
                x => x.PlayerID,
                x => new TeamProfilePlayer
                {
                    PID = x.PlayerID,
                    PN = x.Name,
                    BT = x.BattleTag,
                    IsLOO = x.IsLOO == 1,
                });

        return teamProfilePlayerDictionary;
    }
}
