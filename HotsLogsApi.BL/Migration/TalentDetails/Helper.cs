using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.Models;
using HotsLogsApi.BL.Migration.TalentDetails.Models;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotsLogsApi.BL.Migration.TalentDetails;

public class Helper : HelperBase<TalentDetailsResponse>
{
    private readonly TalentDetailsArgs _args;

    public Helper(TalentDetailsArgs args, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
    }

    public override TalentDetailsResponse MainCalculation()
    {
        using var scope = Svcp.CreateScope();
        var res = new TalentDetailsResponse();

        var teams = GetTeamsIfTournamentSelected(scope);
        res.Teams = teams;

        switch (_args.Tab)
        {
            case 0:
                var (data1, stats) = GetHeroTalentStatistics(scope);
                var (data1B, data1C) = GetPopularTalentBuilds(stats);
                res.TalentStatistics = data1;
                res.PopularTalentBuilds = data1B;
                res.RecentPatchNotesVisible = data1C;
                res.TalentBuildStatistics = FillTalentNames(stats?.HeroTalentBuildStatisticArray);
                break;
            case 1:
                var data2 = CalcWinRateByDateTime();
                res.WinRatesByDate = data2;
                break;
            case 2:
                var data3 = CalcCharacterGameTimeWinRates();
                res.WinRatesByGameLength = data3;
                break;
            case 3:
                var data4 = GetWinPercentVsOtherCharacters();
                res.WinRateVs = data4;
                break;
            case 4:
                var data5 = GetWinPercentWithOtherCharacters();
                res.WinRateWith = data5;
                break;
            case 5:
                var data6 = GetMapStatistics();
                res.MapStatistics = data6;
                break;
            case 6:
                var data7 = CalcWinRateByHeroLevel();
                res.WinRateByHeroLevel = data7;
                break;
            case 7:
                var (data8, data8B) = CalcWinRateByTalentUpgrade();
                res.WinRateByTalentUpgrade = data8;
                res.TalentUpgradeTypes = data8B;
                break;
        }

        return res;
    }

    private string CalcCharacterGameTimeWinRates()
    {
        var selectedLeagues = GetSelectedLeagues();
        var gameModes = GetGameModes();

        // Set Sitewide Character Win Rate by Game Time
        SitewideCharacterGameTimeWinRates winRateByGameTime = null;
        foreach (var selectedLeague in selectedLeagues)
        {
            var key = $"HOTSLogs:SitewideCharacterGameTimeWinRates:{selectedLeague}";

            void SetStats(int gameMode)
            {
                winRateByGameTime += DataHelper.RedisCacheGet<SitewideCharacterGameTimeWinRates>(
                    $"{key}:{gameMode}:{_args.Hero}");
            }

            gameModes.ForEach(SetStats);
        }

        if (!(winRateByGameTime?.GameTimeWinRates?.Length > 0))
        {
            return null;
        }

        winRateByGameTime.GameTimeWinRates =
            winRateByGameTime.GameTimeWinRates.OrderBy(i => i.GameTimeMinuteBegin).ToArray();

        var rc1 =
            winRateByGameTime.GameTimeWinRates.Select(
                i => new RadChartDataRow<int, decimal>
                {
                    X = i.GameTimeMinuteBegin,
                    GamesPlayed = i.GamesPlayed,
                    WinPercent = i.WinPercent,
                }).ToList();

        var rc = new RadChartDef<int, decimal>
        {
            Name = "Game Length in Minutes",
            Type = RadChartType.Number,
            Data = rc1,
            MinY = 0.35m,
            MaxY = 0.7m,
        };
        var json1 = rc.ToJson();
        var json = HttpUtility.HtmlAttributeEncode(json1);

        return json1;
    }

