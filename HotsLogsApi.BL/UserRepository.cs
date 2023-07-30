using Heroes.DataAccessLayer.Data;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotsLogsApi.BL;

public class UserRepository
{
    private readonly HeroesdataContext _dc;

    public UserRepository(HeroesdataContext dc)
    {
        _dc = dc;
    }

    public async Task<AppUser> GetUser(int userId, bool includeExtraAccountData = false)
    {
        var dbUser = await _dc.Net48Users
            .Include(r => r.Player)
            .ThenInclude(r => r.LeaderboardOptOut)
            .SingleOrDefaultAsync(x => x.Id == userId);
        var premiumExpiration = dbUser.Expiration;
        var isAdmin = (dbUser.Admin ?? 0) != 0;
        var region = dbUser.Player?.BattleNetRegionId ?? 1;
        var defaultGameMode = 8;
        var optOut = dbUser.Player?.LeaderboardOptOut is not null;
        var rc = new AppUser
        {
            Id = dbUser.Id,
            Email = dbUser.Email,
            IsBnetAuthorized = dbUser.IsBattleNetOauthAuthorized == 1,
            PremiumExpiration = premiumExpiration,
            IsPremium = premiumExpiration > DateTime.Now || isAdmin,
            SupporterSince = dbUser.PremiumSupporterSince,
            MainPlayerId = dbUser.PlayerId,
            Username = dbUser.Username,
            IsAdmin = isAdmin,
            Region = region,
            DefaultGameMode = defaultGameMode,
            IsOptOut = optOut,
        };

        if (includeExtraAccountData)
        {
            var player = dbUser.Player;

            List<PlayerProfileSlim> alts = null;
            PlayerProfileSlim main = null;
            int[] regions = null;

            if (player is not null)
            {
                main = new PlayerProfileSlim
                {
                    BattleTag = player.BattleTag,
                    Name = player.Name,
                    Region = player.BattleNetRegionId,
                    Id = player.PlayerId,
                };

                alts = _dc.PlayerAlts
                    .Include(x => x.PlayerIdaltNavigation)
                    .Where(x => x.PlayerIdmain == player.PlayerId)
                    .Select(x => x.PlayerIdaltNavigation)
                    .Select(
                        x => new PlayerProfileSlim
                        {
                            BattleTag = x.BattleTag,
                            Id = x.PlayerId,
                            Name = x.Name,
                            Region = x.BattleNetRegionId,
                        })
                    .ToList();

                regions = _dc.Players
                    .Where(i => i.Name == player.Name && i.BattleTag == player.BattleTag)
                    .Select(i => i.BattleNetRegionId).OrderBy(i => i).ToArray();
            }


            var ex = new AccountData
            {
                Main = main,
                Alts = alts ?? new List<PlayerProfileSlim>(),
                SubscriptionId = dbUser.Subscriptionid,
                Regions = regions ?? Array.Empty<int>(),
            };

            rc.AccountData = ex;
        }

        return rc;
    }
}
