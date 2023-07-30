using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Resources;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Linq;

namespace HotsLogsApi.BL.Migration.HeroRankings;

public class Helper : HelperBase<HeroRankingsResponse>
{
    private readonly AppUser _appUser;
    private readonly HeroRankingsArgs _args;

    public Helper(HeroRankingsArgs args, AppUser appUser, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
        _appUser = appUser;
    }

    public override HeroRankingsResponse MainCalculation()
    {
        var res = new HeroRankingsResponse();

        var heroName = _args.Hero;

        if (heroName == "Cho'gall")
        {
            heroName = "Cho";
        }

        var selectedCharacterId = Global.GetLocalizationAlias()
            .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero)
            .Single(r => r.PrimaryName == heroName).IdentifierId;
        var selectedSeason = _args.Season;
        var selectedGameMode = GetSelectedGameMode();
        var selectedBattleNetRegionId = _args.Region;

        var redisKey =
            "HOTSLogs:SitewideHeroLeaderboardStatisticsV2" +
            $":{selectedBattleNetRegionId}" +
            $":{selectedCharacterId}" +
            $":{selectedSeason}" +
            $":{selectedGameMode}";

        var stats = DataHelper.RedisCacheGet<SitewideHeroLeaderboardStatistics>(redisKey);
        if (stats != null)
        {
            res.LastUpdatedText =
                $"Last Updated {(DateTime.UtcNow - stats.LastUpdated).TotalDays:0.#} Days Ago";
        }

        // Adjust page for mobile devices
        //if (Request.Browser.IsMobileDevice)
        //{
        //    DataControlField fld = RadGridRankings.Columns.FindByDataField("PlayerIDMatchHistory");
        //    if (!(fld is null))
        //    {
        //        fld.Visible = false;
        //    }
        //}

        if (stats is null)
        {
            return res;
        }

        var q = stats.SitewideHeroLeaderboardStatisticArray;

        var flt = string.IsNullOrWhiteSpace(_args.Filter)
            ? q
            : q.Where(r => r.N.ContainsIgnoreCase(_args.Filter)).ToArray();

        if (_args.Sort?.direction is "asc" or "desc")
        {
            var desc = _args.Sort.direction is "desc";
            var prop = typeof(SitewideHeroLeaderboardStatistic).GetProperty(_args.Sort.active);
            if (prop != null)
            {
                flt = desc
                    ? flt.OrderByDescending(r => prop.GetValue(r)).ToArray()
                    : flt.OrderBy(r => prop.GetValue(r)).ToArray();
            }
        }

        res.Total = flt.Length;

        var skipCount = _args.Page.pageSize * _args.Page.pageIndex;

        var statsFlt = flt.Skip(skipCount).Take(_args.Page.pageSize).ToArray();
        res.Stats = statsFlt;

        /* Get updated premium supporter info, since the redis data can be up to 2 days old
         * and we want to reward premium supporters as soon as they become one.
         */
        UpdatePremiumInfo(res);

        return res;
    }

    private void UpdatePremiumInfo(HeroRankingsResponse res)
    {
        using var scope = Svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        var playerIds = res.Stats.Select(x => x.PID).ToArray();
        var users = dc.Net48Users
            .Where(z => z.IsBattleNetOauthAuthorized != 0)
            .Where(z => z.PlayerId.HasValue)
            .Where(z => z.Expiration > DateTime.UtcNow)
            .Where(z => z.PremiumSupporterSince.HasValue)
            .Where(x => playerIds.Contains(x.PlayerId.Value))
            .ToDictionary(r => r.PlayerId!.Value, z => z.PremiumSupporterSince!.Value);

        foreach (var stat in res.Stats)
        {
            if (!users.ContainsKey(stat.PID))
            {
                continue;
            }

            stat.TSS = users[stat.PID];
        }
    }

    private string SetAuthPlayerInfo()
    {
        if (_appUser is null)
        {
            return null;
        }

        var selectedGameMode = GetSelectedGameMode();

        using var scope = Svcp.CreateScope();
        var heroesEntity = HeroesdataContext.Create(scope);
        var user = heroesEntity.Net48Users
            .Include(x => x.Player)
            .ThenInclude(x => x.LeaderboardRankings)
            .ThenInclude(x => x.League)
            .SingleOrDefault(i => i.Id == _appUser.Id);
        var player = user?.Player;

        var leaderboardRanking = player?.LeaderboardRankings.SingleOrDefault(
            i => i.GameMode == selectedGameMode && i.LeagueId.HasValue);

        if (leaderboardRanking == null)
        {
            return null;
        }

        var minGamesRequirement = string.Format(
            LocalizedText.LeaderboardLeagueGamesPlayedRequirement,
            leaderboardRanking.League.RequiredGames);

        var rankOrNotAvailable =
            leaderboardRanking.LeagueRank?.ToString() ?? $"({minGamesRequirement})";

        var league = SiteMaster.GetLocalizedString(
            "GenericLeague",
            leaderboardRanking.LeagueId.ToString());

        var currentPlayerLeagueRank = string.Format(
            LocalizedText.LeaderboardCurrentPlayerRank,
            $"{league} {rankOrNotAvailable}");

        return currentPlayerLeagueRank;
    }
}