    private SitewideHeroTalentStatistics CalcHeroTalentStatistics(
        IServiceScope scope,
        string[] selectedPatchesOrWeeks,
        string[] selectedMaps,
        int[] selectedLeagues,
        List<int> gameModes)
    {
        SitewideHeroTalentStatistics stats = null;
        foreach (var selectedDateTime in selectedPatchesOrWeeks)
        {
            foreach (var selectedMap in selectedMaps)
            {
                foreach (var selectedLeague in selectedLeagues)
                {
                    const string keyRoot = "HOTSLogs:SitewideHeroTalentStatisticsV5";
                    var keyPrefix =
                        $"{keyRoot}:{selectedDateTime}:{_args.Hero}:{selectedMap}:{selectedLeague}";

                    void SetStats(int gameMode)
                    {
                        stats += DataHelper.RedisCacheGet<SitewideHeroTalentStatistics>($"{keyPrefix}:{gameMode}");
                    }

                    gameModes.ForEach(SetStats);
                }
            }
        }

        if (stats?.HeroTalentBuildStatisticArray is null)
        {
            return stats;
        }

        List<HeroTalentInformation> latestTalents;
        var dc = HeroesdataContext.Create(scope);
        {
            latestTalents = dc.HeroTalentInformations
                .Where(x => x.ReplayBuildLast == 1000000)
                .ToList();
        }

        var talentDic = latestTalents
            .ToDictionary(x => (x.Character, x.TalentTier, x.TalentName), x => x.TalentId);

        var talentMinIds = latestTalents
            .ToLookup(x => (x.Character, x.TalentTier), x => x.TalentId)
            .ToDictionary(x => x.Key, x => x.Min());

        int[] tiers = { 1, 4, 7, 10, 13, 16, 20 };

        foreach (var stat in stats.HeroTalentBuildStatisticArray)
        {
            var heroName = stats.Character;
            var images = Global.HeroTalentImages;

            var talentNames1 = stat.TalentNameDescription.Select(GetTalentName).ToArray();
            var talentNames2 = stat.TalentName.ToArray();
            var talentNames = talentNames2.Zip(talentNames1, (a, b) => a ?? b).ToArray();

            stat.TalentImageURL[0] = images[heroName, talentNames[0]];
            stat.TalentImageURL[1] = images[heroName, talentNames[1]];
            stat.TalentImageURL[2] = images[heroName, talentNames[2]];
            stat.TalentImageURL[3] = images[heroName, talentNames[3]];
            stat.TalentImageURL[4] = images[heroName, talentNames[4]];
            stat.TalentImageURL[5] = images[heroName, talentNames[5]];
            stat.TalentImageURL[6] = images[heroName, talentNames[6]];
            stat.TalentId = talentNames.Select(
                (x, i) =>
                {
                    var tier = tiers[i];
                    var key = (heroName, tier, x);
                    if (string.IsNullOrWhiteSpace(x) || !talentDic.ContainsKey(key))
                    {
                        return 0;
                    }

                    var minId = talentMinIds[(heroName, tiers[i])];
                    return talentDic[key] - minId + 1;
                }).ToArray();
        }

        return stats;
    }

    private string CalcWinRateByDateTime()
    {
        var selectedLeagues = GetSelectedLeagues();
        var gameModes = GetGameModes();

        SitewideCharacterWinRateByDateTime sitewideCharacterWinRateByDateTime = null;
        foreach (var selectedLeague in selectedLeagues)
        {
            var key = $"HOTSLogs:SitewideCharacterWinRateByDateTime:{selectedLeague}";

            void SetStats(int gameMode)
            {
                sitewideCharacterWinRateByDateTime += DataHelper.RedisCacheGet<SitewideCharacterWinRateByDateTime>(
                    $"{key}:{gameMode}:{_args.Hero}");
            }

            gameModes.ForEach(SetStats);
        }

        if (!(sitewideCharacterWinRateByDateTime?.DateTimeWinRates?.Length > 0))
        {
            return null;
        }

        sitewideCharacterWinRateByDateTime.DateTimeWinRates = sitewideCharacterWinRateByDateTime
            .DateTimeWinRates
            .OrderBy(i => i.DateTimeEnd)
            .ToArray();

        var rc1 =
            sitewideCharacterWinRateByDateTime.DateTimeWinRates.Select(
                i => new RadChartDataRow<DateTime, decimal>
                {
                    X = i.DateTimeEnd,
                    GamesPlayed = i.GamesPlayed,
                    WinPercent = i.WinPercent,
                }).ToList();

        var rc = new RadChartDef<DateTime, decimal>
        {
            Type = RadChartType.Date,
            Data = rc1,
            SuggestedMinY = 0.4m,
            SuggestedMaxY = 0.5m,
        };
        var json1 = rc.ToJson();
        var json = HttpUtility.HtmlAttributeEncode(json1);

        return json1;
    }

