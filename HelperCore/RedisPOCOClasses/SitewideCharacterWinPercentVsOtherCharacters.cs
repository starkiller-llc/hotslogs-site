using System;
using System.Collections.Generic;
using System.Linq;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideCharacterWinPercentVsOtherCharacters
{
    public SitewideCharacterStatistic[] SitewideCharacterStatisticArray { get; set; }
    public int League { get; set; }
    public int GameMode { get; set; }
    public string Character { get; set; }
    public DateTime LastUpdated { get; set; }

    public static SitewideCharacterWinPercentVsOtherCharacters operator +(
        SitewideCharacterWinPercentVsOtherCharacters a,
        SitewideCharacterWinPercentVsOtherCharacters b)
    {
        // Make sure two valid instances are provided
        if (a == null || a.SitewideCharacterStatisticArray == null || a.SitewideCharacterStatisticArray.Length == 0)
        {
            return b;
        }

        if (b == null || b.SitewideCharacterStatisticArray == null || b.SitewideCharacterStatisticArray.Length == 0)
        {
            return a;
        }

        // Create the result container based on 'a'
        var result = new SitewideCharacterWinPercentVsOtherCharacters
        {
            League = a.League,
            GameMode = a.GameMode,
            Character = a.Character,
            LastUpdated = a.LastUpdated,
        };

        // Create a dictionary of 'SitewideCharacterStatistic'
        var sitewideCharacterStatisticResultDictionary = new Dictionary<string, SitewideCharacterStatistic>();

        // Loop through 'a' and 'b', combining results
        foreach (var sitewideCharacterStatisticToMerge in a.SitewideCharacterStatisticArray
                     .Concat(b.SitewideCharacterStatisticArray).Where(i => i.GamesPlayed > 0))
        {
            if (sitewideCharacterStatisticResultDictionary.ContainsKey(sitewideCharacterStatisticToMerge.Character))
            {
                // This Character has already been added; let's merge with the current results
                var sitewideCharacterStatisticResult =
                    sitewideCharacterStatisticResultDictionary[sitewideCharacterStatisticToMerge.Character];

                // Combine 'AverageLength'
                sitewideCharacterStatisticResult.AverageLength = DataHelper.InterpolateTimeSpan(
                    sitewideCharacterStatisticResult.AverageLength,
                    sitewideCharacterStatisticToMerge.AverageLength,
                    sitewideCharacterStatisticResult.GamesPlayed,
                    sitewideCharacterStatisticToMerge.GamesPlayed);

                // Combine 'GamesWon'
                var combinedGamesWon =
                    (sitewideCharacterStatisticToMerge.GamesPlayed * sitewideCharacterStatisticToMerge.WinPercent) +
                    (sitewideCharacterStatisticResult.GamesPlayed * sitewideCharacterStatisticResult.WinPercent);

                // Combine 'GamesPlayed'
                sitewideCharacterStatisticResult.GamesPlayed += sitewideCharacterStatisticToMerge.GamesPlayed;

                // Combine 'WinPercent'
                sitewideCharacterStatisticResult.WinPercent =
                    combinedGamesWon / sitewideCharacterStatisticResult.GamesPlayed;
            }
            else
                // This Character has not yet been added; let's create a new instance
            {
                sitewideCharacterStatisticResultDictionary[sitewideCharacterStatisticToMerge.Character] =
                    new SitewideCharacterStatistic
                    {
                        HeroPortraitURL = sitewideCharacterStatisticToMerge.HeroPortraitURL,
                        Character = sitewideCharacterStatisticToMerge.Character,
                        GamesPlayed = sitewideCharacterStatisticToMerge.GamesPlayed,
                        AverageLength = sitewideCharacterStatisticToMerge.AverageLength,
                        WinPercent = sitewideCharacterStatisticToMerge.WinPercent,
                    };
            }
        }

        result.SitewideCharacterStatisticArray =
            sitewideCharacterStatisticResultDictionary.Select(i => i.Value).ToArray();

        return result;
    }
}
