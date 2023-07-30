using Heroes.DataAccessLayer.Data;
using Heroes.ReplayParser;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Player = Heroes.DataAccessLayer.Models.Player;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class PlayersController : ControllerBase
{
    private readonly HeroesdataContext _dc;

    public PlayersController(HeroesdataContext dc)
    {
        this._dc = dc;
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
            return GetPlayerInfo(
                _dc.Players
                    .Include(x => x.LeaderboardRankings)
                    .Include(x => x.LeaderboardOptOut)
                    .Single(
                        i => i.BattleNetRegionId == battleNetRegionId && i.BattleNetSubId == battleNetSubId &&
                             i.BattleNetId == battleNetId && i.LeaderboardOptOut == null));
        }

        return null;
    }

    [HttpGet("{parameter1:int}/{parameter2}")]
    public object GetPlayerByBattleTag(int parameter1, string parameter2)
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
            return GetPlayerInfo(
                _dc.Players
                    .Include(x => x.LeaderboardRankings)
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
            return GetPlayerInfo(
                _dc.Players
                    .Include(x => x.LeaderboardRankings)
                    .Include(x => x.LeaderboardOptOut)
                    .Single(i => i.PlayerId == id && i.LeaderboardOptOut == null));
        }

        return null;
    }

    private object GetPlayerInfo(Player player)
    {
        return new
        {
            PlayerID = player.PlayerId,
            player.Name,
            LeaderboardRankings = player.LeaderboardRankings.Select(
                i => new
                {
                    GameMode = ((GameMode)i.GameMode).ToString(),
                    LeagueID = i.LeagueId,
                    i.LeagueRank,
                    CurrentMMR = i.CurrentMmr,
                }),
        };
    }
}