    private string CalcWinRateByHeroLevel()
    {
        var selectedLeagues = GetSelectedLeagues();
        var gameModes = GetGameModes();

        SitewideCharacterStatisticsWithCharacterLevel stats = null;
        foreach (var selectedLeague in selectedLeagues)
        {
            var key = $"HOTSLogs:SitewideCharacterLevelWinRates:{selectedLeague}";

            void SetStats(int gameMode)
            {
                var data =
                    DataHelper.RedisCacheGet<SitewideCharacterStatisticWithCharacterLevel[]>(
                        $"{key}:{gameMode}:{_args.Hero}");

                stats += new SitewideCharacterStatisticsWithCharacterLevel
                {
                    SitewideCharacterStatisticWithCharacterLevelArray = data,
                };
            }

            gameModes.ForEach(SetStats);
        }

        if (stats?.SitewideCharacterStatisticWithCharacterLevelArray is null ||
            stats.SitewideCharacterStatisticWithCharacterLevelArray.Length <= 0)
        {
            return null;
        }

        stats.SitewideCharacterStatisticWithCharacterLevelArray =
            stats.SitewideCharacterStatisticWithCharacterLevelArray
                .OrderBy(i => i.CharacterLevel)
                .ToArray();

        var rc1 =
            stats.SitewideCharacterStatisticWithCharacterLevelArray
                .Select(
                    i => new RadChartDataRow<int, decimal>
                    {
                        X = i.CharacterLevel,
                        GamesPlayed = i.GamesPlayed,
                        WinPercent = i.WinPercent,
                    }).ToList();

        var rc = new RadChartDef<int, decimal>
        {
            Name = "Hero Level",
            Type = RadChartType.Number,
            Data = rc1,
            MinY = 0.35m,
            MaxY = 0.7m,
        };
        var json1 = rc.ToJson();
        var json = HttpUtility.HtmlAttributeEncode(json1);

        return json1;
    }

    private (string json1, UpgradeEventRow[] upgradeEventTypes) CalcWinRateByTalentUpgrade()
    {
        var selectedLeagues = GetSelectedLeagues();
        var gameModes = GetGameModes();

        SitewideCharacterWinRateByTalentUpgradeEventAverageValue stats = null;
        foreach (var selectedLeague in selectedLeagues)
        {
            var key = $"HOTSLogs:SitewideTalentUpgradeWinRates:{selectedLeague}";

            void SetStats(int gameMode)
            {
                stats +=
                    DataHelper.RedisCacheGet<SitewideCharacterWinRateByTalentUpgradeEventAverageValue>(
                        $"{key}:{gameMode}:{_args.Hero}");
            }

            gameModes.ForEach(SetStats);
        }

        if (stats?.TalentUpgradeEventAverageValueArray is null ||
            stats.TalentUpgradeEventAverageValueArray.Length <= 0)
        {
            return (null, null);
        }

        var upgradeEventTypes = stats
            .TalentUpgradeEventAverageValueArray
            .Select(i => (UpgradeEventType)i.UpgradeEventType)
            .Distinct()
            .OrderBy(i => i)
            .Select(
                i => new UpgradeEventRow
                {
                    Text = i.GetTalentName(),
                    Value = (int)i,
                })
            .ToArray();

        var upgradeEventType = (UpgradeEventType)upgradeEventTypes[0].Value;

        // Copy the array so we don't mess with what is cached in memory
        var derivedStats = stats
            .TalentUpgradeEventAverageValueArray
            .Where(i => i.UpgradeEventType == (int)upgradeEventType)
            .Select(
                i => new TalentUpgradeEventAverageValue
                {
                    UpgradeEventType = i.UpgradeEventType,
                    AverageUpgradeEventValue = i.AverageUpgradeEventValue,
                    GamesPlayed = i.GamesPlayed,
                    WinPercent = i.WinPercent,
                })
            .ToArray();

        if (upgradeEventType == UpgradeEventType.MarksmanStacks)
        {
            foreach (var value in derivedStats)
            {
                value.AverageUpgradeEventValue = (int)value.AverageUpgradeEventValue / 10 * 10; // TODO
            }
        }

        for (var i = 1; i < derivedStats.Length; i += 2)
        {
            derivedStats[i].AverageUpgradeEventValue = derivedStats[i - 1].AverageUpgradeEventValue; // TODO
        }

        derivedStats = derivedStats
            .GroupBy(i => i.AverageUpgradeEventValue)
            .Select(
                i => new TalentUpgradeEventAverageValue
                {
                    UpgradeEventType = (int)upgradeEventType,
                    AverageUpgradeEventValue = i.Key,
                    GamesPlayed = i.Sum(j => j.GamesPlayed),
                    WinPercent = i.Sum(j => j.GamesPlayed * j.WinPercent) / i.Sum(j => j.GamesPlayed),
                })
            .OrderBy(i => i.AverageUpgradeEventValue)
            .ToArray();

        derivedStats = derivedStats.Where(i => i.GamesPlayed > 10).ToArray();

        var rc1 = derivedStats
            .Select(
                i => new RadChartDataRow<decimal, decimal>
                {
                    X = i.AverageUpgradeEventValue,
                    GamesPlayed = i.GamesPlayed,
                    WinPercent = i.WinPercent,
                });

        var rc = new RadChartDef<decimal, decimal>
        {
            Name = "Average Talent Upgrade Stacks",
            Type = RadChartType.Number,
            Data = rc1,
            MinY = 0.35m,
            MaxY = 0.7m,
        };
        var json1 = rc.ToJson();
        var json = HttpUtility.HtmlAttributeEncode(json1);

        return (json1, upgradeEventTypes);
    }

