// Copyright (c) Starkiller LLC. All rights reserved.
// ReSharper disable InconsistentNaming

using HelperCore.RedisPOCOClasses;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.ScoreResults.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable SA1306

namespace HotsLogsApi.BL.Migration.ScoreResults;

internal class ScoreHelper
{
    private readonly ConcurrentDictionary<string, string> roleDic;

    private TeamProfile _teamProfile;
    private decimal? assistMax;
    private decimal? assistMin;
    private decimal? creepDamageMax;
    private decimal? creepDamageMin;
    private decimal? damageTakenMax;
    private decimal? damageTakenMin;
    private decimal? deathMax;
    private decimal? deathMin;
    private int? experienceContributionMax;
    private int? experienceContributionMin;

    private decimal gamesPlayedMax;
    private decimal? healingMax;
    private decimal? healingMin;
    private int? heroDamageMax;
    private int? heroDamageMin;
    private decimal? kdMax;
    private decimal? kdMin;
    private decimal? mccMax;
    private decimal? mccMin;
    private decimal? minionDamageMax;
    private decimal? minionDamageMin;
    private Dictionary<int, TeamProfileReplay> replaysDictionary;
    private int? selfHealingMax;
    private int? selfHealingMin;
    private int? siegeDamageMax;
    private int? siegeDamageMin;
    private decimal? soloKillMax;
    private decimal? soloKillMin;
    private decimal? structureDamageMax;
    private decimal? structureDamageMin;
    private decimal? takedownMax;
    private decimal? takedownMin;
    private decimal? tdMax;
    private decimal? tdMin;
    private decimal? townKillMax;
    private decimal? townKillMin;
    private decimal winPercentMax;
    private decimal winPercentMin;

    public ScoreHelper()
    {
        roleDic = Global.GetHeroRoleConcurrentDictionary();
    }

    public TeamProfile TeamProfile
    {
        get => _teamProfile;
        set
        {
            _teamProfile = value;
            replaysDictionary = _teamProfile.Replays.ToDictionary(i => i.RID, i => i);
        }
    }

