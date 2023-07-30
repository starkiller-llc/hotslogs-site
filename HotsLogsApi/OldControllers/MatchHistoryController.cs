using HelperCore;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using HotsLogsApi.BL.Migration.MatchHistory;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class MatchHistoryController : ControllerBase
{
    private readonly HeroesdataContext _dc;
    private readonly MatchHistoryHelper _matchHistoryHelper;

    public MatchHistoryController(HeroesdataContext dc, MatchHistoryHelper matchHistoryHelper)
    {
        this._dc = dc;
        _matchHistoryHelper = matchHistoryHelper;
    }

    [HttpGet("bnet")]
    public object GetPlayerByBattleNetID(
        [FromQuery] int battleNetRegionId,
        [FromQuery] int battleNetSubId,
        [FromQuery] int battleNetId)
    {
        if (_dc.Players
            .Include(x => x.LeaderboardOptOut)
            .Any(
                i => i.BattleNetRegionId == battleNetRegionId && i.BattleNetSubId == battleNetSubId &&
                     i.BattleNetId == battleNetId && i.LeaderboardOptOut == null))
        {
            return GetMatchHistory(
                _dc,
                _dc.Players
                    .Include(x => x.LeaderboardOptOut)
                    .Single(
                        i => i.BattleNetRegionId == battleNetRegionId && i.BattleNetSubId == battleNetSubId &&
                             i.BattleNetId == battleNetId && i.LeaderboardOptOut == null));
        }

        return null;
    }

    [HttpGet("tag")]
    public object GetPlayerByBattleTag([FromQuery] int parameter1, [FromQuery] string parameter2)
    {
        var nameAndBattleTagSplit = parameter2.Split('_');
        if (nameAndBattleTagSplit.Length != 2)
        {
            return null;
        }

        if (!int.TryParse(nameAndBattleTagSplit[1], out var battleTag))
        {
            return null;
        }

        var name = nameAndBattleTagSplit[0];

        if (_dc.Players
            .Include(x => x.LeaderboardOptOut)
            .Any(
                i => i.Name == name && i.BattleTag == battleTag && i.BattleNetRegionId == parameter1 &&
                     i.LeaderboardOptOut == null))
        {
            return GetMatchHistory(
                _dc,
                _dc.Players
                    .Include(x => x.LeaderboardOptOut)
                    .Single(
                        i => i.Name == name && i.BattleTag == battleTag && i.BattleNetRegionId == parameter1 &&
                             i.LeaderboardOptOut == null));
        }

        return null;
    }

    [HttpGet("{id:int}")]
    public object GetPlayerByID(int id)
    {
        if (_dc.Players
            .Include(x => x.LeaderboardOptOut)
            .Any(i => i.PlayerId == id && i.LeaderboardOptOut == null))
        {
            return GetMatchHistory(
                _dc,
                _dc.Players
                    .Include(x => x.LeaderboardOptOut)
                    .Single(i => i.PlayerId == id && i.LeaderboardOptOut == null));
        }

        return null;
    }

    private object GetMatchHistory(HeroesdataContext heroesEntity, Player player)
    {
        var queryString = Request.Query;

        int[] otherPlayerIDs = null;
        if (queryString.ContainsKey("OtherPlayerIDs"))
        {
            otherPlayerIDs = ((string)queryString["OtherPlayerIDs"]).Split(',').Take(9).Select(int.Parse)
                .OrderBy(i => i).ToArray();
        }

        var playerMatchHistory = _matchHistoryHelper.GetMatchHistory(player, otherPlayerIDs);

        // Don't accidentally share Custom games, or other unusual uploads
        playerMatchHistory.PlayerMatches = playerMatchHistory.PlayerMatches
            .Where(i => DataHelper.GameModeWithStatistics.Any(j => j == (int)i.GM)).ToArray();

        return playerMatchHistory;
    }
}
