// Copyright (c) StarkillerLLC. All rights reserved.

using HelperCore;
using HelperCore.RedisPOCOClasses;
using HotsLogsApi.BL.Migration;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class ReplayCharacterResultsController : ControllerBase
{
    [HttpGet("{parameter1}/{parameter2}")]
    public ReplayCharacterQueryResult GetDataByString(string parameter1, string parameter2)
    {
        //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "https://www.hotslogs.com");

        // Validate input - futher validation is done in Global.QueryRecentReplayCharacterResultsByMapAndHero()
        var parameter1Parts = parameter1.Split(',');
        if (parameter1Parts.Length != 3)
        {
            return new ReplayCharacterQueryResult { Status = 1 };
        }

        var availableMaps = Global.GetLocalizationAlias()
            .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map)
            .ToDictionary(i => i.IdentifierId, i => i);
        var mapIDs = parameter1Parts[0].Split('|').Select(int.Parse).Distinct()
            .Where(i => availableMaps.ContainsKey(i)).OrderBy(i => i).ToArray();
        if (mapIDs.Length == 0)
        {
            mapIDs = availableMaps.Keys.OrderBy(i => i).ToArray();
        }

        var leagueIds = parameter1Parts[1]
            .Split('|')
            .Select(sbyte.Parse)
            .Distinct()
            .OrderBy(i => i)
            .ToArray();
        if (leagueIds.Length >= 6)
        {
            leagueIds = null;
        }
        else if (leagueIds.Length > 1)
        {
            leagueIds = leagueIds.Where(i => i != -1).ToArray();
        }

        var gameModes = parameter1Parts[2]
            .Split('|')
            .Select(int.Parse)
            .Distinct()
            .Where(i => DataHelper.GameModeWithMMR.Any(j => j == i))
            .OrderBy(i => i)
            .ToArray();
        if (gameModes.Length == 0)
        {
            gameModes = DataHelper.GameModeWithMMR;
        }

        var lastUpdate = Global.RecentReplayCharacterResultsByMapAndHeroLastUpdated;
        if (parameter2 == "-1")
        {
            // No hero selected - let's just return the sitewide win rates for the selected parameters
            var result = new ReplayCharacterQueryResult
            {
                SelectedHeroes = new[] { "-1" },
                SelectedGameModes = gameModes,
                SelectedMapIDs = mapIDs,
                SelectedLeagues = leagueIds,
            };

            var sitewideStatisticsDateTimeStrings = new List<string>();
            for (var currentDateTimeBegin = DateTime.UtcNow
                     .AddDays(-1 * (int)Global.TeamDraftHelperDaysOfReplayCharacterResultsToCache)
                     .StartOfWeek(DayOfWeek.Sunday);
                 currentDateTimeBegin < DateTime.UtcNow.AddDays(7);
                 currentDateTimeBegin = currentDateTimeBegin.AddDays(7))
            {
                sitewideStatisticsDateTimeStrings.Add(
                    currentDateTimeBegin.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }

            // Use sitewide map win rates, so we can calculate using any map selection
            SitewideMapStatistics sitewideMapStatistics = null;

            if (leagueIds != null)
            {
                foreach (var sitewideStatisticsDateTimeString in sitewideStatisticsDateTimeStrings)
                {
                    foreach (var gameMode in gameModes)
                    {
                        foreach (var leagueId in leagueIds)
                        {
                            sitewideMapStatistics += DataHelper.RedisCacheGet<SitewideMapStatistics>(
                                $"HOTSLogs:SitewideMapStatisticsV2:{sitewideStatisticsDateTimeString}:{leagueId}:{gameMode}");
                        }
                    }
                }
            }

            if (sitewideMapStatistics?.SitewideMapStatisticArray == null)
            {
                return new ReplayCharacterQueryResult { Status = 1 };
            }

            var selectedMapNamesDictionary = mapIDs.ToDictionary(i => availableMaps[i].PrimaryName, i => true);

            result.PotentialReplayCharacterQueryResultEntries = sitewideMapStatistics.SitewideMapStatisticArray
                .Where(i => selectedMapNamesDictionary.ContainsKey(i.Map))
                .GroupBy(i => i.Character).Select(
                    i => new ReplayCharacterQueryResultEntry
                    {
                        HeroPortraitURL = Global.HeroPortraitImages[i.Key],
                        Character = i.Key,
                        GamesPlayed = i.Sum(j => j.GamesPlayed),
                        WinPercent = i.Sum(j => j.GamesPlayed * j.WinPercent) / i.Sum(j => j.GamesPlayed),
                    })
                .OrderByDescending(i => i.WinPercent)
                .ThenBy(i => i.Character)
                .ToArray();

            ReplayCharacterQueryResultCalculateGamesPlayedPercent(result);

            result.HoursSinceLastUpdate = (DateTime.UtcNow - lastUpdate).TotalHours;

            return result;
        }

        var stats = Global.RecentReplayCharacterResultsByGameModeAndMapAndHero;
        if ((DateTime.UtcNow - lastUpdate).TotalHours > 1000 ||
            stats == null ||
            !gameModes.Any(i => stats.ContainsKey(i)) ||
            mapIDs.Any(i => gameModes.Any(j => !stats[j].ContainsKey(i))))
        {
            // Data isn't available - this takes ~30 minutes to generate, so we won't make the user wait
            // Users shouldn't experience this, though if IIS is reset unexpectedly or other issues arise it may be null
            return new ReplayCharacterQueryResult { Status = 2 };
        }

        {
            // Convert Hero names to their IdentifierIDs
            var localizationAliasesIdentifierIdDictionary = Global.GetLocalizationAliasesIdentifierIDDictionary();
            var selectedCharacterIDs = parameter2.Split(',')
                .Select(i => (sbyte)localizationAliasesIdentifierIdDictionary[i])
                .Distinct()
                .OrderBy(i => i)
                .Take(5)
                .ToArray();

            if (selectedCharacterIDs.Length == 0)
            {
                return new ReplayCharacterQueryResult { Status = 1 };
            }

            // Make sure new heroes exist in the cached data dictionaries
            foreach (var gameMode in gameModes)
            {
                foreach (var mapId in mapIDs)
                {
                    var stats2 = stats[gameMode][mapId];

                    foreach (var selectedHeroId in selectedCharacterIDs)
                    {
                        if (!stats2.ContainsKey(selectedHeroId))
                        {
                            stats2[selectedHeroId] = new ConcurrentDictionary<sbyte, List<sbyte[][]>[]>();
                        }
                    }

                    foreach (var outerKey in stats2.Keys)
                    {
                        foreach (var selectedHeroId in selectedCharacterIDs)
                        {
                            if (!stats2[outerKey].ContainsKey(selectedHeroId))
                            {
                                stats2[outerKey][selectedHeroId] = new List<sbyte[][]>[2];
                                for (var i = 0; i < stats2[outerKey][selectedHeroId].Length; i++)
                                {
                                    stats2[outerKey][selectedHeroId][i] = new List<sbyte[][]>();
                                }
                            }
                        }
                    }
                }
            }

            // Query the data
            var result = ReplayCharacterQueryResult.QueryReplayCharacterResult(
                stats,
                Global.GetLocalizationAlias()
                    .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero)
                    .ToDictionary(i => i.IdentifierId, i => i.PrimaryName),
                selectedCharacterIDs,
                gameModes,
                mapIDs,
                leagueIds,
                lastUpdate);

            ReplayCharacterQueryResultCalculateGamesPlayedPercent(result);

            return result;
        }
    }

    private static void ReplayCharacterQueryResultCalculateGamesPlayedPercent(
        ReplayCharacterQueryResult replayCharacterQueryResult)
    {
        var results = replayCharacterQueryResult.PotentialReplayCharacterQueryResultEntries;

        if (results != null && results.Length > 0)
        {
            var maxPotentialGamesPlayed = results.Max(i => i.GamesPlayed);

            if (maxPotentialGamesPlayed > 0)
            {
                foreach (var entry in results)
                {
                    entry.GamesPlayedPercent = (decimal)entry.GamesPlayed / maxPotentialGamesPlayed;
                }
            }
        }
    }
}