    public IEnumerable<StatsResultType> CalcGeneric(IEnumerable<SitewideCharacterStatistic> stats)
    {
        var colorDic = TeamCompHelper.HeroRoleColorsDictionary;
        var result = stats.Select(
            i =>
            {
                var selfHeal = i.AverageScoreResult.SH == 0 ? (int?)null : i.AverageScoreResult.SH;
                return new StatsResultType
                {
                    Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                    CharacterURL = i.Character,
                    HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                    GamesPlayed = SiteMaster.GetGaugeHtml(i.GamesPlayed, 0, gamesPlayedMax, colorDic["Support"], "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                    AverageLength = i.AverageLength,
                    KDRatio = SiteMaster.GetGaugeHtml(
                        (decimal?)i.AverageScoreResult.S / (i.AverageScoreResult.D > 0 ? i.AverageScoreResult.D : 1),
                        kdMin,
                        kdMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    Assists = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.A,
                        assistMin,
                        assistMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    TDRatio = SiteMaster.GetGaugeHtml(
                        (decimal?)i.AverageScoreResult.T / (i.AverageScoreResult.D > 0 ? i.AverageScoreResult.D : 1),
                        tdMin,
                        tdMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    Takedowns = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.T,
                        takedownMin,
                        takedownMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    Deaths = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.D,
                        deathMin,
                        deathMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    HeroDamage = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.HD,
                        heroDamageMin,
                        heroDamageMax,
                        colorDic["Ranged Assassin"],
                        "N0"),
                    SiegeDamage = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.SiD,
                        siegeDamageMin,
                        siegeDamageMax,
                        colorDic["Ranged Assassin"],
                        "N0"),
                    StructureDamage = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.StD,
                        structureDamageMin,
                        structureDamageMax,
                        colorDic["Ranged Assassin"],
                        "N0"),
                    MinionDamage = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.MD,
                        minionDamageMin,
                        minionDamageMax,
                        colorDic["Ranged Assassin"],
                        "N0"),
                    CreepDamage = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.CD,
                        creepDamageMin,
                        creepDamageMax,
                        colorDic["Ranged Assassin"],
                        "N0"),
                    SelfHealing = SiteMaster.GetGaugeHtml(
                        selfHeal,
                        selfHealingMin,
                        selfHealingMax,
                        colorDic["Support"],
                        "N0"),
                    DamageTaken = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.DT,
                        damageTakenMin,
                        damageTakenMax,
                        colorDic["Tank"],
                        "N0"),
                    ExperienceContribution = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.EC,
                        experienceContributionMin,
                        experienceContributionMax,
                        colorDic["Support"],
                        "N0"),
                    MercCampCaptures = i.AverageScoreResult.MCC,
                    Healing = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.H,
                        healingMin,
                        healingMax,
                        colorDic["Healer"],
                        "N0"),
                    SoloKills = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.S,
                        soloKillMin,
                        soloKillMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    TownKills = SiteMaster.GetGaugeHtml(
                        i.AverageScoreResult.TK,
                        townKillMin,
                        townKillMax,
                        colorDic["Support"],
                        "F1"),
                };
            });
        return result;
    }

    public StatsResultType[] CalcStatsGeneric(SitewideCharacterStatistic[] stats)
    {
        CalcStep1(stats);

        var orderedStats = stats.OrderByDescending(i => i.AverageScoreResult.HD);
        var result = CalcGeneric(orderedStats);

        return result.ToArray();
    }

    public TeamStatsResultType[] CalcStatsGenericTeam(IDictionary<int, TeamProfileReplayCharacter[]> stats)
    {
        CalcStep1Team(stats);

        var result = CalcGenericTeam(stats);

        return result.ToArray();
    }

    public StatsResultType[] CalcStatsRole(
        SitewideCharacterStatistic[] stats,
        Func<StatsResultType, decimal> sortKey)
    {
        CalcStep1(stats);

        var result = CalcGeneric(stats);

        return result.OrderByDescending(sortKey).ToArray();
    }

    public TeamStatsResultType[] CalcStatsRoleTeam(
        IGrouping<int, TeamProfileReplayCharacter>[] stats1,
        Func<TeamStatsResultType, string> sortKey)
    {
        var stats = stats1.ToDictionary(x => x.Key, x => x.ToArray());
        CalcStep1Team(stats);

        var result = CalcGenericTeam(stats);

        return result.OrderByDescending(sortKey).ToArray();
    }

    private IEnumerable<TeamStatsResultType> CalcGenericTeam(IDictionary<int, TeamProfileReplayCharacter[]> stats)
    {
        return stats.Select(
            i =>
            {
                var colorDic = TeamCompHelper.HeroRoleColorsDictionary;
                return new TeamStatsResultType
                {
                    PlayerID = TeamProfile.Players[i.Key].IsLOO == false ? i.Key : null,
                    PlayerName = TeamProfile.Players[i.Key].PN,
                    GamesPlayed = SiteMaster.GetGaugeHtml(
                        i.Value.Length,
                        0,
                        gamesPlayedMax,
                        colorDic["Support"],
                        "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(
                        (decimal)i.Value.Count(j => j.IsW) / i.Value.Length,
                        winPercentMin,
                        winPercentMax),
                    AverageLength = TimeSpan.FromSeconds(
                        (int)i.Value.Average(j => replaysDictionary[j.RID].RL.TotalSeconds)),
                    FavoriteHeroes = i.Value
                        .GroupBy(j => j.C)
                        .OrderByDescending(j => j.Count())
                        .Take(3)
                        .Select(
                            j => new FavoriteHeroRow
                            {
                                CharacterURL = j.Key,
                                Character = SiteMaster.GetLocalizedString("GenericHero", j.Key),
                                GamesPlayed = j.Count(),
                            })
                        .ToArray(),
                    TDRatio = SiteMaster.GetGaugeHtml(
                        (decimal?)i.Value.Sum(j => j.SR?.T) /
                        (i.Value.Sum(j => j.SR?.D) > 0 ? i.Value.Sum(j => j.SR?.D) : 1),
                        tdMin,
                        tdMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    Takedowns = SiteMaster.GetGaugeHtml(
                        (decimal?)i.Value.Average(j => j.SR?.T),
                        takedownMin,
                        takedownMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    SoloKills = SiteMaster.GetGaugeHtml(
                        (decimal?)i.Value.Average(j => j.SR?.S),
                        soloKillMin,
                        soloKillMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    Deaths = SiteMaster.GetGaugeHtml(
                        (decimal?)i.Value.Average(j => j.SR?.D),
                        deathMin,
                        deathMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    HeroDamage = SiteMaster.GetGaugeHtml(
                        (int?)i.Value.Average(j => j.SR?.HD),
                        heroDamageMin,
                        heroDamageMax,
                        colorDic["Ranged Assassin"],
                        "N0"),
                    SiegeDamage = SiteMaster.GetGaugeHtml(
                        (int?)i.Value.Average(j => j.SR?.SiD),
                        siegeDamageMin,
                        siegeDamageMax,
                        colorDic["Ranged Assassin"],
                        "N0"),
                    Healing = SiteMaster.GetGaugeHtml(
                        (int?)i.Value.Average(j => j.SR != null && j.SR.H > 0 ? j.SR.H : null),
                        healingMin,
                        healingMax,
                        colorDic["Healer"],
                        "N0"),
                    SelfHealing = SiteMaster.GetGaugeHtml(
                        (int?)i.Value.Average(j => j.SR != null && j.SR.SH > 0 ? j.SR.SH : null),
                        selfHealingMin,
                        selfHealingMax,
                        colorDic["Support"],
                        "N0"),
                    DamageTaken = SiteMaster.GetGaugeHtml(
                        (int?)((decimal?)i.Value.Sum(GetDt) / (i.Value.Sum(GetD) > 0 ? i.Value.Sum(GetD) : 1)),
                        damageTakenMin,
                        damageTakenMax,
                        colorDic["Tank"],
                        "N0"),
                    // MercCampCaptures = i.Value.Average(j => j.SR != null ? (decimal?)j.SR.MCC : null),
                    ExperienceContribution = SiteMaster.GetGaugeHtml(
                        (int?)i.Value.Average(j => j.SR?.EC),
                        experienceContributionMin,
                        experienceContributionMax,
                        colorDic["Support"],
                        "N0"),
                    KDRatio = SiteMaster.GetGaugeHtml(
                        (decimal?)i.Value.Sum(j => j.SR?.S) /
                        (i.Value.Sum(j => j.SR?.D) > 0 ? i.Value.Sum(j => j.SR?.D) : 1),
                        kdMin,
                        kdMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    Assists = SiteMaster.GetGaugeHtml(
                        (decimal?)i.Value.Average(j => j.SR?.A),
                        assistMin,
                        assistMax,
                        colorDic["Ranged Assassin"],
                        "F1"),
                    StructureDamage = SiteMaster.GetGaugeHtml(
                        (int?)i.Value.Average(j => j.SR?.StD),
                        structureDamageMin,
                        structureDamageMax,
                        colorDic["Ranged Assassin"],
                        "N0"),
                    MinionDamage = SiteMaster.GetGaugeHtml(
                        (int?)i.Value.Average(j => j.SR?.MD),
                        minionDamageMin,
                        minionDamageMax,
                        colorDic["Ranged Assassin"],
                        "N0"),
                    CreepDamage = SiteMaster.GetGaugeHtml(
                        (int?)i.Value.Average(j => j.SR?.CD),
                        creepDamageMin,
                        creepDamageMax,
                        colorDic["Ranged Assassin"],
                        "N0"),
                    TownKills = SiteMaster.GetGaugeHtml(
                        i.Value.Average(j => j.SR != null ? (decimal?)j.SR.TK : null),
                        townKillMin,
                        townKillMax,
                        colorDic["Support"],
                        "F1"),
                    MercCampCaptures = SiteMaster.GetGaugeHtml(
                        i.Value.Average(j => j.SR != null ? (decimal?)j.SR.MCC : null),
                        mccMin,
                        mccMax,
                        colorDic["Support"],
                        "F1"),
                };
            }).OrderByDescending(i => i.HeroDamage);
    }

    private void CalcStep1(SitewideCharacterStatistic[] stats)
    {
        gamesPlayedMax = stats.Max(i => i.GamesPlayed);

        tdMin = stats.Min(i => i.AverageScoreResult.T / (i.AverageScoreResult.D > 0 ? i.AverageScoreResult.D : 1));
        tdMax = stats.Max(i => i.AverageScoreResult.T / (i.AverageScoreResult.D > 0 ? i.AverageScoreResult.D : 1));

        kdMin = stats.Min(i => i.AverageScoreResult.S / (i.AverageScoreResult.D > 0 ? i.AverageScoreResult.D : 1));
        kdMax = stats.Max(i => i.AverageScoreResult.S / (i.AverageScoreResult.D > 0 ? i.AverageScoreResult.D : 1));

        assistMin = stats.Min(i => i.AverageScoreResult.A);
        assistMax = stats.Max(i => i.AverageScoreResult.A);

        takedownMin = stats.Min(i => i.AverageScoreResult.T);
        takedownMax = stats.Max(i => i.AverageScoreResult.T);

        soloKillMin = stats.Min(i => i.AverageScoreResult.S);
        soloKillMax = stats.Max(i => i.AverageScoreResult.S);

        deathMin = stats.Min(i => i.AverageScoreResult.D);
        deathMax = stats.Max(i => i.AverageScoreResult.D);

        winPercentMin = stats.Min(i => i.WinPercent);
        winPercentMax = stats.Max(i => i.WinPercent);

        heroDamageMin = stats.Min(i => i.AverageScoreResult.HD);
        heroDamageMax = stats.Max(i => i.AverageScoreResult.HD);

        siegeDamageMin = stats.Min(i => i.AverageScoreResult.SiD);
        siegeDamageMax = stats.Max(i => i.AverageScoreResult.SiD);

        structureDamageMin = stats.Min(i => i.AverageScoreResult.StD);
        structureDamageMax = stats.Max(i => i.AverageScoreResult.StD);

        minionDamageMin = stats.Min(i => i.AverageScoreResult.MD);
        minionDamageMax = stats.Max(i => i.AverageScoreResult.MD);

        creepDamageMin = stats.Min(i => i.AverageScoreResult.CD);
        creepDamageMax = stats.Max(i => i.AverageScoreResult.CD);

        healingMin = stats.Min(i => i.AverageScoreResult.H);
        healingMax = stats.Max(i => i.AverageScoreResult.H);

        selfHealingMin = stats.Min(i => i.AverageScoreResult.SH);
        selfHealingMax = stats.Max(i => i.AverageScoreResult.SH);

        damageTakenMin = stats.Min(i => i.AverageScoreResult.DT);
        damageTakenMax = stats.Max(i => i.AverageScoreResult.DT);

        experienceContributionMin = stats.Min(i => i.AverageScoreResult.EC);
        experienceContributionMax = stats.Max(i => i.AverageScoreResult.EC);

        townKillMin = stats.Min(i => i.AverageScoreResult.TK);
        townKillMax = stats.Max(i => i.AverageScoreResult.TK);
    }

    private void CalcStep1Team(IDictionary<int, TeamProfileReplayCharacter[]> stats)
    {
        gamesPlayedMax = stats.Max(i => i.Value.Length);

        tdMin = stats.Min(
            i =>
            {
                var sumD = i.Value.Sum(j => j.SR?.D);
                var sumT = i.Value.Sum(j => j.SR?.T);
                return (decimal?)sumT / (sumD > 0 ? sumD : 1);
            });
        tdMax = stats.Max(
            i =>
            {
                var sumD = i.Value.Sum(j => j.SR?.D);
                var sumT = i.Value.Sum(j => j.SR?.T);
                return (decimal?)sumT / (sumD > 0 ? sumD : 1);
            });

        takedownMin = stats.Min(i => (decimal?)i.Value.Average(j => j.SR?.T));
        takedownMax = stats.Max(i => (decimal?)i.Value.Average(j => j.SR?.T));

        soloKillMin = stats.Min(i => (decimal?)i.Value.Average(j => j.SR?.S));
        soloKillMax = stats.Max(i => (decimal?)i.Value.Average(j => j.SR?.S));

        deathMin = stats.Min(i => (decimal?)i.Value.Average(j => j.SR?.D));
        deathMax = stats.Max(i => (decimal?)i.Value.Average(j => j.SR?.D));

        winPercentMin = stats.Min(i => (decimal)i.Value.Count(j => j.IsW) / i.Value.Length);
        winPercentMax = stats.Max(i => (decimal)i.Value.Count(j => j.IsW) / i.Value.Length);

        heroDamageMin = stats.Min(i => (int?)i.Value.Average(j => j.SR?.HD));
        heroDamageMax = stats.Max(i => (int?)i.Value.Average(j => j.SR?.HD));

        siegeDamageMin = stats.Min(i => (int?)i.Value.Average(j => j.SR?.SiD));
        siegeDamageMax = stats.Max(i => (int?)i.Value.Average(j => j.SR?.SiD));

        healingMin = stats.Min(i => (int?)i.Value.Average(j => j.SR != null && j.SR.H > 0 ? j.SR.H : null));
        healingMax = stats.Max(i => (int?)i.Value.Average(j => j.SR != null && j.SR.H > 0 ? j.SR.H : null));

        selfHealingMin = stats.Min(i => (int?)i.Value.Average(j => j.SR != null && j.SR.SH > 0 ? j.SR.SH : null));
        selfHealingMax = stats.Max(i => (int?)i.Value.Average(j => j.SR != null && j.SR.SH > 0 ? j.SR.SH : null));

        damageTakenMin = stats.Min(
            i =>
            {
                var sumD = i.Value.Sum(GetD);
                var sumDT = i.Value.Sum(GetDt);
                return (int?)((decimal?)sumDT / (sumD > 0 ? sumD : 1));
            });
        damageTakenMax = stats.Max(
            i =>
            {
                var sumD = i.Value.Sum(GetD);
                var sumDT = i.Value.Sum(GetDt);
                return (int?)((decimal?)sumDT / (sumD > 0 ? sumD : 1));
            });

        experienceContributionMin = stats.Min(i => (int?)i.Value.Average(j => j.SR?.EC));
        experienceContributionMax = stats.Max(i => (int?)i.Value.Average(j => j.SR?.EC));

        kdMin = stats.Min(
            i =>
            {
                var sumD = i.Value.Sum(j => j.SR?.D);
                var sumS = i.Value.Sum(j => j.SR?.S);
                return (decimal?)sumS / (sumD > 0 ? sumD : 1);
            });
        kdMax = stats.Max(
            i =>
            {
                var sumD = i.Value.Sum(j => j.SR?.D);
                var sumS = i.Value.Sum(j => j.SR?.S);
                return (decimal?)sumS / (sumD > 0 ? sumD : 1);
            });

        assistMin = stats.Min(i => (decimal?)i.Value.Average(j => j.SR?.A));
        assistMax = stats.Max(i => (decimal?)i.Value.Average(j => j.SR?.A));

        structureDamageMin = stats.Min(i => (int?)i.Value.Average(j => j.SR?.StD));
        structureDamageMax = stats.Max(i => (int?)i.Value.Average(j => j.SR?.StD));

        minionDamageMin = stats.Min(i => (int?)i.Value.Average(j => j.SR?.MD));
        minionDamageMax = stats.Max(i => (int?)i.Value.Average(j => j.SR?.MD));

        creepDamageMin = stats.Min(i => (int?)i.Value.Average(j => j.SR?.CD));
        creepDamageMax = stats.Max(i => (int?)i.Value.Average(j => j.SR?.CD));

        townKillMin = stats.Min(i => i.Value.Average(j => j.SR != null ? (decimal?)j.SR.TK : null));
        townKillMax = stats.Max(i => i.Value.Average(j => j.SR != null ? (decimal?)j.SR.TK : null));

        mccMin = stats.Min(i => i.Value.Average(j => j.SR != null ? (decimal?)j.SR.MCC : null));
        mccMax = stats.Max(i => i.Value.Average(j => j.SR != null ? (decimal?)j.SR.MCC : null));
    }

    private int? GetD(TeamProfileReplayCharacter j) =>
        roleDic.ContainsKey(j.C) && roleDic[j.C] == "Warrior" && j.SR != null ? j.SR.D : null;

    private int? GetDt(TeamProfileReplayCharacter j) =>
        roleDic.ContainsKey(j.C) && roleDic[j.C] == "Warrior" && j.SR != null ? j.SR.DT : null;
}
