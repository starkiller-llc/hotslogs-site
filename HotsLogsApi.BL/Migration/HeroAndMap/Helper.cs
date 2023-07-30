using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.HeroAndMap.Models;
using HotsLogsApi.BL.Migration.Models;
using HotsLogsApi.BL.Resources;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HotsLogsApi.BL.Migration.HeroAndMap;

public class Helper : HelperBase<HeroAndMapResponse>
{
    private readonly HeroAndMapArgs _args;
    private readonly Dictionary<string, SitewideHeroTalentStatistics> _talentStatsDic = new();

    public Helper(HeroAndMapArgs args, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
    }

    public override HeroAndMapResponse MainCalculation()
    {
        using var scope = Svcp.CreateScope();
        var res = new HeroAndMapResponse();

        res.Roles = Roles;

        var teams = GetTeamsIfTournamentSelected(scope);
        res.Teams = teams;

        var selectedGameMode = GetSelectedGameMode();
        var selectedMaps = GetSelectedMaps();
        var selectedLeagues = GetSelectedLeagues();
        var selectedGameLengths = GetSelectedGameLengths();
        var selectedCharacterLevels = GetSelectedLevels();
        var selectedPatchesOrWeeks = SelectedPatchesOrWeeks();

        res.PopularityNotice = selectedLeagues[0] != -1 && _args.Talent == "AllTalents";

        SitewideCharacterStatistics stats = null;
        SitewideCharacterStatistics statsPrevious = null;
        if (selectedGameLengths[0] != -1)
        {
            // Specific Game Length
            if (selectedGameMode == -99)
            {
                selectedGameMode = (int)GameMode.StormLeague;
                // GameModeFilter.SelectedValue = selectedGameMode.ToString();
            }

            // TODO: verify these don't get in the way of the event selection
            res.GameLengthFilterNotice = true;
            res.CharacterLevelFilterNotice = true;
            // pCharacterLevelFilterNotice.Visible = false;
            // MapFilter.Visible = false;
            // TimeFilter.Visible = false;
            // GameLengthFilter.Visible = true;
            // LevelFilter.Visible = false;
            // TalentFilter.Visible = false;

            // StatisticsGrid.Columns.FindByDataField("WinPercentDelta").Visible = false;

            foreach (var selectedGameLength in selectedGameLengths)
            {
                foreach (var selectedLeague in selectedLeagues)
                {
                    var sitewideCharacterGameTimeWinRatesArray =
                        DataHelper.RedisCacheGet<SitewideCharacterGameTimeWinRates[]>(
                            $"HOTSLogs:SitewideCharacterGameTimeWinRates:{selectedLeague}:{selectedGameMode}");
                    if (sitewideCharacterGameTimeWinRatesArray is not { Length: > 0 })
                    {
                        continue;
                    }

                    stats = new SitewideCharacterStatistics
                    {
                        League = selectedLeague,
                        GameMode = selectedGameMode,
                        LastUpdated = sitewideCharacterGameTimeWinRatesArray[0].LastUpdated,
                        SitewideCharacterStatisticArray = sitewideCharacterGameTimeWinRatesArray.Select(
                            i =>
                                new SitewideCharacterStatistic
                                {
                                    HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                                    Character = i.Character,
                                    GamesPlayed =
                                        i.GameTimeWinRates.Where(
                                            j =>
                                                j.GameTimeMinuteBegin >= selectedGameLength &&
                                                j.GameTimeMinuteBegin < selectedGameLength +
                                                FilterDataSources.GameTimeFilterMinuteInterval).Sum(j => j.GamesPlayed),
                                    WinPercent = i.GameTimeWinRates
                                        .Where(
                                            j => j.GameTimeMinuteBegin >= selectedGameLength &&
                                                 j.GameTimeMinuteBegin < selectedGameLength +
                                                 FilterDataSources.GameTimeFilterMinuteInterval).Sum(
                                            j =>
                                                j.GamesPlayed * j.WinPercent),
                                }).ToArray(),
                    };

                    // To avoid divide by 0 exception, we initially store 'GamesWon' in the 'WinPercent' variable, and now we check if 'GamesPlayed' > 0
                    stats.SitewideCharacterStatisticArray =
                        stats.SitewideCharacterStatisticArray
                            .Where(i => i.GamesPlayed > 0).ToArray();
                    foreach (var sitewideCharacterStatisticsArrayEntry in
                             stats
                                 .SitewideCharacterStatisticArray)
                    {
                        sitewideCharacterStatisticsArrayEntry.WinPercent /=
                            sitewideCharacterStatisticsArrayEntry.GamesPlayed;
                    }

                    var totalMinutes =
                        (int)(DateTime.UtcNow - sitewideCharacterGameTimeWinRatesArray[0].LastUpdated)
                        .TotalMinutes;
                    res.LastUpdatedText = string.Format(
                        LocalizedText.GenericLastUpdatedMinutesAgo,
                        totalMinutes);
                }
            }
        }
        else
        {
            if (selectedCharacterLevels[0] != -1)
            {
                // Specific Character Level
                stats = new SitewideCharacterStatistics();
                if (selectedGameMode == -99)
                {
                    selectedGameMode = (int)GameMode.StormLeague;
                    // GameModeFilter.SelectedValue = selectedGameMode.ToString();
                }

                // pGameLengthFilterNotice.Visible = false;
                res.CharacterLevelFilterNotice = true;
                // MapFilter.Visible = false;
                // TimeFilter.Visible = false;
                // GameLengthFilter.Visible = false;
                // LevelFilter.Visible = true;
                // TalentFilter.Visible = false;

                // StatisticsGrid.Columns.FindByDataField("WinPercentDelta").Visible = false;

                foreach (var selectedCharacterLevel in selectedCharacterLevels)
                {
                    foreach (var selectedLeague in selectedLeagues)
                    {
                        var sitewideCharacterLevelWinRatesArray =
                            DataHelper.RedisCacheGet<SitewideCharacterStatisticWithCharacterLevel[]>(
                                $"HOTSLogs:SitewideCharacterLevelWinRates:{selectedLeague}:{selectedGameMode}:{selectedCharacterLevel}");
                        if (sitewideCharacterLevelWinRatesArray is not { Length: > 0 })
                        {
                            continue;
                        }

                        stats += new SitewideCharacterStatistics
                        {
                            League = selectedLeague,
                            GameMode = selectedGameMode,
                            SitewideCharacterStatisticArray = sitewideCharacterLevelWinRatesArray.Select(
                                i =>
                                    new SitewideCharacterStatistic
                                    {
                                        HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                                        Character = i.Character,
                                        GamesPlayed = i.GamesPlayed,
                                        WinPercent = i.WinPercent,
                                    }).ToArray(),
                        };
                    }
                }

                var redisClient = MyDbWrapper.Create(scope);
                var timeToLive = redisClient.GetTimeToLive(
                    $"HOTSLogs:SitewideCharacterLevelWinRates:{selectedLeagues[0]}:{selectedGameMode}:{selectedCharacterLevels[0]}");

                if (timeToLive.HasValue)
                {
                    res.LastUpdatedText = string.Format(
                        LocalizedText.GenericLastUpdatedMinutesAgo,
                        (int)(TimeSpan.FromDays(30) - timeToLive.Value).TotalMinutes);
                }
                else
                {
                    res.LastUpdatedText = string.Empty;
                }
            }
            else
            {
                // All Game Lengths and Character Levels
                stats = null;
                //pGameLengthFilterNotice.Visible = false;
                //pCharacterLevelFilterNotice.Visible = false;
                //MapFilter.Visible = true;
                ////ComboBoxReplayDateTime.Visible = true;
                //GameLengthFilter.Visible = true;
                ////LevelFilter.Visible = true;
                //TalentFilter.Visible = true;

                //StatisticsGrid.Columns.FindByDataField("WinPercentDelta").Visible = true;

                SitewideMapStatistics sitewideMapStatistics = null;

                foreach (var patchOrWeek in selectedPatchesOrWeeks)
                {
                    foreach (var selectedLeague in selectedLeagues)
                    {
                        if (_args.Talent == "AllTalents")
                        {
                            if (selectedMaps.Length == 1 && selectedMaps.Single() == "-1")
                            {
                                // All Maps
                                if (selectedGameMode == -99)
                                {
                                    // Combine Hero league, Team league, and Storm league statistics
                                    stats +=
                                        DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                                            $"HOTSLogs:SitewideCharacterStatisticsV2:{patchOrWeek}:{selectedLeague}:{(int)GameMode.HeroLeague}");
                                    stats +=
                                        DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                                            $"HOTSLogs:SitewideCharacterStatisticsV2:{patchOrWeek}:{selectedLeague}:{(int)GameMode.TeamLeague}");
                                    stats +=
                                        DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                                            $"HOTSLogs:SitewideCharacterStatisticsV2:{patchOrWeek}:{selectedLeague}:{(int)GameMode.StormLeague}");
                                }
                                else
                                {
                                    stats +=
                                        DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                                            $"HOTSLogs:SitewideCharacterStatisticsV2:{patchOrWeek}:{selectedLeague}:{selectedGameMode}");
                                }
                            }
                            else
                                // Specific Maps
                            if (selectedGameMode == -99)
                            {
                                // Combine Hero league, Team league, and Storm league statistics
                                sitewideMapStatistics +=
                                    DataHelper.RedisCacheGet<SitewideMapStatistics>(
                                        $"HOTSLogs:SitewideMapStatisticsV2:{patchOrWeek}:{selectedLeague}:{(int)GameMode.HeroLeague}");
                                sitewideMapStatistics +=
                                    DataHelper.RedisCacheGet<SitewideMapStatistics>(
                                        $"HOTSLogs:SitewideMapStatisticsV2:{patchOrWeek}:{selectedLeague}:{(int)GameMode.TeamLeague}");
                                sitewideMapStatistics +=
                                    DataHelper.RedisCacheGet<SitewideMapStatistics>(
                                        $"HOTSLogs:SitewideMapStatisticsV2:{patchOrWeek}:{selectedLeague}:{(int)GameMode.StormLeague}");
                            }
                            else
                            {
                                sitewideMapStatistics +=
                                    DataHelper.RedisCacheGet<SitewideMapStatistics>(
                                        $"HOTSLogs:SitewideMapStatisticsV2:{patchOrWeek}:{selectedLeague}:{selectedGameMode}");
                            }
                        }
                        else
                        {
                            foreach (var heroPrimaryName in Global.GetLocalizationAlias()
                                         .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero)
                                         .Select(i => i.PrimaryName).ToArray())
                            {
                                foreach (var selectedMap in selectedMaps)
                                {
                                    if (!_talentStatsDic.ContainsKey(heroPrimaryName))
                                    {
                                        if (selectedGameMode == -99)
                                        {
                                            // Combine Hero league, Team league, and Storm league statistics
                                            _talentStatsDic[heroPrimaryName] =
                                                DataHelper.RedisCacheGet<SitewideHeroTalentStatistics>(
                                                    $"HOTSLogs:SitewideHeroTalentStatisticsV5:{patchOrWeek}:{heroPrimaryName}:{selectedMap}:{selectedLeague}:{(int)GameMode.HeroLeague}");
                                            _talentStatsDic[heroPrimaryName] =
                                                DataHelper.RedisCacheGet<SitewideHeroTalentStatistics>(
                                                    $"HOTSLogs:SitewideHeroTalentStatisticsV5:{patchOrWeek}:{heroPrimaryName}:{selectedMap}:{selectedLeague}:{(int)GameMode.TeamLeague}");
                                            _talentStatsDic[heroPrimaryName] =
                                                DataHelper.RedisCacheGet<SitewideHeroTalentStatistics>(
                                                    $"HOTSLogs:SitewideHeroTalentStatisticsV5:{patchOrWeek}:{heroPrimaryName}:{selectedMap}:{selectedLeague}:{(int)GameMode.StormLeague}");
                                        }
                                        else
                                        {
                                            _talentStatsDic[heroPrimaryName] =
                                                DataHelper.RedisCacheGet<SitewideHeroTalentStatistics>(
                                                    $"HOTSLogs:SitewideHeroTalentStatisticsV5:{patchOrWeek}:{heroPrimaryName}:{selectedMap}:{selectedLeague}:{selectedGameMode}");
                                        }
                                    }
                                    else if (selectedGameMode == -99)
                                    {
                                        // Combine Hero league, Team league, and Storm league statistics
                                        _talentStatsDic[heroPrimaryName] +=
                                            DataHelper.RedisCacheGet<SitewideHeroTalentStatistics>(
                                                $"HOTSLogs:SitewideHeroTalentStatisticsV5:{patchOrWeek}:{heroPrimaryName}:{selectedMap}:{selectedLeague}:{(int)GameMode.HeroLeague}");
                                        _talentStatsDic[heroPrimaryName] +=
                                            DataHelper.RedisCacheGet<SitewideHeroTalentStatistics>(
                                                $"HOTSLogs:SitewideHeroTalentStatisticsV5:{patchOrWeek}:{heroPrimaryName}:{selectedMap}:{selectedLeague}:{(int)GameMode.TeamLeague}");
                                        _talentStatsDic[heroPrimaryName] +=
                                            DataHelper.RedisCacheGet<SitewideHeroTalentStatistics>(
                                                $"HOTSLogs:SitewideHeroTalentStatisticsV5:{patchOrWeek}:{heroPrimaryName}:{selectedMap}:{selectedLeague}:{(int)GameMode.StormLeague}");
                                    }
                                    else
                                    {
                                        _talentStatsDic[heroPrimaryName] +=
                                            DataHelper.RedisCacheGet<SitewideHeroTalentStatistics>(
                                                $"HOTSLogs:SitewideHeroTalentStatisticsV5:{patchOrWeek}:{heroPrimaryName}:{selectedMap}:{selectedLeague}:{selectedGameMode}");
                                    }
                                }
                            }
                        }
                    }
                }

                // If filtering by Map, Convert Map container to Character container
                if (sitewideMapStatistics != null)
                {
                    foreach (var selectedMap in selectedMaps)
                    {
                        stats += new SitewideCharacterStatistics
                        {
                            DateTimeBegin = sitewideMapStatistics.DateTimeBegin,
                            DateTimeEnd = sitewideMapStatistics.DateTimeEnd,
                            GameMode = sitewideMapStatistics.GameMode,
                            League = sitewideMapStatistics.League,
                            LastUpdated = sitewideMapStatistics.LastUpdated,
                            SitewideCharacterStatisticArray = sitewideMapStatistics.SitewideMapStatisticArray
                                .Where(i => i.Map == selectedMap).Select(
                                    i => new SitewideCharacterStatistic
                                    {
                                        HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                                        Character = i.Character,
                                        GamesPlayed = i.GamesPlayed,
                                        GamesBanned = i.GamesBanned,
                                        AverageLength = i.AverageLength,
                                        WinPercent = i.WinPercent,
                                        AverageScoreResult = i.AverageScoreResult,
                                    }).OrderByDescending(i => i.WinPercent).ToArray(),
                        };
                    }
                }

                if (stats != null)
                {
                    statsPrevious = null;

                    var prevKey = GetPreviousKey();

                    SitewideMapStatistics mapStatsPrevious = null;

                    foreach (var selectedLeague in selectedLeagues)
                    {
                        if (selectedMaps.Length == 1 && selectedMaps.Single() == "-1")
                        {
                            SitewideCharacterStatistics CacheGet(string key) =>
                                DataHelper.RedisCacheGet<SitewideCharacterStatistics>(key);

                            // All Maps
                            var prefix = $"HOTSLogs:SitewideCharacterStatisticsV2:{prevKey}:{selectedLeague}";
                            if (selectedGameMode == -99)
                            {
                                // Combine Hero league, Team league, and Storm league statistics
                                statsPrevious += CacheGet($"{prefix}:{(int)GameMode.HeroLeague}");
                                statsPrevious += CacheGet($"{prefix}:{(int)GameMode.TeamLeague}");
                                statsPrevious += CacheGet($"{prefix}:{(int)GameMode.StormLeague}");
                            }
                            else
                            {
                                statsPrevious += CacheGet($"{prefix}:{selectedGameMode}");
                            }
                        }
                        else
                        {
                            SitewideMapStatistics CacheGet(string key) =>
                                DataHelper.RedisCacheGet<SitewideMapStatistics>(key);

                            var prefix = $"HOTSLogs:SitewideMapStatisticsV2:{prevKey}:{selectedLeague}";
                            if (selectedGameMode == -99)
                            {
                                // Specific Maps
                                // Combine Hero league, Team league, and Storm league statistics
                                mapStatsPrevious += CacheGet($"{prefix}:{(int)GameMode.HeroLeague}");
                                mapStatsPrevious += CacheGet($"{prefix}:{(int)GameMode.TeamLeague}");
                                mapStatsPrevious += CacheGet($"{prefix}:{(int)GameMode.StormLeague}");
                            }
                            else
                            {
                                mapStatsPrevious += CacheGet($"{prefix}:{selectedGameMode}");
                            }
                        }
                    }

                    if (mapStatsPrevious != null)
                    {
                        foreach (var selectedMap in selectedMaps)
                        {
                            statsPrevious += new SitewideCharacterStatistics
                            {
                                DateTimeBegin = mapStatsPrevious.DateTimeBegin,
                                DateTimeEnd = mapStatsPrevious.DateTimeEnd,
                                GameMode = mapStatsPrevious.GameMode,
                                League = mapStatsPrevious.League,
                                LastUpdated = mapStatsPrevious.LastUpdated,
                                SitewideCharacterStatisticArray = mapStatsPrevious
                                    .SitewideMapStatisticArray.Where(i => i.Map == selectedMap).Select(
                                        i =>
                                            new SitewideCharacterStatistic
                                            {
                                                HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                                                Character = i.Character,
                                                GamesPlayed = i.GamesPlayed,
                                                GamesBanned = i.GamesBanned,
                                                AverageLength = i.AverageLength,
                                                WinPercent = i.WinPercent,
                                                AverageScoreResult = i.AverageScoreResult,
                                            }).OrderByDescending(i => i.WinPercent).ToArray(),
                            };
                        }
                    }

                    res.LastUpdatedText = string.Format(
                        LocalizedText.GenericLastUpdatedMinutesAgo,
                        (int)(DateTime.UtcNow - stats.LastUpdated).TotalMinutes);
                }
                else if (_talentStatsDic.Values.Any(i => i != null))
                {
                    var dateTime = _talentStatsDic.Values
                        .Where(i => i != null)
                        .Select(i => i.LastUpdated)
                        .OrderByDescending(i => i).First();

                    var totalMinutes = (int)(DateTime.UtcNow - dateTime).TotalMinutes;

                    res.LastUpdatedText = string.Format(LocalizedText.GenericLastUpdatedMinutesAgo, totalMinutes);
                }
            }
        }

        // Check for Bans
        var isBanDataAvailable = stats != null &&
                                 (stats.GameMode == (int)GameMode.UnrankedDraft ||
                                  stats.GameMode == (int)GameMode.HeroLeague ||
                                  stats.GameMode == (int)GameMode.TeamLeague ||
                                  stats.GameMode == (int)GameMode.StormLeague ||
                                  Global.IsEventGameMode(stats.GameMode)) &&
                                 stats.DateTimeEnd > new DateTime(2016, 3, 29);

        var (popularTalentBuilds, recentPatchNoteVisible) = GetPopularTalentBuildsDataSource(scope);
        var stats2 = GetStatisticsGridDataSource(stats, statsPrevious);

        res.PopularTalentBuilds = popularTalentBuilds;
        res.RecentPatchNoteVisible = recentPatchNoteVisible;
        res.Stats = stats2;
        res.BanDataAvailable = isBanDataAvailable;

        FillTalentNames(res.PopularTalentBuilds);

        return res;
    }

    protected StatisticsGridRow[] GetStatisticsGridDataSource(
        SitewideCharacterStatistics stats,
        SitewideCharacterStatistics statsPrevious)
    {
        var rc = Array.Empty<StatisticsGridRow>();

        var
            heroRoleConcurrentDictionary = Global.GetHeroRoleConcurrentDictionary();
        var heroAliasCsvConcurrentDictionary =
            Global.GetHeroAliasCSVConcurrentDictionary();

        Dictionary<string, decimal> sitewideCharacterStatisticsPreviousWeekDictionary = null;
        if (statsPrevious != null)
        {
            sitewideCharacterStatisticsPreviousWeekDictionary = statsPrevious
                .SitewideCharacterStatisticArray.Where(i => i.GamesPlayed > 10)
                .ToDictionary(i => i.Character, i => i.WinPercent);
        }

        if (stats != null &&
            stats.SitewideCharacterStatisticArray.Length > 0)
        {
            var winRateMin = stats.SitewideCharacterStatisticArray.Min(i => i.WinPercent);
            var winRateMax = stats.SitewideCharacterStatisticArray.Max(i => i.WinPercent);

            rc = stats.SitewideCharacterStatisticArray
                .Select(
                    i =>
                    {
                        var popularityNumeric = (decimal)((i.GamesPlayed + i.GamesBanned) *
                                                          (stats.GameMode == (int)GameMode.UnrankedDraft ||
                                                           stats.GameMode == (int)GameMode.HeroLeague ||
                                                           stats.GameMode == (int)GameMode.TeamLeague ||
                                                           stats.GameMode == (int)GameMode.StormLeague
                                                              ? 13.875
                                                              : 10)) / stats.SitewideCharacterStatisticArray
                            .Sum(j => j.GamesPlayed + j.GamesBanned);
                        return new StatisticsGridRow
                        {
                            HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                            Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                            CharacterURL = i.Character,
                            GamesPlayed = i.GamesPlayed,
                            GamesBanned = i.GamesBanned,
                            Popularity = SiteMaster.GetGaugeHtml(
                                popularityNumeric,
                                color: TeamCompHelper.HeroRoleColorsDictionary["Support"]),
                            WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winRateMin, winRateMax),
                            WinPercentDelta = sitewideCharacterStatisticsPreviousWeekDictionary != null &&
                                              sitewideCharacterStatisticsPreviousWeekDictionary.ContainsKey(i.Character)
                                ? (decimal?)i.WinPercent -
                                  sitewideCharacterStatisticsPreviousWeekDictionary[i.Character]
                                : null,
                            Role = heroRoleConcurrentDictionary.ContainsKey(i.Character)
                                ? heroRoleConcurrentDictionary[i.Character]
                                : null,
                            AliasCSV = heroAliasCsvConcurrentDictionary.ContainsKey(i.Character)
                                ? heroAliasCsvConcurrentDictionary[i.Character]
                                : null,
                            GameMode = _args.GameModeEx,
                            Event = _args.Tournament,
                        };
                    }).OrderByDescending(i => i.WinPercent).ToArray();
        }

        return rc;
    }

    private (PopularTalentsRow[] rcc, bool pRecentPatchNote) GetPopularTalentBuildsDataSource(IServiceScope scope)
    {
        var rcc = Array.Empty<PopularTalentsRow>();

        var dc = HeroesdataContext.Create(scope);
        var latestTalents = dc.HeroTalentInformations
            .Where(x => x.ReplayBuildLast == 1000000)
            .ToList();

        var talentDic = latestTalents
            .ToDictionary(x => (x.Character, x.TalentTier, x.TalentName), x => x.TalentId);

        var talentMinIds = latestTalents
            .ToLookup(x => (x.Character, x.TalentTier), x => x.TalentId)
            .ToDictionary(x => x.Key, x => x.Min());

        int[] tiers = { 1, 4, 7, 10, 13, 16, 20 };

        var sitewideHeroTalentStatistics = _talentStatsDic.Values
            .Where(
                i => i?.HeroTalentBuildStatisticArray != null && i.HeroTalentBuildStatisticArray.Any(
                    j =>
                        j.GamesPlayed >= MinRequiredGames(_args.GameMode)))
            .Select(
                i => new
                {
                    HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                    Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                    CharacterURL = i.Character,
                    TalentBuild = i.HeroTalentBuildStatisticArray
                        .Where(j => j.GamesPlayed >= MinRequiredGames(_args.GameMode))
                        .OrderByDescending(
                            j =>
                                _args.Talent == "HighestWinRate"
                                    ? j.WinPercent
                                    : j.GamesPlayed)
                        .First(),
                })
            .Select(
                i =>
                {
                    var ex = new Regex(@"^(?<talentName>Calldown: MULE|Divert Power: Weapons|[^:]+)");
                    var talentNames = i.TalentBuild.TalentNameDescription
                        .Select(x => x == null ? null : ex.Match(x).Value).ToArray();
                    var heroName = i.CharacterURL;
                    var talentId = talentNames.Select(
                        (x, ii) =>
                        {
                            var tier = tiers[ii];
                            var key = (heroName, tier, x);
                            if (string.IsNullOrWhiteSpace(x) || !talentDic.ContainsKey(key))
                            {
                                return 0;
                            }

                            var minId = talentMinIds[(heroName, tier)];
                            return talentDic[key] - minId + 1;
                        }).ToArray();

                    var exportBuild = string.Join(string.Empty, talentId.Select(x => $"{x}"));

                    var rc = new
                    {
                        i.HeroPortraitURL,
                        i.Character,
                        i.CharacterURL,
                        i.TalentBuild.GamesPlayed,
                        i.TalentBuild.WinPercent,
                        TalentNameDescription01 = i.TalentBuild.TalentNameDescription[0],
                        TalentNameDescription04 = i.TalentBuild.TalentNameDescription[1],
                        TalentNameDescription07 = i.TalentBuild.TalentNameDescription[2],
                        TalentNameDescription10 = i.TalentBuild.TalentNameDescription[3],
                        TalentNameDescription13 = i.TalentBuild.TalentNameDescription[4],
                        TalentNameDescription16 = i.TalentBuild.TalentNameDescription[5],
                        TalentNameDescription20 = i.TalentBuild.TalentNameDescription[6],
                        TalentName01 = i.TalentBuild.TalentName[0],
                        TalentName04 = i.TalentBuild.TalentName[1],
                        TalentName07 = i.TalentBuild.TalentName[2],
                        TalentName10 = i.TalentBuild.TalentName[3],
                        TalentName13 = i.TalentBuild.TalentName[4],
                        TalentName16 = i.TalentBuild.TalentName[5],
                        TalentName20 = i.TalentBuild.TalentName[6],
                        TalentImageURL01 = Global.HeroTalentImages[i.Character, talentNames[0]],
                        TalentImageURL04 = Global.HeroTalentImages[i.Character, talentNames[1]],
                        TalentImageURL07 = Global.HeroTalentImages[i.Character, talentNames[2]],
                        TalentImageURL10 = Global.HeroTalentImages[i.Character, talentNames[3]],
                        TalentImageURL13 = Global.HeroTalentImages[i.Character, talentNames[4]],
                        TalentImageURL16 = Global.HeroTalentImages[i.Character, talentNames[5]],
                        TalentImageURL20 = Global.HeroTalentImages[i.Character, talentNames[6]],
                        Export = exportBuild,
                    };
                    return rc;
                }).ToArray();

        var pRecentPatchNote = false;
        if (sitewideHeroTalentStatistics.Length < 10)
        {
            pRecentPatchNote = true;
            return (rcc, pRecentPatchNote);
        }

        pRecentPatchNote = false;

        var gamesPlayedMin = sitewideHeroTalentStatistics.Min(i => i.GamesPlayed);
        var gamesPlayedMax = sitewideHeroTalentStatistics.Max(i => i.GamesPlayed);

        var winPercentMin = sitewideHeroTalentStatistics.Min(i => i.WinPercent);
        var winPercentMax = sitewideHeroTalentStatistics.Max(i => i.WinPercent);

        var heroRoleConcurrentDictionary =
            Global.GetHeroRoleConcurrentDictionary();
        var heroAliasCsvConcurrentDictionary =
            Global.GetHeroAliasCSVConcurrentDictionary();

        rcc = sitewideHeroTalentStatistics
            .OrderByDescending(i => i.WinPercent)
            .Select(
                i => new PopularTalentsRow
                {
                    HeroPortraitURL = i.HeroPortraitURL,
                    Character = i.Character,
                    CharacterURL = i.CharacterURL,
                    GamesPlayed = SiteMaster.GetGaugeHtml(
                        i.GamesPlayed,
                        gamesPlayedMin,
                        gamesPlayedMax,
                        TeamCompHelper.HeroRoleColorsDictionary["Support"],
                        "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                    TalentNameDescription01 = i.TalentNameDescription01,
                    TalentNameDescription04 = i.TalentNameDescription04,
                    TalentNameDescription07 = i.TalentNameDescription07,
                    TalentNameDescription10 = i.TalentNameDescription10,
                    TalentNameDescription13 = i.TalentNameDescription13,
                    TalentNameDescription16 = i.TalentNameDescription16,
                    TalentNameDescription20 = i.TalentNameDescription20,
                    TalentName01 = i.TalentName01,
                    TalentName04 = i.TalentName04,
                    TalentName07 = i.TalentName07,
                    TalentName10 = i.TalentName10,
                    TalentName13 = i.TalentName13,
                    TalentName16 = i.TalentName16,
                    TalentName20 = i.TalentName20,
                    TalentImageURL01 = i.TalentImageURL01,
                    TalentImageURL04 = i.TalentImageURL04,
                    TalentImageURL07 = i.TalentImageURL07,
                    TalentImageURL10 = i.TalentImageURL10,
                    TalentImageURL13 = i.TalentImageURL13,
                    TalentImageURL16 = i.TalentImageURL16,
                    TalentImageURL20 = i.TalentImageURL20,
                    Role = heroRoleConcurrentDictionary.GetValueOrDefault(i.Character),
                    AliasCSV = heroAliasCsvConcurrentDictionary.GetValueOrDefault(i.Character),
                    Export = i.Export,
                }).ToArray();

        return (rcc, pRecentPatchNote);
    }
}
