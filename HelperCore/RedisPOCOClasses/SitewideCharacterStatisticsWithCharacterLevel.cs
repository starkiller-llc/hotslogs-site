using System;
using System.Collections.Generic;
using System.Linq;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideCharacterStatisticsWithCharacterLevel
{
    public SitewideCharacterStatisticWithCharacterLevel[] SitewideCharacterStatisticWithCharacterLevelArray
    {
        get;
        set;
    }

    /* public DateTime DateTimeBegin { get; set; }
    public DateTime DateTimeEnd { get; set; }
    public int League { get; set; }
    public int GameMode { get; set; }
    public DateTime LastUpdated { get; set; } */

    public static SitewideCharacterStatisticsWithCharacterLevel operator +(
        SitewideCharacterStatisticsWithCharacterLevel a,
        SitewideCharacterStatisticsWithCharacterLevel b)
    {
        // Make sure two valid instances are provided
        if (a?.SitewideCharacterStatisticWithCharacterLevelArray == null ||
            a.SitewideCharacterStatisticWithCharacterLevelArray.Length == 0)
        {
            return b ?? a;
        }

        if (b?.SitewideCharacterStatisticWithCharacterLevelArray == null ||
            b.SitewideCharacterStatisticWithCharacterLevelArray.Length == 0)
        {
            return a ?? b;
        }

        // Create the result container based on 'a'
        var result = new SitewideCharacterStatisticsWithCharacterLevel();

        // Create a dictionary of 'SitewideCharacterStatisticWithCharacterLevel'
        var stats = new Dictionary<string, SitewideCharacterStatisticWithCharacterLevel>();

        // Loop through 'a' and 'b', combining results
        var statsCollection = a.SitewideCharacterStatisticWithCharacterLevelArray
            .Concat(b.SitewideCharacterStatisticWithCharacterLevelArray)
            .Where(i => i.GamesPlayed > 0);
        foreach (var src in statsCollection)
        {
            var key = $"{src.Character}:{src.CharacterLevel}";
            if (stats.ContainsKey(key))
            {
                // This Character / CharacterLevel has already been added; let's merge with the current results
                var dst = stats[key];

                // Combine 'AverageLength'
                dst.AverageLength = DataHelper.InterpolateTimeSpan(
                    dst.AverageLength,
                    src.AverageLength,
                    dst.GamesPlayed,
                    src.GamesPlayed);

                // Combine 'GamesWon'
                var combinedGamesWon = (src.GamesPlayed * src.WinPercent) + (dst.GamesPlayed * dst.WinPercent);

                // Combine 'GamesPlayed'
                dst.GamesPlayed += src.GamesPlayed;

                // Combine 'WinPercent'
                dst.WinPercent = combinedGamesWon / dst.GamesPlayed;
            }
            else
            {
                // This Character has not yet been added; let's create a new instance
                stats[key] = new SitewideCharacterStatisticWithCharacterLevel
                {
                    HeroPortraitURL = src.HeroPortraitURL,
                    Character = src.Character,
                    CharacterLevel = src.CharacterLevel,
                    GamesPlayed = src.GamesPlayed,
                    AverageLength = src.AverageLength,
                    WinPercent = src.WinPercent,
                };
            }
        }

        result.SitewideCharacterStatisticWithCharacterLevelArray =
            stats.Select(i => i.Value).ToArray();

        return result;
    }
}