    private (SitewideCharacterWinPercentVsOtherCharacters wrWith,
        SitewideCharacterWinPercentVsOtherCharacters wrVs,
        Dictionary<string, decimal> wrDic) GetDataForWinRatesWithAndAgainst()
    {
        // Set Sitewide Character Win Percent Vs/With Other Characters

        var selectedLeagues = GetSelectedLeagues();
        var gameModes = GetGameModes();

        SitewideCharacterWinPercentVsOtherCharacters wrWith = null;
        SitewideCharacterWinPercentVsOtherCharacters wrVs = null;

        SitewideCharacterStatistics sitewideCharacterStatisticsForDictionary = null;
        foreach (var selectedLeague in selectedLeagues)
        {
            var key1 = $"HOTSLogs:SitewideCharacterStatisticsV2:Current:{selectedLeague}";
            var key2 = $"HOTSLogs:SitewideCharacterWinPercentVsOtherCharacters:{selectedLeague}";
            var key3 = $"HOTSLogs:SitewideCharacterWinPercentWithOtherCharacters:{selectedLeague}";

            void SetStats(int gameMode)
            {
                sitewideCharacterStatisticsForDictionary +=
                    DataHelper.RedisCacheGet<SitewideCharacterStatistics>($"{key1}:{gameMode}");
                wrVs +=
                    DataHelper.RedisCacheGet<SitewideCharacterWinPercentVsOtherCharacters>(
                        $"{key2}:{gameMode}:{_args.Hero}");
                wrWith +=
                    DataHelper.RedisCacheGet<SitewideCharacterWinPercentVsOtherCharacters>(
                        $"{key3}:{gameMode}:{_args.Hero}");
            }

            gameModes.ForEach(SetStats);
        }

        var wrDic =
            sitewideCharacterStatisticsForDictionary?.SitewideCharacterStatisticArray?.ToDictionary(
                i => i.Character,
                i => i.WinPercent);

        return (wrWith, wrVs, wrDic);
    }

