// Copyright (c) StarkillerLLC. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HelperCore.RedisPOCOClasses;

public class ReplayCharacterQueryResult
{
    public int Status { get; set; }
    public string[] SelectedHeroes { get; set; }
    public int[] SelectedGameModes { get; set; }
    public int[] SelectedMapIDs { get; set; }
    public sbyte[] SelectedLeagues { get; set; }
    public ReplayCharacterQueryResultEntry CurrentReplayCharacterQueryResultEntry { get; set; }
    public ReplayCharacterQueryResultEntry[] PotentialReplayCharacterQueryResultEntries { get; set; }
    public double HoursSinceLastUpdate { get; set; }

    public static ReplayCharacterQueryResult QueryReplayCharacterResult(
        ConcurrentDictionary<int,
                ConcurrentDictionary<int,
                    ConcurrentDictionary<sbyte,
                        ConcurrentDictionary<sbyte, List<sbyte[][]>[]>>>>
            recentReplayCharacterResultsByGameModeAndMapAndHero,
        Dictionary<int, string> locDic,
        sbyte[] selectedCharacterIDs,
        int[] gameModes,
        int[] mapIDs,
        sbyte[] leagueIDs,
        DateTime lastUpdated)
    {
        // Set up return result container
        var result = new ReplayCharacterQueryResult
        {
            SelectedHeroes = selectedCharacterIDs.Select(i => locDic[i]).OrderBy(i => i).ToArray(),
            SelectedGameModes = gameModes,
            SelectedMapIDs = mapIDs,
            SelectedLeagues = leagueIDs,
            CurrentReplayCharacterQueryResultEntry = new ReplayCharacterQueryResultEntry(),
        };

        // Set up dictionary to store potential replay character query result entries, which we will fill in while reading through data
        Dictionary<sbyte, int[]> results = null;
        if (selectedCharacterIDs.Length != 5)
        {
            results = locDic.Keys
                .Where(i => selectedCharacterIDs.All(j => j != i))
                .ToDictionary(i => (sbyte)i, i => new int[2]);
        }

        var gamesPlayed = new int[2];
        byte index;

        // Grab two heroes for dictionary indexes; it doesn't matter which two hero combination, data is duplicated to save processing time
        var twoHeroDictionaryIndex =
            selectedCharacterIDs.OrderBy(i => Guid.NewGuid()).Take(2).OrderBy(i => i).ToArray();
        if (twoHeroDictionaryIndex.Length == 1)
        {
            twoHeroDictionaryIndex = new[] { twoHeroDictionaryIndex[0], twoHeroDictionaryIndex[0] };
        }

        foreach (var gameMode in gameModes)
        {
            foreach (var mapId in mapIDs)
            {
                var recent =
                    recentReplayCharacterResultsByGameModeAndMapAndHero[gameMode][mapId][twoHeroDictionaryIndex[0]][
                        twoHeroDictionaryIndex[1]];

                byte currentSelectedHero;
                if (leagueIDs == null ||
                    leagueIDs.Length == 0 ||
                    (leagueIDs.Length == 1 && leagueIDs[0] == -1))
                {
                    // The requester doesn't care about league
                    if (selectedCharacterIDs.Length <= 2)
                    {
                        // If the requester has selected 1 or 2 heroes, they fit into our index, so all entries in the index match
                        for (var isWinner = 0; isWinner < recent.Length; isWinner++)
                        {
                            for (var matchResultIndex = 0;
                                 matchResultIndex < recent[isWinner].Count;
                                 matchResultIndex++)
                            {
                                // All entries in this index match the selected heroes; increment the counter
                                gamesPlayed[isWinner]++;

                                // Let's also increment the other heroes in the match, to show potential heroes for the selected composition
                                for (index = currentSelectedHero = 0; index < 5; index++)
                                {
                                    if (currentSelectedHero < selectedCharacterIDs.Length &&
                                        recent[isWinner][matchResultIndex][index][0] ==
                                        selectedCharacterIDs[currentSelectedHero])
                                    {
                                        currentSelectedHero++;
                                    }
                                    else if (results != null)
                                    {
                                        results[recent[isWinner][matchResultIndex][index][0]][isWinner]++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // The requester has selected more than 2 heroes; we need to check each replay in the index
                        for (var isWinner = 0; isWinner < recent.Length; isWinner++)
                        {
                            for (var matchResultIndex = 0;
                                 matchResultIndex < recent[isWinner].Count;
                                 matchResultIndex++)
                            {
                                // For each set of heroes in a replay, check to see if the requester's selected heroes are played
                                for (index = currentSelectedHero = 0;
                                     index < 5 && currentSelectedHero < selectedCharacterIDs.Length;
                                     index++)
                                {
                                    if (recent[isWinner][matchResultIndex][index][0] ==
                                        selectedCharacterIDs[currentSelectedHero])
                                    {
                                        currentSelectedHero++;
                                    }
                                }

                                if (currentSelectedHero == selectedCharacterIDs.Length)
                                {
                                    // If all heroes are found, increment the counter
                                    gamesPlayed[isWinner]++;

                                    // Let's also increment the other heroes in the match, to show potential heroes for the selected composition
                                    for (index = currentSelectedHero = 0; index < 5; index++)
                                    {
                                        if (currentSelectedHero < selectedCharacterIDs.Length &&
                                            recent[isWinner][matchResultIndex][index][0] ==
                                            selectedCharacterIDs[currentSelectedHero])
                                        {
                                            currentSelectedHero++;
                                        }
                                        else if (results != null)
                                        {
                                            results[recent[isWinner][matchResultIndex][index][0]][isWinner]++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // The requester wants a specific league
                    for (var isWinner = 0; isWinner < recent.Length; isWinner++)
                    {
                        for (var matchResultIndex = 0; matchResultIndex < recent[isWinner].Count; matchResultIndex++)
                        {
                            // For each set of heroes in a replay, check to see if the requester's selected heroes are played
                            for (index = currentSelectedHero = 0;
                                 index < 5 && currentSelectedHero < selectedCharacterIDs.Length;
                                 index++)
                            {
                                if (recent[isWinner][matchResultIndex][index][0] ==
                                    selectedCharacterIDs[currentSelectedHero])
                                {
                                    // Make sure the matched hero also is in the specified league
                                    if (leagueIDs.Any(i => i == recent[isWinner][matchResultIndex][index][1]))
                                    {
                                        currentSelectedHero++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            if (currentSelectedHero == selectedCharacterIDs.Length)
                            {
                                // If all heroes are found, increment the counter
                                gamesPlayed[isWinner]++;

                                // Let's also increment the other heroes in the match, to show potential heroes for the selected composition
                                for (index = currentSelectedHero = 0; index < 5; index++)
                                {
                                    if (currentSelectedHero < selectedCharacterIDs.Length &&
                                        recent[isWinner][matchResultIndex][index][0] ==
                                        selectedCharacterIDs[currentSelectedHero])
                                    {
                                        currentSelectedHero++;
                                    }
                                    else if (leagueIDs.Any(i => i == recent[isWinner][matchResultIndex][index][1]))
                                    {
                                        // Make sure we only increment heroes in the specified league
                                        if (results != null)
                                        {
                                            results[recent[isWinner][matchResultIndex][index][0]][isWinner]++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Combine, order, and return the results
        result.CurrentReplayCharacterQueryResultEntry.GamesPlayed = gamesPlayed[0] + gamesPlayed[1];
        if (result.CurrentReplayCharacterQueryResultEntry.GamesPlayed != 0)
        {
            result.CurrentReplayCharacterQueryResultEntry.WinPercent =
                (decimal)gamesPlayed[1] / result.CurrentReplayCharacterQueryResultEntry.GamesPlayed;

            if (results != null)
            {
                result.PotentialReplayCharacterQueryResultEntries = results.Keys
                    .Select(
                        i =>
                            new ReplayCharacterQueryResultEntry
                            {
                                HeroPortraitURL = locDic[i].PrepareForImageURL(),
                                Character = locDic[i],
                                GamesPlayed = results[i][0] + results[i][1],
                                WinPercent = results[i][1] > 0
                                    ? (decimal)results[i][1] / (results[i][0] + results[i][1])
                                    : 0.0m,
                            })
                    .OrderByDescending(i => i.WinPercent)
                    .ToArray();
            }
        }

        result.HoursSinceLastUpdate = (DateTime.UtcNow - lastUpdated).TotalHours;

        return result;
    }
}