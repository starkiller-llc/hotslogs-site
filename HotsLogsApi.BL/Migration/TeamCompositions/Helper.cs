using HelperCore;
using HelperCore.RedisPOCOClasses;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.TeamCompositions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotsLogsApi.BL.Migration.TeamCompositions;

public class Helper : HelperBase<TeamCompositionsResponse>
{
    private readonly TeamCompositionsArgs _args;

    public Helper(TeamCompositionsArgs args, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
    }

    public override TeamCompositionsResponse MainCalculation()
    {
        var res = new TeamCompositionsResponse();

        var selectedHero = _args.Hero;
        var selectedMapName = GetSelectedMaps()[0];
        var selectedHeroGrouping = (HeroGrouping)_args.Grouping;

        switch (selectedHeroGrouping)
        {
            case HeroGrouping.Hero:
                break;

            // ReSharper disable once RedundantCaseLabel
            case HeroGrouping.Role:
            default:
                selectedHero = "-1:HeroRoles";
                break;
        }

        var stats = DataHelper.RedisCacheGet<SitewideTeamCompositionStatistics>(
            "HOTSLogs:SitewideTeamCompositionStatistics:" + selectedMapName + ":" + selectedHero);

        res.Stats = GetTeamCompositionsDataSource(stats, selectedHeroGrouping);

        return res;
    }

    // TODO: I have a feeling the 'grouping' parameter should be used... something got lost along the way -- Aviad, 29-Oct-2022
#pragma warning disable IDE0060 // Remove unused parameter
    private TeamCompositionsRow[] GetTeamCompositionsDataSource(
        SitewideTeamCompositionStatistics stats,
        HeroGrouping grouping)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        var rc = Array.Empty<TeamCompositionsRow>();

        if (stats == null ||
            stats.SitewideTeamCompositionStatisticArray.Length <= 0)
        {
            return rc;
        }

        var totalGamesPlayed =
            stats.SitewideTeamCompositionStatisticArray
                .Sum(x => x.GamesPlayed);

        var statsByGamesPlayed =
            stats.SitewideTeamCompositionStatisticArray
                .OrderByDescending(x => x.GamesPlayed);

        var data = new List<SitewideTeamCompositionStatistic>();
        var runningSum = 0;
        foreach (var s in statsByGamesPlayed)
        {
            runningSum += s.GamesPlayed;
            if (runningSum > totalGamesPlayed * 0.95)
            {
                break;
            }

            data.Add(s);
        }

        var dataSource = data
            .OrderByDescending(i => i.WinPercent)
            .ToArray();

        var winRateMin = dataSource.Min(i => i.WinPercent);
        var winRateMax = dataSource.Max(i => i.WinPercent);

        var gamesPlayedMax = dataSource.Max(i => i.GamesPlayed);

        rc = dataSource.Select(
            i => new TeamCompositionsRow
            {
                Character1 = SiteMaster.GetLocalizedString("GenericHero", i.CharacterName1),
                Character2 = SiteMaster.GetLocalizedString("GenericHero", i.CharacterName2),
                Character3 = SiteMaster.GetLocalizedString("GenericHero", i.CharacterName3),
                Character4 = SiteMaster.GetLocalizedString("GenericHero", i.CharacterName4),
                Character5 = SiteMaster.GetLocalizedString("GenericHero", i.CharacterName5),
                CharacterImageURL1 = i.CharacterName1,
                CharacterImageURL2 = i.CharacterName2,
                CharacterImageURL3 = i.CharacterName3,
                CharacterImageURL4 = i.CharacterName4,
                CharacterImageURL5 = i.CharacterName5,
                GamesPlayed = SiteMaster.GetGaugeHtml(
                    i.GamesPlayed,
                    0,
                    gamesPlayedMax,
                    TeamCompHelper.HeroRoleColorsDictionary["Support"],
                    "N0"),
                WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winRateMin, winRateMax),
            }).ToArray();

        return rc;
    }

    private enum HeroGrouping
    {
        Role = 1,
        Hero = 2,
    }
}
