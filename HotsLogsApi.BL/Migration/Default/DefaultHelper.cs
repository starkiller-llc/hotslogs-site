using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.Models;
using HotsLogsApi.BL.Resources;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotsLogsApi.BL.Migration.Default;

public class DefaultHelper
{
    private readonly DefaultHelperArgs _args;

    public DefaultHelper(DefaultHelperArgs args)
    {
        _args = args;
    }

    public DefaultHelperResult MainCalculation()
    {
        var res = new DefaultHelperResult
        {
            Constants =
            {
                Header = LocalizedText.DefaultHeader,
                Intro = LocalizedText.DefaultIntroMessageLine1,
                SitewideHeroStatistics = LocalizedText.SitewideHeroStatistics,
                GenericWinPercent = LocalizedText.GenericWinPercent,
                MonkeyBrokerScript = SiteMaster.MasterAdvertisementBannerMonkeyBrokerTopScript,
                MonkeyBrokerScriptVisible = _args.MonkeyBrokerScriptVisible,
            },
        };

        var homePageMessage = DataHelper.RedisCacheGet<string>("HOTSLogs:HomePageMessage");
        if (homePageMessage != null)
        {
            res.ImportantNoteVisible = true;
            res.ImportantNote = homePageMessage;
        }

        var generalStatistics =
            DataHelper.RedisCacheGet<GeneralStatistics>("HOTSLogs:GeneralStatistics");
        if (generalStatistics != null)
        {
            res.TotalGamesPlayedMessage = string.Format(
                LocalizedText.DefaultIntroMessageTotalGames,
                $@"<strong>{generalStatistics.TotalReplaysUploaded:n0}</strong>");
        }

        res.DataSource = GetDefaultStats(out var lastNPatches, out var latestStats);

        res.NumberOfPatchesShown = lastNPatches;

        if (latestStats != null)
        {
            res.LastUpdated = string.Format(
                LocalizedText.GenericLastUpdatedMinutesAgo,
                (int)(_args.ReferenceDate - latestStats.LastUpdated).TotalMinutes);

            // Set up Role filter shortcut buttons
            if (!_args.IsMobileDevice)
            {
                res.Roles = TeamCompHelper.HeroRoleColorsDictionary
                    .Where(i => HeroRole.HeroRoleOrderDictionary.ContainsKey(i.Key))
                    .OrderByDescending(i => HeroRole.HeroRoleOrderDictionary[i.Key]);
            }
            else
            {
                res.Roles = Array.Empty<KeyValuePair<string, string>>();
            }

            if (!_args.IsPostBack)
            {
                res.RoleButtonsDataSource = res.Roles;
            }
        }

        // Adjust page for mobile devices
        if (_args.IsMobileDevice)
        {
            res.GamesPlayedVisible = false;
            res.GamesBannedVisible = false;
            res.WinPercentDeltaVisible = false;
        }

        return res;
    }

    private List<CharacterStatisticsViewData> GetDefaultStats(
        out int lastNPatches,
        out SitewideCharacterStatistics latestStats2)
    {
        List<CharacterStatisticsViewData> dataSource = null;
        const int gameMode = (int)GameMode.StormLeague;

        var latestGameVersions = DataHelper.GetGameVersions();

        var latestPatch = latestGameVersions.Select(x => $"{x.BuildFirst}-{x.BuildLast}").First();
        var ratedGamesLatestPatchKey =
            $"HOTSLogs:NumberOfRatedGamesV1:{latestPatch}:-1:-1:-1:{gameMode}";
        var numberOfRatedGamesLatestPatch = DataHelper.RedisCacheGetInt(ratedGamesLatestPatchKey);

        lastNPatches = numberOfRatedGamesLatestPatch < 10000 ? 2 : 1;

        var lastNPatchVersions = latestGameVersions.Take(lastNPatches).DefaultIfEmpty();
        var prevPatch = latestGameVersions.Skip(lastNPatches).FirstOrDefault();

        var latestPatches = lastNPatchVersions.Select(x => $"{x.BuildFirst}-{x.BuildLast}").ToList();
        var previousPatch = $"{prevPatch.BuildFirst}-{prevPatch.BuildLast}";

        var latestStatsArr = latestPatches.Select(
            x => DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                $"HOTSLogs:SitewideCharacterStatisticsV2:{x}:-1:{gameMode}"));

        var latestStats = latestStatsArr.Aggregate((x, y) => x + y);
        latestStats2 = latestStats;

        if (latestStats != null)
        {
            var prevStats = DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                $"HOTSLogs:SitewideCharacterStatisticsV2:{previousPatch}:-1:{gameMode}");
            Dictionary<string, decimal> prevStatsDic = null;
            if (prevStats != null)
            {
                prevStatsDic =
                    prevStats.SitewideCharacterStatisticArray
                        .Where(i => i.GamesPlayed > 10)
                        .ToDictionary(i => i.Character, i => i.WinPercent);
            }

            var heroRoleDic = Global.GetHeroRoleConcurrentDictionary();
            var heroAliasCsvDic = Global.GetHeroAliasCSVConcurrentDictionary();

            var statsDataSource =
                latestStats.SitewideCharacterStatisticArray
                    .Where(i => i.GamesPlayed >= 1 && i.Character != "Unknown").OrderByDescending(i => i.WinPercent)
                    .ToArray();

            if (statsDataSource.Length != 0)
            {
                var winRateMin = statsDataSource.Min(i => i.WinPercent);
                var winRateMax = statsDataSource.Max(i => i.WinPercent);

                dataSource = statsDataSource
                    .Select(
                        i =>
                        {
                            var popularityNumeric = (decimal)((i.GamesPlayed + i.GamesBanned) * 13.875) /
                                                    latestStats.SitewideCharacterStatisticArray
                                                        .Sum(j => j.GamesPlayed + j.GamesBanned);
                            return new CharacterStatisticsViewData
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
                                WinPercentDelta = prevStatsDic != null &&
                                                  prevStatsDic.ContainsKey(i.Character)
                                    ? (decimal?)i.WinPercent - prevStatsDic[i.Character]
                                    : null,
                                Role = heroRoleDic.GetValueOrDefault(i.Character),
                                AliasCSV = heroAliasCsvDic.GetValueOrDefault(i.Character),
                            };
                        })
                    .OrderByDescending(i => i.WinPercent)
                    .ToList();
            }
        }

        return dataSource;
    }
}
