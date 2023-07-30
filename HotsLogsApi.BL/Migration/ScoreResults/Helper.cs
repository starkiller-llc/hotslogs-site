using HelperCore;
using HelperCore.RedisPOCOClasses;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.ScoreResults.Models;
using HotsLogsApi.BL.Resources;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HotsLogsApi.BL.Migration.ScoreResults;

public class Helper : HelperBase<ScoreResultsResponse>
{
    private readonly ScoreResultsArgs _args;

    public Helper(ScoreResultsArgs args, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
    }

    public override ScoreResultsResponse MainCalculation()
    {
        using var scope = Svcp.CreateScope();
        var res = new ScoreResultsResponse();

        var teams = GetTeamsIfTournamentSelected(scope);
        res.Teams = teams;

        // Get selected Replay Date Times
        var selectedPatchesOrWeeks = SelectedPatchesOrWeeks();
        var selectedGameMode = GetSelectedGameMode();
        var selectedLeagues = GetSelectedLeagues();
        var selectedMaps = GetSelectedMaps().ToDictionary(x => x, _ => true);

        SitewideCharacterStatistics heroStats = null;
        SitewideMapStatistics mapStats = null;

        foreach (var dt in selectedPatchesOrWeeks)
        {
            foreach (var league in selectedLeagues)
            {
                if (selectedMaps.Count == 1 && selectedMaps.Keys.Single() == "-1")
                {
                    heroStats += DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                        $"HOTSLogs:SitewideCharacterStatisticsV2:{dt}:{league}:{selectedGameMode}");
                }
                else
                {
                    mapStats += DataHelper.RedisCacheGet<SitewideMapStatistics>(
                        $"HOTSLogs:SitewideMapStatisticsV2:{dt}:{league}:{selectedGameMode}");
                }
            }
        }

        if (mapStats != null)
        {
            var bySelectedMap = mapStats.SitewideMapStatisticArray
                .Where(i => selectedMaps.ContainsKey(i.Map))
                .GroupBy(i => i.Map)
                .Select(
                    i => new SitewideCharacterStatistics
                    {
                        SitewideCharacterStatisticArray = i.Select(
                            j => new SitewideCharacterStatistic
                            {
                                AverageLength = j.AverageLength,
                                AverageScoreResult = j.AverageScoreResult,
                                Character = j.Character,
                                GamesPlayed = j.GamesPlayed,
                                HeroPortraitURL = Global.HeroPortraitImages[j.Character],
                                WinPercent = j.WinPercent,
                            }).ToArray(),
                    });

            foreach (var s in bySelectedMap)
            {
                heroStats += s;
            }

            if (heroStats != null)
            {
                heroStats = new SitewideCharacterStatistics
                {
                    DateTimeBegin = mapStats.DateTimeBegin,
                    DateTimeEnd = mapStats.DateTimeEnd,
                    GameMode = mapStats.GameMode,
                    LastUpdated = mapStats.LastUpdated,
                    League = mapStats.League,
                    SitewideCharacterStatisticArray = heroStats.SitewideCharacterStatisticArray,
                };
            }
        }

        var rex = new Regex(@"<!-- \$val:(?<val>.*?)\$ -->");

        decimal GetValue(string s)
        {
            var match = rex.Match(s ?? string.Empty);
            if (match.Success && decimal.TryParse(match.Groups["val"].Value, out var d))
            {
                return d;
            }

            return decimal.MinValue;
        }

        StatsResultType[] calcStatsGeneric;
        Dictionary<string, StatsResultType[]> roleDataDic = null;
        var emptyStats = Array.Empty<StatsResultType>();
        var calcDic =
            new List<(string key, Func<StatsResultType, decimal> sortKeySelector)>
            {
                ("Tank", x => GetValue(x.DamageTaken)),
                ("Bruiser", x => GetValue(x.DamageTaken)),
                ("Healer", x => GetValue(x.Healing)),
                ("Support", x => GetValue(x.Healing)),
                ("Melee Assassin", x => GetValue(x.Takedowns)),
                ("Ranged Assassin", x => GetValue(x.HeroDamage)),
            };

        if (heroStats != null && heroStats.SitewideCharacterStatisticArray.Length > 0)
        {
            res.LastUpdatedText = string.Format(
                LocalizedText.GenericLastUpdatedMinutesAgo,
                (int)(DateTime.UtcNow - heroStats.LastUpdated).TotalMinutes);
        }
        else
        {
            calcStatsGeneric = Array.Empty<StatsResultType>();
            roleDataDic = calcDic.ToDictionary(x => x.key, x => emptyStats);
            goto End;
        }

        // Gather Hero Role dictionary
        var
            heroRoleConcurrentDictionary = Global.GetHeroRoleConcurrentDictionary();

        var helper = new ScoreHelper();

        calcStatsGeneric = helper.CalcStatsGeneric(heroStats.SitewideCharacterStatisticArray);
        Array.ForEach(
            calcStatsGeneric,
            x =>
            {
                x.GameMode = _args.GameModeEx;
                x.Event = _args.Tournament;
            });

        // Populate Role Statistics Tabs
        var roleStatistics = heroStats.SitewideCharacterStatisticArray
            .Where(i => heroRoleConcurrentDictionary.ContainsKey(i.Character))
            .GroupBy(i => heroRoleConcurrentDictionary[i.Character])
            .ToDictionary(i => i.Key, i => i.ToArray());

        roleDataDic = calcDic
            .Where(x => roleStatistics.ContainsKey(x.key))
            .Select(
                x =>
                {
                    var (key, sortKeySelector) = x;
                    var stats = helper.CalcStatsRole(roleStatistics[key], sortKeySelector);
                    Array.ForEach(
                        stats,
                        z =>
                        {
                            z.GameMode = _args.GameModeEx;
                            z.Event = _args.Tournament;
                        });
                    return (key, stats);
                })
            .ToDictionary(r => r.key, r => r.stats);
        End:

        res.GeneralStats = calcStatsGeneric;
        res.RoleStats = roleDataDic;

        return res;
    }
}
