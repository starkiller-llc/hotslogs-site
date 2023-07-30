using HelperCore;
using HelperCore.RedisPOCOClasses;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.MapObjectives.Models;
using HotsLogsApi.BL.Migration.Models;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HotsLogsApi.BL.Migration.MapObjectives;

public class Helper : HelperBase<MapObjectivesResponse>
{
    private readonly MapObjectivesArgs _args;

    public Helper(MapObjectivesArgs args, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
    }

    public override MapObjectivesResponse MainCalculation()
    {
        using var scope = Svcp.CreateScope();

        var res = new MapObjectivesResponse
        {
            Tables = new List<MapObjectivesTable>(),
            Charts = new List<MapObjectivesChart>(),
        };

        var teams = GetTeamsIfTournamentSelected(scope);
        res.Teams = teams;

        var data =
            DataHelper.RedisCacheGet<TeamObjectiveRTOSummaryContainer>(
                "HOTSLogs:SitewideMapObjectives:" + _args.GameMode + ":" +
                _args.Map[0]);

        if (data == null)
        {
            return res;
        }

        foreach (var grd in data.TeamObjectiveRTOSummaryRadGrids)
        {
            var valueMin = grd.RadGridRows.Min(i => i.Value);
            var valueMax = grd.RadGridRows.Max(i => i.Value);

            var dataSource = grd.RadGridRows.Select(
                i => new MapObjectivesTableRow
                {
                    RowTitle = i.RowTitle,
                    GamesPlayed = i.GamesPlayed,
                    Value = SiteMaster.GetGaugeHtml(
                        i.Value,
                        valueMin,
                        valueMax,
                        formatString: grd.ValueFormatString),
                }).ToArray();

            var table = new MapObjectivesTable
            {
                Heading = grd.RadGridTitle,
                FieldName = grd.ValueColumnHeaderText,
                Data = dataSource,
            };

            res.Tables.Add(table);
        }

        foreach (var chrt in data.TeamObjectiveRTOSummaryRadHtmlCharts)
        {
            var rex = new Regex(@"Games Played:.*?(?<num>\d+)");
            int ExtractGamesPlayed(string s) => int.Parse(rex.Match(s).Groups["num"].Value);

            var rc1 = chrt.RadHtmlChartItems.Select(
                r => new RadChartDataRow<decimal, decimal>
                {
                    X = r.XValue,
                    WinPercent = r.YValue,
                    GamesPlayed = ExtractGamesPlayed(r.Tooltip),
                });

            var rc = new RadChartDef<decimal, decimal>
            {
                Type = RadChartType.Number,
                Name = chrt.XValueTitle,
                Data = rc1,
                MinY = 0,
                MaxY = 1,
            };
            var json1 = rc.ToJson();

            var chart = new MapObjectivesChart
            {
                Heading = chrt.RadHtmlChartTitle,
                JsonData = json1,
            };

            res.Charts.Add(chart);
        }

        return res;
    }
}
