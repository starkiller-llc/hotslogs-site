using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.Reputation.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace HotsLogsApi.BL.Migration.Reputation;

public class Helper : HelperBase<ReputationResponse>
{
    private readonly ReputationArgs _args;

    public Helper(ReputationArgs args, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
    }

    public override ReputationResponse MainCalculation()
    {
        using var scope = Svcp.CreateScope();
        var res = new ReputationResponse();

        var regionId = _args.Region;

        var dc = HeroesdataContext.Create(scope);

        var source = dc.Reputations.Include(x => x.Player);

        var q = from x in source
                where x.Player.BattleNetRegionId == regionId && x.Reputation1 != 0
                orderby x.Reputation1 descending
                select x;

        var flt = string.IsNullOrWhiteSpace(_args.Filter)
            ? q
            : q.Where(r => r.Player.Name.Contains(_args.Filter));

        if (_args.Sort?.direction is "asc" or "desc")
        {
            var desc = _args.Sort.direction is "desc";
            flt = _args.Sort.active switch
            {
                "Name" => desc ? flt.OrderByDescending(x => x.Player.Name) : flt.OrderBy(x => x.Player.Name),
                "Reputation" => desc ? flt.OrderByDescending(x => x.Reputation1) : flt.OrderBy(x => x.Reputation1),
                _ => flt,
            };
        }

        res.Total = flt.Count();

        var skipCount = _args.Page.pageSize * _args.Page.pageIndex;

        var repChart = flt
            .Skip(skipCount)
            .Take(_args.Page.pageSize)
            .Select(
                x => new ReputationLeaderboardEntry
                {
                    PlayerId = x.PlayerId,
                    Name = x.Player.Name,
                    Reputation = x.Reputation1,
                })
            .ToArray();

        res.Stats = repChart;

        return res;
    }
}
