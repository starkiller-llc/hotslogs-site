using HelperCore.RedisPOCOClasses;
using HelperCore;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.Helpers;
using Microsoft.EntityFrameworkCore;
using ServiceStackReplacement;
using System;
using System.Linq;
using System.Collections.Generic;
using Player = Heroes.DataAccessLayer.Models.Player;

namespace HotsLogsApi.BL.Migration.MatchHistory;

public class MatchHistoryHelper
{
    private readonly HeroesdataContext _dc;
    private readonly MyDbWrapper _redisClient;
    private readonly EventHelper _eventHelper;

    public MatchHistoryHelper(HeroesdataContext dc, MyDbWrapper redisClient, EventHelper eventHelper)
    {
        _dc = dc;
        _redisClient = redisClient;
        _eventHelper = eventHelper;
    }

    public PlayerMatchHistory GetMatchHistory(
        Event eventEntity,
        bool forceCacheReset = false)
    {
        if (!forceCacheReset && _redisClient.ContainsKey("HOTSLogs:EventMatchHistoryV5:" + eventEntity.EventId))
        {
            return _redisClient.Get<PlayerMatchHistory>("HOTSLogs:EventMatchHistoryV5:" + eventEntity.EventId);
        }

        // Gather all children events
        var eventChildrenList = _eventHelper.GetChildEvents(
            _dc,
            eventEntity,
            includeDisabledEvents: false);

        const string mySqlCommand =
            @"select
                            r.ReplayID,
                            r.GameMode,
                            laMap.PrimaryName as Map,
                            r.ReplayLength,
                            concat('<strong>',
                                   group_concat(case when rc.IsWinner = 1 then p.`Name` else null end separator ', '),
                                   '</strong> vs<br>',
                                   group_concat(case when rc.IsWinner = 0 then p.`Name` else null end separator ', '),
                                   '<div style=""display:none;"">',
                                   group_concat(laHero.PrimaryName), 
                                   ',',
                                   group_concat(laHero.NewGroup), '</div>')
                                as Characters,
                            avg(rc.CharacterLevel) as CharacterLevel,
                            avg(rc.MMRBefore) as MMRBefore,
                            avg(rc.MMRChange) as MMRChange,
                            r.TimestampReplay
                            from Replay r use index (IX_GameMode)
                            join ReplayCharacter rc on rc.ReplayID = r.ReplayID
                            left join PlayerAlt pa on pa.PlayerIDAlt = rc.PlayerID
                            join Player p on p.PlayerID = coalesce(pa.PlayerIDMain, rc.PlayerID)
                            join LocalizationAlias laHero on laHero.IdentifierID = rc.CharacterID
                            join LocalizationAlias laMap on laMap.IdentifierID = r.MapID
                            where r.GameMode in ({0})
                            group by r.ReplayID
                            order by r.TimestampReplay desc";

        var parameters = string.Join(",", eventChildrenList.Select(i => i.EventId));
        var q = _dc.PlayerMatchCustoms.FromSqlRaw(mySqlCommand, parameters).ToList();
        var playerMatches = q.Select(
            r => new PlayerMatch
            {
                RID = r.ReplayID,
                GM = (GameMode)r.GameMode,
                M = r.Map,
                RL = r.ReplayLength,
                C = r.Characters,
                CL = r.CharacterLevel,
                IsW = false,
                MMRB = r.MMRBefore,
                MMRC = r.MMRChange,
                IsRS = false,
                TR = r.TimestampReplay,
            });

        var playerMatchHistory = new PlayerMatchHistory
        {
            PlayerID = -1,
            PlayerName = "Event: " + eventEntity.EventName,
            PlayerMatches = playerMatches.ToArray(),
            LastUpdated = DateTime.UtcNow,
        };

        _redisClient.TrySet(
            "HOTSLogs:EventMatchHistoryV5:" + eventEntity.EventId,
            playerMatchHistory,
            TimeSpan.FromMinutes(EventHelper.RedisPlayerProfileCacheExpireInMinutes));

        return playerMatchHistory;
    }