    private (TalentDetailsRow<int>[], SitewideHeroTalentStatistics) GetHeroTalentStatistics(IServiceScope scope)
    {
        var rc = Array.Empty<TalentDetailsRow<int>>();

        var selectedPatchesOrWeeks = SelectedPatchesOrWeeks();
        var selectedMaps = GetSelectedMaps();
        var selectedLeagues = GetSelectedLeagues();
        var gameModes = GetGameModes();

        var stats = CalcHeroTalentStatistics(scope, selectedPatchesOrWeeks, selectedMaps, selectedLeagues, gameModes);

        ThinTalentBuildStatistics(stats);

        var dataSource = stats?
            .SitewideHeroTalentStatisticArray
            .Where(i => i.GamesPlayed >= MinRequiredGamesForWinRate(_args.GameMode)).OrderBy(i => i.TalentID)
            .ToArray();

        if ((dataSource?.Length ?? 0) <= 0)
        {
            return (rc, stats);
        }

        var winPercentMin = dataSource.Min(i => i.WinPercent);
        var winPercentMax = dataSource.Max(i => i.WinPercent);

        rc = dataSource
            .Select(
                i => new TalentDetailsRow<int>
                {
                    TalentTier = i.TalentTier,
                    TalentImageURL =
                        Global.HeroTalentImages[stats.Character, i.TalentName],
                    TalentName = i.TalentName ?? GetTalentName(i.TalentDescription),
                    TalentDescription = i.TalentDescription,
                    GamesPlayed = i.GamesPlayed,
                    Popularity = SiteMaster.GetGaugeHtml(
                        (decimal)i.GamesPlayed / dataSource.Where(j => j.TalentTier == i.TalentTier)
                            .Sum(j => j.GamesPlayed),
                        color: SupportColor),
                    WinPercent = i.GamesPlayed >= MinRequiredGames(_args.GameMode)
                        ? SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax)
                        : null,
                })
            .ToArray();
        var headers = rc.Skip(1)
            .Zip(rc, (a, b) => (Talent: a, TierBoundary: a.TalentTier != b.TalentTier))
            .Where(x => x.TierBoundary).ToList();
        var selectedHero = _args.Hero;
        var chromieMod = selectedHero == "Chromie" ? 2 : 0;
        headers.ForEach(
            x => x.Talent.HeaderStart =
                $"Level: {Math.Max(1, x.Talent.TalentTier.GetValueOrDefault() - chromieMod)}");
        rc[0].HeaderStart = "Level: 1";

