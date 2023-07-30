using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HotsLogsApi.BL.Migration.Helpers;

public static class PlayerIdExtensions
{
    [CanBeNull]
    public static Net48User GetBnetUserOfPlayer(this int playerId, HeroesdataContext dc)
    {
        var user = dc.Net48Users
            .Include(x => x.Player)
            .ThenInclude(x => x.PlayerAltPlayerIdmainNavigations)
            .SingleOrDefault(
                x => x.IsBattleNetOauthAuthorized != 0ul &&
                     (x.PlayerId == playerId ||
                      x.Player.PlayerAltPlayerIdmainNavigations.Any(
                          y =>
                              y.PlayerIdalt == playerId)));
        return user;
    }
}