    public PlayerMatchHistory GetMatchHistory(
        Player player,
        int[] otherPlayerIDs,
        bool forceCacheReset = false)
    {
        // Check if the user is requesting to view their Match History with other players
        var otherPlayers = new List<Player>();
        if (otherPlayerIDs != null)
        {
            foreach (var otherPlayerId in otherPlayerIDs)
            {
                var otherPlayer = _dc.Players
                    .Include(x => x.LeaderboardOptOut)
                    .SingleOrDefault(i => i.PlayerId == otherPlayerId && i.LeaderboardOptOut == null);
                if (otherPlayer == null)
                {
                    otherPlayerIDs = null;
                    otherPlayers = null;
                    break;
                }

                otherPlayers.Add(otherPlayer);
            }
        }

        if (!forceCacheReset && _redisClient.ContainsKey(
                "HOTSLogs:PlayerMatchHistoryV6:" + player.PlayerId + ":" +
                (otherPlayerIDs != null ? string.Join(",", otherPlayerIDs) : "-1")))
        {
            return _redisClient.Get<PlayerMatchHistory>(
                "HOTSLogs:PlayerMatchHistoryV6:" + player.PlayerId + ":" +
                (otherPlayerIDs != null ? string.Join(",", otherPlayerIDs) : "-1"));
        }

        // Set up Other Players query if we are looking at a Match History for players playing together
        var otherPlayersQuery = "";
        if (otherPlayerIDs != null)
        {
            for (var i = 0; i < otherPlayerIDs.Length; i++)
            {
                otherPlayersQuery += string.Format(
                    "join ReplayCharacter rcOtherPlayer{0} on rcOtherPlayer{0}.ReplayID = rc.ReplayID and rcOtherPlayer{0}.PlayerID = {1} ",
                    i,
                    otherPlayerIDs[i]);
            }
        }

        var localizationAliasesPrimaryNameDictionary = Global.GetLocalizationAliasesPrimaryNameDictionary();

        // Check if the selected Player has any alts that should be included
        var playerIDAlts = player.GetPlayerIdAlts(_dc.Database.GetConnectionString());

        const string mySqlCommand =
            @"select
                            rc.ReplayID,
                            r.GameMode,
                            r.MapID,
                            r.ReplayLength,
                            rc.CharacterID,
                            rc.CharacterLevel,
                            rc.IsWinner,
                            rc.MMRBefore,
                            rc.MMRChange,
                            r.TimestampReplay > date_add(now(), interval -30 day) and count(l.PlayerID) = 0 as IsReplayShareable,
                            r.TimestampReplay
                            from ReplayCharacter rc
                            join Replay r on r.ReplayID = rc.ReplayID
                            join ReplayCharacter rcReplay on rcReplay.ReplayID = r.ReplayID
                            left join LeaderboardOptOut l on l.PlayerID = rcReplay.PlayerID
                            {0}
                            where rc.PlayerID = {{0}} {1}
                            group by
                            rc.ReplayID,
                            r.GameMode,
                            r.MapID,
                            r.ReplayLength,
                            rc.CharacterID,
                            rc.CharacterLevel,
                            rc.IsWinner,
                            rc.MMRBefore,
                            rc.MMRChange,
                            r.TimestampReplay
                            order by r.TimestampReplay desc";

        var sqlFragment = playerIDAlts.Count > 0
            ? "or rc.PlayerID in (" + string.Join(",", playerIDAlts) + @")"
            : null;
        var commandText = string.Format(
            mySqlCommand,
            otherPlayersQuery,
            sqlFragment);

        var q = _dc.PlayerMatchCustoms2.FromSqlRaw(commandText, player.PlayerId).ToList();
        var playerMatches = q.Select(
            r => new PlayerMatch
            {
                RID = r.ReplayID,
                GM = (GameMode)r.GameMode,
                M = localizationAliasesPrimaryNameDictionary.ContainsKey(r.MapID)
                    ? localizationAliasesPrimaryNameDictionary[
                        r.MapID]
                    : "Unknown",
                RL = r.ReplayLength,
                C = localizationAliasesPrimaryNameDictionary.ContainsKey(r.CharacterID)
                    ? localizationAliasesPrimaryNameDictionary[
                        r.CharacterID]
                    : "Unknown",
                CL = r.CharacterLevel,
                IsW = r.IsWinner == 1,
                MMRB = r.MMRBefore,
                MMRC = r.MMRChange,
                IsRS = r.IsReplayShareable == 1,
                TR = r.TimestampReplay,
            });

        var playerMatchHistory = new PlayerMatchHistory
        {
            PlayerID = player.PlayerId,
            PlayerName = player.Name,
            PlayerMatches = playerMatches.ToArray(),
            LastUpdated = DateTime.UtcNow,
        };

        if (otherPlayerIDs != null)
        {
            playerMatchHistory.OtherPlayerIDs = otherPlayerIDs;
            playerMatchHistory.OtherPlayerNames = otherPlayers.Select(i => i.Name).ToArray();
        }

        _redisClient.TrySet(
            "HOTSLogs:PlayerMatchHistoryV6:" + player.PlayerId + ":" +
            (otherPlayerIDs != null ? string.Join(",", otherPlayerIDs) : "-1"),
            playerMatchHistory,
            TimeSpan.FromMinutes(EventHelper.RedisPlayerProfileCacheExpireInMinutes));

        return playerMatchHistory;
    }
}
