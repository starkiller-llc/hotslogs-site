using Heroes.DataAccessLayer.Data;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.Profile;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Player = Heroes.DataAccessLayer.Models.Player;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class PlayerDetailsController : ControllerBase
{
    private readonly HeroesdataContext heroesEntity;
    private readonly ProfileHelper _profileHelper;

    public PlayerDetailsController(HeroesdataContext heroesEntity, ProfileHelper profileHelper)
    {
        this.heroesEntity = heroesEntity;
        _profileHelper = profileHelper;
    }

    [HttpGet("bnet")]
    public object GetPlayerByBattleNetID(
        [FromQuery] int battleNetRegionId,
        [FromQuery] int battleNetSubId,
        [FromQuery] int battleNetId)
    {
        if (heroesEntity.Players
            .Include(x => x.LeaderboardOptOut)
            .Any(
                i => i.BattleNetRegionId == battleNetRegionId && i.BattleNetSubId == battleNetSubId &&
                     i.BattleNetId == battleNetId && i.LeaderboardOptOut == null))
        {
            return GetPlayerInfo(
                heroesEntity.Players
                    .Include(x => x.PlayerMmrMilestoneV3s)
                    .Include(x => x.LeaderboardRankings)
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

        if (heroesEntity.Players
            .Include(x => x.LeaderboardOptOut)
            .Any(
                i => i.Name == name && i.BattleTag == battleTag && i.BattleNetRegionId == parameter1 &&
                     i.LeaderboardOptOut == null))
        {
            return GetPlayerInfo(
                heroesEntity.Players
                    .Include(x => x.PlayerMmrMilestoneV3s)
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
        if (heroesEntity.Players
            .Include(x => x.LeaderboardOptOut)
            .Any(i => i.PlayerId == id && i.LeaderboardOptOut == null))
        {
            return GetPlayerInfo(
                heroesEntity.Players
                    .Include(x => x.LeaderboardRankings)
                    .Include(x => x.PlayerMmrMilestoneV3s)
                    .Include(x => x.LeaderboardOptOut)
                    .Single(i => i.PlayerId == id && i.LeaderboardOptOut == null));
        }

        return null;
    }

    private object GetPlayerInfo(Player player)
    {
        var queryString = Request.Query;

        if (!queryString.ContainsKey("GameMode") ||
            !int.TryParse(queryString["GameMode"], out var selectedGameMode))
        {
            selectedGameMode = -1;
        }

        DateTime? filterProfileDateTimeBegin = null;
        if (queryString.ContainsKey("PastDaysToCalculate"))
        {
            filterProfileDateTimeBegin = DateTime.UtcNow.AddDays(int.Parse(queryString["PastDaysToCalculate"]));
        }

        return _profileHelper.GetPlayerProfile(
            player,
            selectedGameMode > 0 ? (GameMode)selectedGameMode : null,
            filterProfileDateTimeBegin);
    }
}
