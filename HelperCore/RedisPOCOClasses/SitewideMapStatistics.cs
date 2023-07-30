using System;
using System.Collections.Generic;
using System.Linq;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideMapStatistics
{
    public SitewideMapStatistic[] SitewideMapStatisticArray { get; set; }
    public DateTime DateTimeBegin { get; set; }
    public DateTime DateTimeEnd { get; set; }
    public int League { get; set; }
    public int GameMode { get; set; }
    public DateTime LastUpdated { get; set; }

    public static SitewideMapStatistics operator +(SitewideMapStatistics a, SitewideMapStatistics b)
    {
        // Make sure two valid instances are provided
        if (a == null || a.SitewideMapStatisticArray == null || a.SitewideMapStatisticArray.Length == 0)
        {
            return b;
        }

        if (b == null || b.SitewideMapStatisticArray == null || b.SitewideMapStatisticArray.Length == 0)
        {
            return a;
        }

        // Create the result container based on 'a'
        var result = new SitewideMapStatistics
        {
            DateTimeBegin = a.DateTimeBegin,
            DateTimeEnd = a.DateTimeEnd,
            League = a.League,
            GameMode = a.GameMode,
            LastUpdated = a.LastUpdated,
        };

        // Create a dictionary of 'SitewideMapStatistic'
        var sitewideMapStatisticResultDictionary = new Dictionary<string, SitewideMapStatistic>();

        // Loop through 'a' and 'b', combining results
        foreach (var sitewideMapStatisticToMerge in a.SitewideMapStatisticArray.Concat(b.SitewideMapStatisticArray)
                     .Where(i => i.GamesPlayed > 0))
        {
            if (sitewideMapStatisticResultDictionary.ContainsKey(
                    sitewideMapStatisticToMerge.Map + ":" + sitewideMapStatisticToMerge.Character))
            {
                // This Map and Character has already been added; let's merge with the current results
                var sitewideMapStatisticResult = sitewideMapStatisticResultDictionary[sitewideMapStatisticToMerge.Map +
                    ":" + sitewideMapStatisticToMerge.Character];

                // Combine 'AverageScoreResult'
                if (sitewideMapStatisticToMerge.AverageScoreResult != null)
                {
                    // Takedowns
                    if (sitewideMapStatisticResult.AverageScoreResult.T == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.T =
                            sitewideMapStatisticToMerge.AverageScoreResult.T;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.T != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.T = DataHelper.InterpolateDecimal(
                            sitewideMapStatisticResult.AverageScoreResult.T,
                            sitewideMapStatisticToMerge.AverageScoreResult.T,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Solo Kills
                    if (sitewideMapStatisticResult.AverageScoreResult.S == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.S =
                            sitewideMapStatisticToMerge.AverageScoreResult.S;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.S != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.S = DataHelper.InterpolateDecimal(
                            sitewideMapStatisticResult.AverageScoreResult.S,
                            sitewideMapStatisticToMerge.AverageScoreResult.S,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Assists
                    if (sitewideMapStatisticResult.AverageScoreResult.A == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.A =
                            sitewideMapStatisticToMerge.AverageScoreResult.A;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.A != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.A = DataHelper.InterpolateDecimal(
                            sitewideMapStatisticResult.AverageScoreResult.A,
                            sitewideMapStatisticToMerge.AverageScoreResult.A,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Deaths
                    if (sitewideMapStatisticResult.AverageScoreResult.D == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.D =
                            sitewideMapStatisticToMerge.AverageScoreResult.D;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.D != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.D = DataHelper.InterpolateDecimal(
                            sitewideMapStatisticResult.AverageScoreResult.D,
                            sitewideMapStatisticToMerge.AverageScoreResult.D,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Hero Damage
                    if (sitewideMapStatisticResult.AverageScoreResult.HD == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.HD =
                            sitewideMapStatisticToMerge.AverageScoreResult.HD;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.HD != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.HD = DataHelper.InterpolateInt(
                            sitewideMapStatisticResult.AverageScoreResult.HD,
                            sitewideMapStatisticToMerge.AverageScoreResult.HD,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Siege Damage
                    if (sitewideMapStatisticResult.AverageScoreResult.SiD == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.SiD =
                            sitewideMapStatisticToMerge.AverageScoreResult.SiD;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.SiD != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.SiD = DataHelper.InterpolateInt(
                            sitewideMapStatisticResult.AverageScoreResult.SiD,
                            sitewideMapStatisticToMerge.AverageScoreResult.SiD,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Structure Damage
                    if (sitewideMapStatisticResult.AverageScoreResult.StD == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.StD =
                            sitewideMapStatisticToMerge.AverageScoreResult.StD;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.StD != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.StD = DataHelper.InterpolateInt(
                            sitewideMapStatisticResult.AverageScoreResult.StD,
                            sitewideMapStatisticToMerge.AverageScoreResult.StD,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Minion Damage
                    if (sitewideMapStatisticResult.AverageScoreResult.MD == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.MD =
                            sitewideMapStatisticToMerge.AverageScoreResult.MD;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.MD != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.MD = DataHelper.InterpolateInt(
                            sitewideMapStatisticResult.AverageScoreResult.MD,
                            sitewideMapStatisticToMerge.AverageScoreResult.MD,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Creep Damage
                    if (sitewideMapStatisticResult.AverageScoreResult.CD == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.CD =
                            sitewideMapStatisticToMerge.AverageScoreResult.CD;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.CD != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.CD = DataHelper.InterpolateInt(
                            sitewideMapStatisticResult.AverageScoreResult.CD,
                            sitewideMapStatisticToMerge.AverageScoreResult.CD,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Summon Damage
                    if (sitewideMapStatisticResult.AverageScoreResult.SuD == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.SuD =
                            sitewideMapStatisticToMerge.AverageScoreResult.SuD;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.SuD != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.SuD = DataHelper.InterpolateInt(
                            sitewideMapStatisticResult.AverageScoreResult.SuD,
                            sitewideMapStatisticToMerge.AverageScoreResult.SuD,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Time CCd Enemy Heroes
                    if (!sitewideMapStatisticResult.AverageScoreResult.TCCdEH.HasValue)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.TCCdEH =
                            sitewideMapStatisticToMerge.AverageScoreResult.TCCdEH;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.TCCdEH.HasValue)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.TCCdEH = DataHelper.InterpolateTimeSpan(
                            sitewideMapStatisticResult.AverageScoreResult.TCCdEH.Value,
                            sitewideMapStatisticToMerge.AverageScoreResult.TCCdEH.Value,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Healing
                    if (!sitewideMapStatisticResult.AverageScoreResult.H.HasValue)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.H =
                            sitewideMapStatisticToMerge.AverageScoreResult.H;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.H.HasValue)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.H = DataHelper.InterpolateInt(
                            sitewideMapStatisticResult.AverageScoreResult.H.Value,
                            sitewideMapStatisticToMerge.AverageScoreResult.H.Value,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Self Healing
                    if (sitewideMapStatisticResult.AverageScoreResult.SH == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.SH =
                            sitewideMapStatisticToMerge.AverageScoreResult.SH;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.SH != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.SH = DataHelper.InterpolateInt(
                            sitewideMapStatisticResult.AverageScoreResult.SH,
                            sitewideMapStatisticToMerge.AverageScoreResult.SH,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Damage Taken
                    if (!sitewideMapStatisticResult.AverageScoreResult.DT.HasValue)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.DT =
                            sitewideMapStatisticToMerge.AverageScoreResult.DT;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.DT.HasValue)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.DT = DataHelper.InterpolateInt(
                            sitewideMapStatisticResult.AverageScoreResult.DT.Value,
                            sitewideMapStatisticToMerge.AverageScoreResult.DT.Value,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Experience Contribution
                    if (sitewideMapStatisticResult.AverageScoreResult.EC == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.EC =
                            sitewideMapStatisticToMerge.AverageScoreResult.EC;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.EC != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.EC = DataHelper.InterpolateInt(
                            sitewideMapStatisticResult.AverageScoreResult.EC,
                            sitewideMapStatisticToMerge.AverageScoreResult.EC,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Town Kills
                    if (sitewideMapStatisticResult.AverageScoreResult.TK == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.TK =
                            sitewideMapStatisticToMerge.AverageScoreResult.TK;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.TK != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.TK = DataHelper.InterpolateDecimal(
                            sitewideMapStatisticResult.AverageScoreResult.TK,
                            sitewideMapStatisticToMerge.AverageScoreResult.TK,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Time Spent Dead
                    if (sitewideMapStatisticResult.AverageScoreResult.TSD == TimeSpan.Zero)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.TSD =
                            sitewideMapStatisticToMerge.AverageScoreResult.TSD;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.TSD != TimeSpan.Zero)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.TSD = DataHelper.InterpolateTimeSpan(
                            sitewideMapStatisticResult.AverageScoreResult.TSD,
                            sitewideMapStatisticToMerge.AverageScoreResult.TSD,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Merc Camp Captures
                    if (sitewideMapStatisticResult.AverageScoreResult.MCC == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.MCC =
                            sitewideMapStatisticToMerge.AverageScoreResult.MCC;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.MCC != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.MCC = DataHelper.InterpolateDecimal(
                            sitewideMapStatisticResult.AverageScoreResult.MCC,
                            sitewideMapStatisticToMerge.AverageScoreResult.MCC,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Watch Tower Captures
                    if (sitewideMapStatisticResult.AverageScoreResult.WTC == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.WTC =
                            sitewideMapStatisticToMerge.AverageScoreResult.WTC;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.WTC != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.WTC = DataHelper.InterpolateDecimal(
                            sitewideMapStatisticResult.AverageScoreResult.WTC,
                            sitewideMapStatisticToMerge.AverageScoreResult.WTC,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }

                    // Meta Experience
                    if (sitewideMapStatisticResult.AverageScoreResult.ME == 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.ME =
                            sitewideMapStatisticToMerge.AverageScoreResult.ME;
                    }
                    else if (sitewideMapStatisticToMerge.AverageScoreResult.ME != 0)
                    {
                        sitewideMapStatisticResult.AverageScoreResult.ME = DataHelper.InterpolateInt(
                            sitewideMapStatisticResult.AverageScoreResult.ME,
                            sitewideMapStatisticToMerge.AverageScoreResult.ME,
                            sitewideMapStatisticResult.GamesPlayed,
                            sitewideMapStatisticToMerge.GamesPlayed);
                    }
                }

                // Combine 'AverageLength'
                sitewideMapStatisticResult.AverageLength = DataHelper.InterpolateTimeSpan(
                    sitewideMapStatisticResult.AverageLength,
                    sitewideMapStatisticToMerge.AverageLength,
                    sitewideMapStatisticResult.GamesPlayed,
                    sitewideMapStatisticToMerge.GamesPlayed);

                // Combine 'GamesWon'
                var combinedGamesWon =
                    (sitewideMapStatisticToMerge.GamesPlayed * sitewideMapStatisticToMerge.WinPercent) +
                    (sitewideMapStatisticResult.GamesPlayed * sitewideMapStatisticResult.WinPercent);

                // Combine 'GamesPlayed'
                sitewideMapStatisticResult.GamesPlayed += sitewideMapStatisticToMerge.GamesPlayed;

                // Combine 'GamesBanned'
                sitewideMapStatisticResult.GamesBanned += sitewideMapStatisticToMerge.GamesBanned;

                // Combine 'WinPercent'
                sitewideMapStatisticResult.WinPercent = combinedGamesWon / sitewideMapStatisticResult.GamesPlayed;
            }
            else
                // This Map and Character has not yet been added; let's create a new instance
            {
                sitewideMapStatisticResultDictionary
                        [sitewideMapStatisticToMerge.Map + ":" + sitewideMapStatisticToMerge.Character] =
                    new SitewideMapStatistic
                    {
                        Map = sitewideMapStatisticToMerge.Map,
                        Character = sitewideMapStatisticToMerge.Character,
                        GamesPlayed = sitewideMapStatisticToMerge.GamesPlayed,
                        GamesBanned = sitewideMapStatisticToMerge.GamesBanned,
                        AverageLength = sitewideMapStatisticToMerge.AverageLength,
                        WinPercent = sitewideMapStatisticToMerge.WinPercent,
                        AverageScoreResult = sitewideMapStatisticToMerge.AverageScoreResult ??
                                             new AverageTeamProfileReplayCharacterScoreResult(),
                    };
            }
        }

        result.SitewideMapStatisticArray = sitewideMapStatisticResultDictionary.Select(i => i.Value).ToArray();

        return result;
    }
}