        return (rc, stats);
    }

    private MapStatsRow[] GetMapStatistics()
    {
        var rc = Array.Empty<MapStatsRow>();

        var selectedLeagues = GetSelectedLeagues();
        var gameModes = GetGameModes();

        SitewideMapStatistics sitewideMapStatistics = null;
        foreach (var selectedLeague in selectedLeagues)
        {
            var key = $"HOTSLogs:SitewideMapStatisticsV2:Current:{selectedLeague}";

            void SetStats(int gameMode)
            {
                sitewideMapStatistics += DataHelper.RedisCacheGet<SitewideMapStatistics>($"{key}:{gameMode}");
            }

            gameModes.ForEach(SetStats);
        }

        var dataSource = sitewideMapStatistics?.SitewideMapStatisticArray?
            .Where(i => i.Character == _args.Hero)
            .ToArray();

        if (!(dataSource?.Length > 0))
        {
            return rc;
        }

        var gamesPlayedMin = dataSource.Min(i => i.GamesPlayed);
        var gamesPlayedMax = dataSource.Max(i => i.GamesPlayed);

        var winPercentMin = dataSource.Min(i => i.WinPercent);
        var winPercentMax = dataSource.Max(i => i.WinPercent);

        rc = dataSource
            .OrderByDescending(i => i.WinPercent)
            .Select(
                i => new MapStatsRow
                {
                    MapImageURL = i.Map.PrepareForImageURL(),
                    MapNameLocalized = SiteMaster.GetLocalizedString("GenericMapName", i.Map),
                    GamesPlayed = SiteMaster.GetGaugeHtml(
                        i.GamesPlayed,
                        gamesPlayedMin,
                        gamesPlayedMax,
                        SupportColor,
                        "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                })
            .ToArray();

        return rc;
    }

    private (PopularTalentBuildsRow[], bool) GetPopularTalentBuilds(SitewideHeroTalentStatistics stats)
    {
        var rc = Array.Empty<PopularTalentBuildsRow>();
        if (stats is null || stats.HeroTalentBuildStatisticArray.Length == 0)
        {
            return (rc, true);
        }

        var gamesPlayedMin = stats.HeroTalentBuildStatisticArray.Min(i => i.GamesPlayed);
        var gamesPlayedMax = stats.HeroTalentBuildStatisticArray.Max(i => i.GamesPlayed);

        var winPercentMin = stats.HeroTalentBuildStatisticArray.Min(i => i.WinPercent);
        var winPercentMax = stats.HeroTalentBuildStatisticArray.Max(i => i.WinPercent);

        rc = stats
            .HeroTalentBuildStatisticArray
            .OrderByDescending(i => i.WinPercent)
            .Select(
                i => new PopularTalentBuildsRow
                {
                    GamesPlayed =
                        SiteMaster.GetGaugeHtml(i.GamesPlayed, gamesPlayedMin, gamesPlayedMax, SupportColor, "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                    TalentNameDescription01 = i.TalentNameDescription[0],
                    TalentNameDescription04 = i.TalentNameDescription[1],
                    TalentNameDescription07 = i.TalentNameDescription[2],
                    TalentNameDescription10 = i.TalentNameDescription[3],
                    TalentNameDescription13 = i.TalentNameDescription[4],
                    TalentNameDescription16 = i.TalentNameDescription[5],
                    TalentNameDescription20 = i.TalentNameDescription[6],
                    TalentName01 = i.TalentName[0] ?? GetTalentName(i.TalentNameDescription[0]),
                    TalentName04 = i.TalentName[1] ?? GetTalentName(i.TalentNameDescription[1]),
                    TalentName07 = i.TalentName[2] ?? GetTalentName(i.TalentNameDescription[2]),
                    TalentName10 = i.TalentName[3] ?? GetTalentName(i.TalentNameDescription[3]),
                    TalentName13 = i.TalentName[4] ?? GetTalentName(i.TalentNameDescription[4]),
                    TalentName16 = i.TalentName[5] ?? GetTalentName(i.TalentNameDescription[5]),
                    TalentName20 = i.TalentName[6] ?? GetTalentName(i.TalentNameDescription[6]),
                    TalentImageURL01 = i.TalentImageURL[0],
                    TalentImageURL04 = i.TalentImageURL[1],
                    TalentImageURL07 = i.TalentImageURL[2],
                    TalentImageURL10 = i.TalentImageURL[3],
                    TalentImageURL13 = i.TalentImageURL[4],
                    TalentImageURL16 = i.TalentImageURL[5],
                    TalentImageURL20 = i.TalentImageURL[6],
                    Export = ExportBuild(i),
                })
            .ToArray();

        var pRecentPatchNoteVisible = stats.HeroTalentBuildStatisticArray.Length <= 3;

        return (rc, pRecentPatchNoteVisible);
    }

    private WinRateWithOrVsRow[]
        GetWinPercentVsOtherCharacters()
    {
        var rc =
            Array.Empty<WinRateWithOrVsRow>();
        var currentHero = _args.Hero;

        var (_, stats, dic) = GetDataForWinRatesWithAndAgainst();
        if (dic is null || !dic.ContainsKey(currentHero) || stats is null)
        {
            return rc;
        }

        var heroRoleDic = Global.GetHeroRoleConcurrentDictionary();
        var heroAliasDic = Global.GetHeroAliasCSVConcurrentDictionary();

        if (currentHero == "Cho")
        {
            stats.SitewideCharacterStatisticArray = stats.SitewideCharacterStatisticArray
                .Where(i => i.Character != "Gall")
                .ToArray();
        }
        else if (currentHero == "Gall")
        {
            stats.SitewideCharacterStatisticArray = stats.SitewideCharacterStatisticArray
                .Where(i => i.Character != "Cho")
                .ToArray();
        }

        var dataSource = stats.SitewideCharacterStatisticArray
            .Where(i => dic.ContainsKey(i.Character)).ToArray();

        if (dataSource.Length <= 0)
        {
            return rc;
        }

        var gamesPlayedMin = dataSource.Min(i => i.GamesPlayed);
        var gamesPlayedMax = dataSource.Max(i => i.GamesPlayed);

        var winPercentMin = dataSource.Min(i => i.WinPercent);
        var winPercentMax = dataSource.Max(i => i.WinPercent);

        rc = dataSource
            .OrderByDescending(i => i.WinPercent).Select(
                i => new
                {
                    HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                    Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                    CharacterURL = i.Character,
                    GamesPlayed =
                        SiteMaster.GetGaugeHtml(
                            i.GamesPlayed,
                            gamesPlayedMin,
                            gamesPlayedMax,
                            SupportColor,
                            "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                    WinPercentDecimal = i.WinPercent,

                    // We split this calculation into two parts so we can avoid a divide by 0 exception
                    RelativeWinPercentPart1 = dic[currentHero] - (dic[currentHero] * dic[i.Character]),
                    RelativeWinPercentPart2 =
                        dic[currentHero] + dic[i.Character] - (2 * dic[currentHero] * dic[i.Character]),
                    Role = heroRoleDic.GetValueOrNull(i.Character),
                    AliasCSV = heroAliasDic.GetValueOrNull(i.Character),
                })
            .Select(
                i => new WinRateWithOrVsRow
                {
                    HeroPortraitURL = i.HeroPortraitURL,
                    Character = i.Character,
                    CharacterURL = i.CharacterURL,
                    GamesPlayed = i.GamesPlayed,
                    WinPercent = i.WinPercent,
                    RelativeWinPercent = i.RelativeWinPercentPart2 != 0
                        ? i.WinPercentDecimal - (i.RelativeWinPercentPart1 / i.RelativeWinPercentPart2)
                        : 0,
                    Role = i.Role,
                    AliasCSV = i.AliasCSV,
                })
            .ToArray();

        return rc;
    }

    private WinRateWithOrVsRow[]
        GetWinPercentWithOtherCharacters()
    {
        var rc = Array.Empty<WinRateWithOrVsRow>();
        var currentHero = _args.Hero;

        var (stats, _, dic) = GetDataForWinRatesWithAndAgainst();
        if (dic is null || !dic.ContainsKey(currentHero) || stats is null)
        {
            return rc;
        }

        var heroRoleDic = Global.GetHeroRoleConcurrentDictionary();
        var heroAliasDic = Global.GetHeroAliasCSVConcurrentDictionary();

        var dataSource = stats.SitewideCharacterStatisticArray
            .Where(i => dic.ContainsKey(i.Character)).ToArray();

        if (dataSource.Length <= 0)
        {
            return rc;
        }

        var gamesPlayedMin = dataSource.Min(i => i.GamesPlayed);
        var gamesPlayedMax = dataSource.Max(i => i.GamesPlayed);

        var winPercentMin = dataSource.Min(i => i.WinPercent);
        var winPercentMax = dataSource.Max(i => i.WinPercent);

        rc = dataSource
            .OrderByDescending(i => i.WinPercent).Select(
                i => new
                {
                    HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                    Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                    CharacterURL = i.Character,
                    GamesPlayed =
                        SiteMaster.GetGaugeHtml(
                            i.GamesPlayed,
                            gamesPlayedMin,
                            gamesPlayedMax,
                            SupportColor,
                            "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                    WinPercentDecimal = i.WinPercent,

                    // We split this calculation into two parts so we can avoid a divide by 0 exception
                    RelativeWinPercentPart1 = dic[currentHero] * dic[i.Character],
                    RelativeWinPercentPart2 =
                        1 + (2 * dic[currentHero] * dic[i.Character]) -
                        (dic[currentHero] + dic[i.Character]),
                    Role = heroRoleDic.GetValueOrNull(i.Character),
                    AliasCSV = heroAliasDic.GetValueOrNull(i.Character),
                })
            .Select(
                i => new WinRateWithOrVsRow
                {
                    HeroPortraitURL = i.HeroPortraitURL,
                    Character = i.Character,
                    CharacterURL = i.CharacterURL,
                    GamesPlayed = i.GamesPlayed,
                    WinPercent = i.WinPercent,
                    RelativeWinPercent = i.RelativeWinPercentPart2 != 0
                        ? i.WinPercentDecimal - (i.RelativeWinPercentPart1 / i.RelativeWinPercentPart2)
                        : 0,
                    Role = i.Role,
                    AliasCSV = i.AliasCSV,
                })
            .ToArray();

        return rc;
    }

    private void ThinTalentBuildStatistics(SitewideHeroTalentStatistics stats)
    {
        if (stats?.HeroTalentBuildStatisticArray is null)
        {
            return;
        }

        stats.HeroTalentBuildStatisticArray = stats
            .HeroTalentBuildStatisticArray
            .Where(i => i.GamesPlayed >= MinRequiredGames(_args.GameMode))
            .ToArray();
    }
}
