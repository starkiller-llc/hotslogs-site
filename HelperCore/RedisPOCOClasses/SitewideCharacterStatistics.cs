// Copyright (c) Starkiller LLC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideCharacterStatistics
{
    public SitewideCharacterStatistic[] SitewideCharacterStatisticArray { get; set; }
    public DateTime DateTimeBegin { get; set; }
    public DateTime DateTimeEnd { get; set; }
    public int League { get; set; }
    public int GameMode { get; set; }
    public DateTime LastUpdated { get; set; }

    public static SitewideCharacterStatistics operator +(
        SitewideCharacterStatistics a,
        SitewideCharacterStatistics b)
    {
        // Make sure two valid instances are provided
        if (a?.SitewideCharacterStatisticArray == null || a.SitewideCharacterStatisticArray.Length == 0)
        {
            return b ?? a;
        }

        if (b?.SitewideCharacterStatisticArray == null || b.SitewideCharacterStatisticArray.Length == 0)
        {
            return a ?? b;
        }

        // Create the result container based on 'a'
        var result = new SitewideCharacterStatistics
        {
            DateTimeBegin = a.DateTimeBegin,
            DateTimeEnd = a.DateTimeEnd,
            League = a.League,
            GameMode = a.GameMode,
            LastUpdated = a.LastUpdated,
        };

        // Create a dictionary of 'SitewideCharacterStatistic'
        var stats = new Dictionary<string, SitewideCharacterStatistic>();

        // Loop through 'a' and 'b', combining results
        var statsCollection = a.SitewideCharacterStatisticArray
            .Concat(b.SitewideCharacterStatisticArray)
            .Where(i => i.GamesPlayed > 0);
        foreach (var src in statsCollection)
        {
            if (stats.ContainsKey(src.Character))
            {
                // This Character has already been added; let's merge with the current results
                var dst = stats[src.Character];

                #region Local Helper Functions

                void MergeDecimal(Expression<Func<AverageTeamProfileReplayCharacterScoreResult, decimal>> expr)
                {
                    var nm = ((MemberExpression)expr.Body).Member.Name;
                    var prop = typeof(AverageTeamProfileReplayCharacterScoreResult).GetProperty(nm);
                    var za = (decimal)prop.GetValue(src.AverageScoreResult);
                    var zb = (decimal)prop.GetValue(dst.AverageScoreResult);
                    if (zb == 0)
                    {
                        zb = za;
                        prop.SetValue(dst.AverageScoreResult, zb);
                    }
                    else if (src.AverageScoreResult.T != 0)
                    {
                        zb = DataHelper.InterpolateDecimal(zb, za, dst.GamesPlayed, src.GamesPlayed);
                        prop.SetValue(dst.AverageScoreResult, zb);
                    }
                }

                void MergeInt(Expression<Func<AverageTeamProfileReplayCharacterScoreResult, int>> expr)
                {
                    var nm = ((MemberExpression)expr.Body).Member.Name;
                    var prop = typeof(AverageTeamProfileReplayCharacterScoreResult).GetProperty(nm);
                    var za = (int)prop.GetValue(src.AverageScoreResult);
                    var zb = (int)prop.GetValue(dst.AverageScoreResult);
                    if (zb == 0)
                    {
                        zb = za;
                        prop.SetValue(dst.AverageScoreResult, zb);
                    }
                    else if (za != 0)
                    {
                        zb = DataHelper.InterpolateInt(zb, za, dst.GamesPlayed, src.GamesPlayed);
                        prop.SetValue(dst.AverageScoreResult, zb);
                    }
                }

                void MergeNullableInt(Expression<Func<AverageTeamProfileReplayCharacterScoreResult, int?>> expr)
                {
                    var nm = ((MemberExpression)expr.Body).Member.Name;
                    var prop = typeof(AverageTeamProfileReplayCharacterScoreResult).GetProperty(nm);
                    var za = (int?)prop.GetValue(src.AverageScoreResult);
                    var zb = (int?)prop.GetValue(dst.AverageScoreResult);
                    if (!zb.HasValue)
                    {
                        zb = za;
                        prop.SetValue(dst.AverageScoreResult, zb);
                    }
                    else if (za.HasValue)
                    {
                        zb = DataHelper.InterpolateInt(zb.Value, za.Value, dst.GamesPlayed, src.GamesPlayed);
                        prop.SetValue(dst.AverageScoreResult, zb);
                    }
                }

                void MergeTimeSpan(Expression<Func<AverageTeamProfileReplayCharacterScoreResult, TimeSpan>> expr)
                {
                    var nm = ((MemberExpression)expr.Body).Member.Name;
                    var prop = typeof(AverageTeamProfileReplayCharacterScoreResult).GetProperty(nm);
                    var za = (TimeSpan)prop.GetValue(src.AverageScoreResult);
                    var zb = (TimeSpan)prop.GetValue(dst.AverageScoreResult);
                    if (zb == TimeSpan.Zero)
                    {
                        zb = za;
                        prop.SetValue(dst.AverageScoreResult, zb);
                    }
                    else if (za != TimeSpan.Zero)
                    {
                        zb = DataHelper.InterpolateTimeSpan(zb, za, dst.GamesPlayed, src.GamesPlayed);
                        prop.SetValue(dst.AverageScoreResult, zb);
                    }
                }

                void MergeNullableTimeSpan(
                    Expression<Func<AverageTeamProfileReplayCharacterScoreResult, TimeSpan?>> expr)
                {
                    var nm = ((MemberExpression)expr.Body).Member.Name;
                    var prop = typeof(AverageTeamProfileReplayCharacterScoreResult).GetProperty(nm);
                    var za = (TimeSpan?)prop.GetValue(src.AverageScoreResult);
                    var zb = (TimeSpan?)prop.GetValue(dst.AverageScoreResult);
                    if (!zb.HasValue)
                    {
                        zb = za;
                        prop.SetValue(dst.AverageScoreResult, zb);
                    }
                    else if (za.HasValue)
                    {
                        zb = DataHelper.InterpolateTimeSpan(zb.Value, za.Value, dst.GamesPlayed, src.GamesPlayed);
                        prop.SetValue(dst.AverageScoreResult, zb);
                    }
                }

                #endregion

                // Combine 'AverageScoreResult'
                if (src.AverageScoreResult != null)
                {
                    MergeDecimal(x => x.T); // Takedowns
                    MergeDecimal(x => x.S); // Solo Kills
                    MergeDecimal(x => x.A); // Assists
                    MergeDecimal(x => x.D); // Deaths
                    MergeInt(x => x.HD); // Hero Damage
                    MergeInt(x => x.SiD); // Siege Damage
                    MergeInt(x => x.StD); // Structure Damage
                    MergeInt(x => x.MD); // Minion Damage
                    MergeInt(x => x.CD); // Creep Damage
                    MergeInt(x => x.SuD); // Summon Damage
                    MergeNullableTimeSpan(x => x.TCCdEH); // Time CCd Enemy Heroes
                    MergeNullableInt(x => x.H); // Healing
                    MergeInt(x => x.SH); // Self Healing
                    MergeNullableInt(x => x.DT); // Damage Taken
                    MergeInt(x => x.EC); // Experience Contribution
                    MergeDecimal(x => x.TK); // Town Kills
                    MergeTimeSpan(x => x.TSD); // Time Spent Dead
                    MergeDecimal(x => x.MCC); // Merc Camp Captures
                    MergeDecimal(x => x.WTC); // Watch Tower Captures
                    MergeInt(x => x.ME); // Meta Experience
                }

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

                // Combine 'GamesBanned'
                dst.GamesBanned += src.GamesBanned;

                // Combine 'WinPercent'
                dst.WinPercent = combinedGamesWon / dst.GamesPlayed;
            }
            else
            {
                // This Character has not yet been added; let's create a new instance
                stats[src.Character] = new SitewideCharacterStatistic
                {
                    HeroPortraitURL = src.HeroPortraitURL,
                    Character = src.Character,
                    GamesPlayed = src.GamesPlayed,
                    GamesBanned = src.GamesBanned,
                    AverageLength = src.AverageLength,
                    WinPercent = src.WinPercent,
                    AverageScoreResult = src.AverageScoreResult ?? new AverageTeamProfileReplayCharacterScoreResult(),
                };
            }
        }

        result.SitewideCharacterStatisticArray = stats.Select(i => i.Value).ToArray();

        return result;
    }
}