using System;
using System.Collections.Generic;
using System.Linq;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideHeroTalentStatistics
{
    public SitewideHeroTalentStatistic[] SitewideHeroTalentStatisticArray { get; set; }
    public HeroTalentBuildStatistic[] HeroTalentBuildStatisticArray { get; set; }
    public DateTime DateTimeBegin { get; set; }
    public DateTime DateTimeEnd { get; set; }
    public int ReplayBuild { get; set; }
    public string Character { get; set; }
    public string MapName { get; set; }
    public int League { get; set; }
    public int GameMode { get; set; }
    public DateTime LastUpdated { get; set; }

    public static SitewideHeroTalentStatistics operator +(
        SitewideHeroTalentStatistics a,
        SitewideHeroTalentStatistics b)
    {
        // Make sure two valid instances are provided
        if (a == null)
        {
            return b;
        }

        if (b == null)
        {
            return a;
        }

        // Create the result container based on 'a'
        var result = new SitewideHeroTalentStatistics
        {
            DateTimeBegin = a.DateTimeBegin,
            DateTimeEnd = a.DateTimeEnd,
            ReplayBuild = a.ReplayBuild,
            Character = a.Character,
            MapName = a.MapName,
            League = a.League,
            GameMode = a.GameMode,
            LastUpdated = a.LastUpdated,
        };

        // Create a dictionary of 'SitewideHeroTalentStatistic'
        {
            var sitewideHeroTalentStatisticResultDictionary = new Dictionary<string, SitewideHeroTalentStatistic>();

            // Loop through 'a' and 'b', combining results
            foreach (var sitewideHeroTalentStatisticToMerge in a.SitewideHeroTalentStatisticArray
                         .Concat(b.SitewideHeroTalentStatisticArray).Where(i => i.GamesPlayed > 0))
            {
                if (sitewideHeroTalentStatisticResultDictionary.ContainsKey(
                        sitewideHeroTalentStatisticToMerge.TalentName))
                {
                    // This TalentID has already been added; let's merge with the current results
                    var sitewideHeroTalentStatisticResult =
                        sitewideHeroTalentStatisticResultDictionary[sitewideHeroTalentStatisticToMerge.TalentName];

                    // Combine 'GamesWon'
                    var combinedGamesWon =
                        (sitewideHeroTalentStatisticToMerge.GamesPlayed *
                         sitewideHeroTalentStatisticToMerge.WinPercent) +
                        (sitewideHeroTalentStatisticResult.GamesPlayed * sitewideHeroTalentStatisticResult.WinPercent);

                    // Combine 'GamesPlayed'
                    sitewideHeroTalentStatisticResult.GamesPlayed += sitewideHeroTalentStatisticToMerge.GamesPlayed;

                    // Combine 'WinPercent'
                    sitewideHeroTalentStatisticResult.WinPercent =
                        combinedGamesWon / sitewideHeroTalentStatisticResult.GamesPlayed;
                }
                else
                    // This TalentID has not yet been added; let's create a new instance
                {
                    sitewideHeroTalentStatisticResultDictionary[sitewideHeroTalentStatisticToMerge.TalentName] =
                        new SitewideHeroTalentStatistic
                        {
                            Character = sitewideHeroTalentStatisticToMerge.Character,
                            TalentTier = sitewideHeroTalentStatisticToMerge.TalentTier,
                            TalentID = sitewideHeroTalentStatisticToMerge.TalentID,
                            TalentName = sitewideHeroTalentStatisticToMerge.TalentName,
                            TalentDescription = sitewideHeroTalentStatisticToMerge.TalentDescription,
                            GamesPlayed = sitewideHeroTalentStatisticToMerge.GamesPlayed,
                            WinPercent = sitewideHeroTalentStatisticToMerge.WinPercent,
                        };
                }
            }

            result.SitewideHeroTalentStatisticArray =
                sitewideHeroTalentStatisticResultDictionary.Select(i => i.Value).ToArray();
        }

        // Create a dictionary of 'HeroTalentBuildStatistic'
        {
            var heroTalentBuildStatisticResultDictionary = new Dictionary<string, HeroTalentBuildStatistic>();

            // Loop through 'a' and 'b', combining results
            foreach (var heroTalentBuildStatisticToMerge in a.HeroTalentBuildStatisticArray
                         .Concat(b.HeroTalentBuildStatisticArray).Where(i => i.GamesPlayed > 0))
            {
                var dictionaryKey =
                    string.Join(string.Empty, heroTalentBuildStatisticToMerge.TalentNameDescription);

                if (heroTalentBuildStatisticResultDictionary.ContainsKey(dictionaryKey))
                {
                    // This entry has already been added; let's merge with the current results
                    var heroTalentBuildStatisticResult = heroTalentBuildStatisticResultDictionary[dictionaryKey];

                    // Combine 'GamesWon'
                    var combinedGamesWon =
                        (heroTalentBuildStatisticToMerge.GamesPlayed * heroTalentBuildStatisticToMerge.WinPercent) +
                        (heroTalentBuildStatisticResult.GamesPlayed * heroTalentBuildStatisticResult.WinPercent);

                    // Combine 'GamesPlayed'
                    heroTalentBuildStatisticResult.GamesPlayed += heroTalentBuildStatisticToMerge.GamesPlayed;

                    // Combine 'WinPercent'
                    heroTalentBuildStatisticResult.WinPercent =
                        combinedGamesWon / heroTalentBuildStatisticResult.GamesPlayed;
                }
                else
                {
                    // This entry has not yet been added; let's create a new instance
                    heroTalentBuildStatisticResultDictionary[dictionaryKey] = new HeroTalentBuildStatistic
                    {
                        GamesPlayed = heroTalentBuildStatisticToMerge.GamesPlayed,
                        WinPercent = heroTalentBuildStatisticToMerge.WinPercent,
                        TalentNameDescription = Clone(heroTalentBuildStatisticToMerge.TalentNameDescription),
                        TalentName = Clone(heroTalentBuildStatisticToMerge.TalentName),
                        TalentImageURL = Clone(heroTalentBuildStatisticToMerge.TalentImageURL),
                    };
                }
            }

            result.HeroTalentBuildStatisticArray =
                heroTalentBuildStatisticResultDictionary.Select(i => i.Value).ToArray();
        }

        return result;
    }

    private static string[] Clone(string[] arr)
    {
        var rc = new string[7];
        for (var i = 0; i < 7; i++)
        {
            rc[i] = arr[i];
        }

        return rc;
    }
}