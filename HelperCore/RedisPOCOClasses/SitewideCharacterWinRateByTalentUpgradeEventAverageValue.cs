using System;
using System.Collections.Generic;
using System.Linq;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideCharacterWinRateByTalentUpgradeEventAverageValue
{
    public TalentUpgradeEventAverageValue[] TalentUpgradeEventAverageValueArray { get; set; }
    public int League { get; set; }
    public int GameMode { get; set; }
    public string Character { get; set; }
    public DateTime LastUpdated { get; set; }

    public static SitewideCharacterWinRateByTalentUpgradeEventAverageValue operator +(
        SitewideCharacterWinRateByTalentUpgradeEventAverageValue a,
        SitewideCharacterWinRateByTalentUpgradeEventAverageValue b)
    {
        // Make sure two valid instances are provided
        if (a == null || a.TalentUpgradeEventAverageValueArray == null ||
            a.TalentUpgradeEventAverageValueArray.Length == 0)
        {
            return b;
        }

        if (b == null || b.TalentUpgradeEventAverageValueArray == null ||
            b.TalentUpgradeEventAverageValueArray.Length == 0)
        {
            return a;
        }

        // Create the result container based on 'a'
        var result = new SitewideCharacterWinRateByTalentUpgradeEventAverageValue
        {
            League = a.League,
            GameMode = a.GameMode,
            Character = a.Character,
            LastUpdated = a.LastUpdated,
        };

        // Create a dictionary of 'TalentUpgradeEventAverageValue'
        var talentUpgradeEventAverageValueDictionary =
            new Dictionary<int, Dictionary<decimal, TalentUpgradeEventAverageValue>>();

        // Loop through 'a' and 'b', combining results
        foreach (var talentUpgradeEventAverageValueToMerge in a.TalentUpgradeEventAverageValueArray
                     .Concat(b.TalentUpgradeEventAverageValueArray).Where(i => i.GamesPlayed > 0))
        {
            if (talentUpgradeEventAverageValueDictionary.ContainsKey(
                    talentUpgradeEventAverageValueToMerge.UpgradeEventType) &&
                talentUpgradeEventAverageValueDictionary[talentUpgradeEventAverageValueToMerge.UpgradeEventType]
                    .ContainsKey(talentUpgradeEventAverageValueToMerge.AverageUpgradeEventValue))
            {
                // This Value has already been added; let's merge with the current results
                var talentUpgradeEventAverageValueResult =
                    talentUpgradeEventAverageValueDictionary[talentUpgradeEventAverageValueToMerge.UpgradeEventType][
                        talentUpgradeEventAverageValueToMerge.AverageUpgradeEventValue];

                // Combine 'GamesWon'
                var combinedGamesWon =
                    (talentUpgradeEventAverageValueToMerge.GamesPlayed *
                     talentUpgradeEventAverageValueToMerge.WinPercent) +
                    (talentUpgradeEventAverageValueResult.GamesPlayed *
                     talentUpgradeEventAverageValueResult.WinPercent);

                // Combine 'GamesPlayed'
                talentUpgradeEventAverageValueResult.GamesPlayed += talentUpgradeEventAverageValueToMerge.GamesPlayed;

                // Combine 'WinPercent'
                talentUpgradeEventAverageValueResult.WinPercent =
                    combinedGamesWon / talentUpgradeEventAverageValueResult.GamesPlayed;
            }
            else
            {
                // This Value has not yet been added; let's create a new instance

                if (!talentUpgradeEventAverageValueDictionary.ContainsKey(
                        talentUpgradeEventAverageValueToMerge.UpgradeEventType))
                {
                    talentUpgradeEventAverageValueDictionary[talentUpgradeEventAverageValueToMerge.UpgradeEventType] =
                        new Dictionary<decimal, TalentUpgradeEventAverageValue>();
                }

                talentUpgradeEventAverageValueDictionary[talentUpgradeEventAverageValueToMerge.UpgradeEventType]
                        [talentUpgradeEventAverageValueToMerge.AverageUpgradeEventValue] =
                    new TalentUpgradeEventAverageValue
                    {
                        UpgradeEventType = talentUpgradeEventAverageValueToMerge.UpgradeEventType,
                        AverageUpgradeEventValue = talentUpgradeEventAverageValueToMerge.AverageUpgradeEventValue,
                        GamesPlayed = talentUpgradeEventAverageValueToMerge.GamesPlayed,
                        WinPercent = talentUpgradeEventAverageValueToMerge.WinPercent,
                    };
            }
        }


        result.TalentUpgradeEventAverageValueArray = talentUpgradeEventAverageValueDictionary.Values
            .SelectMany(i => i.Values).OrderBy(i => i.UpgradeEventType).ThenBy(i => i.AverageUpgradeEventValue)
            .ToArray();

        return result;
    }
}