using HelperCore;
using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL.Migration.PlayerSearch.Models;
using Microsoft.EntityFrameworkCore;
using ServiceStackReplacement;
using System.Collections.Immutable;
using System.Linq;

namespace HotsLogsApi.BL.Migration.PlayerSearch;

public class Helper
{
    private readonly PlayerSearchArgs _args;
    private readonly PlayerNameDictionary _playerNameDictionary;
    private readonly HeroesdataContext _dc;

    public Helper(PlayerSearchArgs args, PlayerNameDictionary playerNameDictionary, HeroesdataContext dc)
    {
        _args = args;
        _playerNameDictionary = playerNameDictionary;
        _dc = dc;
    }

    public PlayerSearchResponse MainCalculation()
    {
        var res = new PlayerSearchResponse();

        var (_, playerLookup) = _playerNameDictionary.GetDictionary();
        var playerNameSearch = _args.Name;
        if (string.IsNullOrWhiteSpace(playerNameSearch))
        {
            return res;
        }

        var leaderboardOptOut = _dc.LeaderboardOptOuts.Select(x => x.PlayerId).ToImmutableHashSet();
        _dc.Database.SetCommandTimeout(60);

        var playerSearchResults = playerLookup
            .Where(x => x.Key.EqualsIgnoreCase(playerNameSearch))
            .SelectMany(x => x)
            .ToArray();

        if (playerSearchResults.Length == 0)
        {
            playerSearchResults = playerLookup
                .Where(x => x.Key.ContainsIgnoreCase(playerNameSearch))
                .SelectMany(x => x)
                .Take(10000)
                .ToArray();
        }

        if (playerSearchResults.Length <= 0)
        {
            return res;
        }

        //if (playerSearchResults.Length == 1 && Request.QueryString["NoRedirect"] == null)
        //{
        //    Response.Redirect("~/Player/Profile?PlayerID=" + playerSearchResults[0], true);
        //}
        //else

        const string playerSearchResultsQuery =
            @"select p.PlayerID, p.BattleNetRegionId, p.`Name`, lr.LeagueID, lr.CurrentMMR, sum(pa.GamesPlayedTotal) as GamesPlayed, max(n48u.PremiumSupporterSince) as PremiumSupporterSince
                                from Player p
                                left join LeaderboardRanking lr on lr.PlayerID = p.PlayerID and lr.GameMode = 8
                                left join PlayerAggregate pa on pa.PlayerID = p.PlayerID
                                left join net48_users n48u on n48u.playerid = p.playerid
                                where p.PlayerID in (@playerIDs)
                                group by p.PlayerID, p.BattleNetRegionId, p.`Name`, lr.LeagueID, lr.CurrentMMR
                                order by lr.CurrentMMR desc";

        var cmdText = playerSearchResultsQuery.Replace(
            "@playerIDs",
            string.Join(",", playerSearchResults));

        var flt = _dc.PlayerSearchResultCustoms.FromSqlRaw(cmdText)
            .AsEnumerable()
            .Select(
                r => new PlayerSearchResult
                {
                    PlayerID = r.PlayerID,
                    Region = DataHelper.BattleNetRegionNames.ContainsKey(r.BattleNetRegionId)
                        ? DataHelper.BattleNetRegionNames[r.BattleNetRegionId]
                            .Replace(" Region", string.Empty)
                        : "Unknown",
                    Name = r.Name,
                    CurrentMMR = r.CurrentMMR,
                    GamesPlayed = (int?)r.GamesPlayed,
                    PremiumSupporterSince = r.PremiumSupporterSince,
                }).ToArray();

        res.Total = flt.Length;

        var skipCount = _args.Page.pageSize * _args.Page.pageIndex;

        var statsFlt = flt.Skip(skipCount).Take(_args.Page.pageSize).ToArray();

        res.Results = statsFlt;

        return res;
    }
}
