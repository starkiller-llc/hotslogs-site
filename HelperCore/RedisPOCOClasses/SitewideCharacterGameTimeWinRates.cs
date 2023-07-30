using System;
using System.Collections.Generic;
using System.Linq;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideCharacterGameTimeWinRates
{
    public SitewideCharacterGameTimeWinRate[] GameTimeWinRates { get; set; }
    public int League { get; set; }
    public int GameMode { get; set; }
    public string Character { get; set; }
    public DateTime LastUpdated { get; set; }

    public static SitewideCharacterGameTimeWinRates operator +(
        SitewideCharacterGameTimeWinRates a,
        SitewideCharacterGameTimeWinRates b)
    {
        // Make sure two valid instances are provided
        if (a == null || a.GameTimeWinRates == null || a.GameTimeWinRates.Length == 0)
        {
            return b;
        }

        if (b == null || b.GameTimeWinRates == null || b.GameTimeWinRates.Length == 0)
        {
            return a;
        }

        // Create the result container based on 'a'
        var result = new SitewideCharacterGameTimeWinRates
        {
            League = a.League,
            GameMode = a.GameMode,
            Character = a.Character,
            LastUpdated = a.LastUpdated,
        };

        // Create a dictionary of 'SitewideCharacterGameTimeWinRate'
        var sitewideCharacterGameTimeWinRateResultDictionary = new Dictionary<int, SitewideCharacterGameTimeWinRate>();

        // Loop through 'a' and 'b', combining results
        foreach (var sitewideCharacterGameTimeWinRateToMerge in a.GameTimeWinRates.Concat(b.GameTimeWinRates)
                     .Where(i => i.GamesPlayed > 0))
        {
            if (sitewideCharacterGameTimeWinRateResultDictionary.ContainsKey(
                    sitewideCharacterGameTimeWinRateToMerge.GameTimeMinuteBegin))
            {
                // This GameTime has already been added; let's merge with the current results
                var sitewideCharacterGameTimeWinRateResult =
                    sitewideCharacterGameTimeWinRateResultDictionary[sitewideCharacterGameTimeWinRateToMerge
                        .GameTimeMinuteBegin];

                // Combine 'GamesWon'
                var combinedGamesWon =
                    (sitewideCharacterGameTimeWinRateToMerge.GamesPlayed *
                     sitewideCharacterGameTimeWinRateToMerge.WinPercent) +
                    (sitewideCharacterGameTimeWinRateResult.GamesPlayed *
                     sitewideCharacterGameTimeWinRateResult.WinPercent);

                // Combine 'GamesPlayed'
                sitewideCharacterGameTimeWinRateResult.GamesPlayed +=
                    sitewideCharacterGameTimeWinRateToMerge.GamesPlayed;

                // Combine 'WinPercent'
                sitewideCharacterGameTimeWinRateResult.WinPercent =
                    combinedGamesWon / sitewideCharacterGameTimeWinRateResult.GamesPlayed;
            }
            else
                // This GameTime has not yet been added; let's create a new instance
            {
                sitewideCharacterGameTimeWinRateResultDictionary
                    [sitewideCharacterGameTimeWinRateToMerge.GameTimeMinuteBegin] = new SitewideCharacterGameTimeWinRate
                {
                    GameTimeMinuteBegin = sitewideCharacterGameTimeWinRateToMerge.GameTimeMinuteBegin,
                    GamesPlayed = sitewideCharacterGameTimeWinRateToMerge.GamesPlayed,
                    WinPercent = sitewideCharacterGameTimeWinRateToMerge.WinPercent,
                };
            }
        }

        result.GameTimeWinRates = sitewideCharacterGameTimeWinRateResultDictionary.Select(i => i.Value).ToArray();

        return result;
    }
}