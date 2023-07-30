using System;
using System.Collections.Generic;
using System.Linq;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideCharacterWinRateByDateTime
{
    public SitewideDateTimeWinRate[] DateTimeWinRates { get; set; }
    public int League { get; set; }
    public int GameMode { get; set; }
    public string Character { get; set; }
    public DateTime LastUpdated { get; set; }

    public static SitewideCharacterWinRateByDateTime operator +(
        SitewideCharacterWinRateByDateTime a,
        SitewideCharacterWinRateByDateTime b)
    {
        // Make sure two valid instances are provided
        if (a == null || a.DateTimeWinRates == null || a.DateTimeWinRates.Length == 0)
        {
            return b;
        }

        if (b == null || b.DateTimeWinRates == null || b.DateTimeWinRates.Length == 0)
        {
            return a;
        }

        // Create the result container based on 'a'
        var result = new SitewideCharacterWinRateByDateTime
        {
            League = a.League,
            GameMode = a.GameMode,
            Character = a.Character,
            LastUpdated = a.LastUpdated,
        };

        // Create a dictionary of 'SitewideDateTimeWinRate'
        var dateTimeWinRatesResultDictionary = new Dictionary<DateTime, SitewideDateTimeWinRate>();

        // Loop through 'a' and 'b', combining results
        foreach (var dateTimeWinRateToMerge in a.DateTimeWinRates.Concat(b.DateTimeWinRates)
                     .Where(i => i.GamesPlayed > 0))
        {
            if (dateTimeWinRatesResultDictionary.ContainsKey(dateTimeWinRateToMerge.DateTimeBegin))
            {
                // This entry has already been added; let's merge with the current results
                var dateTimeWinRateResult = dateTimeWinRatesResultDictionary[dateTimeWinRateToMerge.DateTimeBegin];

                // Combine 'GamesWon'
                var combinedGamesWon = (dateTimeWinRateToMerge.GamesPlayed * dateTimeWinRateToMerge.WinPercent) +
                                       (dateTimeWinRateResult.GamesPlayed * dateTimeWinRateResult.WinPercent);

                // Combine 'GamesPlayed'
                dateTimeWinRateResult.GamesPlayed += dateTimeWinRateToMerge.GamesPlayed;

                // Combine 'WinPercent'
                dateTimeWinRateResult.WinPercent = combinedGamesWon / dateTimeWinRateResult.GamesPlayed;
            }
            else
                // This entry has not yet been added; let's create a new instance
            {
                dateTimeWinRatesResultDictionary[dateTimeWinRateToMerge.DateTimeBegin] = new SitewideDateTimeWinRate
                {
                    DateTimeBegin = dateTimeWinRateToMerge.DateTimeBegin,
                    DateTimeEnd = dateTimeWinRateToMerge.DateTimeEnd,
                    GamesPlayed = dateTimeWinRateToMerge.GamesPlayed,
                    WinPercent = dateTimeWinRateToMerge.WinPercent,
                };
            }
        }

        result.DateTimeWinRates = dateTimeWinRatesResultDictionary.Select(i => i.Value).ToArray();

        return result;
    }
}