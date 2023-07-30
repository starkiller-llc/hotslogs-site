using HelperCore;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using HotsLogsApi.BL.Resources;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HotsLogsApi.BL.Migration.Helpers;

public class BnetHelper
{
    private readonly HeroesdataContext _dc;
    private readonly MyDbWrapper _redisClient;

    public Dictionary<int, string> BattleNetOAuthBaseUrl { get; } = new()
    {
        { 1, @"https://us.battle.net/oauth/" },
        { 2, @"https://eu.battle.net/oauth/" },
        { 3, @"https://kr.battle.net/oauth/" },
        { 5, @"https://www.battlenet.com.cn/oauth/" },
    };

    public Dictionary<int, string> BnetUserInfoEndpoint { get; } = new()
    {
        { 1, @"https://us.battle.net/oauth/userinfo" },
        { 2, @"https://eu.battle.net/oauth/userinfo" },
        { 3, @"https://kr.battle.net/oauth/userinfo" },
        { 5, @"https://www.battlenet.com.cn/oauth/userinfo" },
    };

    public BnetHelper(HeroesdataContext dc, MyDbWrapper redisClient)
    {
        this._dc = dc;
        this._redisClient = redisClient;
    }

    public async Task<(bool success, int code, string error)> BnetAuth(
        AppUser appUser,
        string code,
        BnetOptions bnetOptions)
    {
        var redirectUrl = bnetOptions.BattleNetOAuthRedirectURI;
        var postValues = new Dictionary<string, string>
        {
            ["redirect_uri"] = redirectUrl,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
        };

        // This is good, it forces the user to log in with us, and to choose their
        // battle.net region, but the battle tag they choose should be irrelevant
        // because they are now authenticating with blizzard so we would know
        // their battle tag.
        var playerId = appUser.MainPlayerId;
        if ((playerId ?? -1) == -1)
        {
            return (false, 1, null);
            //Response.Redirect("~/Account/ChooseBattleNetId", true);
        }

        var playerx = _dc.Players.Single(i => i.PlayerId == playerId);
        var playerBattleNetRegionId = playerx.BattleNetRegionId;

        string authenticatedBattleTag;
        using (var webClient = new HttpClient())
        {
            // See https://develop.battle.net/documentation/guides/using-oauth/client-credentials-flow
            // and https://stackoverflow.com/a/35442984/235648
            var idAndSecret = $"{bnetOptions.BnetOAuthKey}:{bnetOptions.BnetOAuthSecret}";
            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(idAndSecret));
            webClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", encodedCredentials);

            try
            {
                var tokenUrl = $"{BattleNetOAuthBaseUrl[playerBattleNetRegionId]}token";
                var content = new FormUrlEncodedContent(postValues);
                var uploadValues = await webClient.PostAsync(tokenUrl, content);
                var value = await uploadValues.Content.ReadAsStringAsync();

                //var value = Encoding.UTF8.GetString(uploadValues);

                var battleNetTokenResponse = JsonConvert.DeserializeObject<BattleNetTokenResponse>(value);
                var accessToken = battleNetTokenResponse?.access_token;

                var authPlayerInfo = await webClient.GetStringAsync(
                    $"{BnetUserInfoEndpoint[playerBattleNetRegionId]}?access_token={accessToken}");

                // {"sub":"1831861","id":1831861,"battletag":"Skywalker#23595"}
                var battleNetBattleTagResponse =
                    JsonConvert.DeserializeObject<BattleNetBattleTagResponse>(authPlayerInfo);
                authenticatedBattleTag = battleNetBattleTagResponse?.battletag;
            }
            catch (WebException exception)
            {
                if (exception.Status != WebExceptionStatus.ProtocolError ||
                    ((HttpWebResponse)exception.Response).StatusCode != HttpStatusCode.ServiceUnavailable)
                {
                    throw;
                }

                // Battle.Net is likely down
                var error =
                    @"<strong>Error: </strong>Couldn't authenticate through Battle.Net.  Usually this is because of Battle.net problems or maintenance.  Check <a href=""https://twitter.com/BlizzardCS"">@BlizzardCS</a> for potential information, and please try again later.";
                return (false, 2, error);
            }
        }

        // if (true /*playerBattleTag == authenticatedBattleTag*/)

        // Successfully Authorized and Verified
        // Master.GlobalInstance.IsBattleNetOAuthAuthorized = true;
        var (bTagName, bTagNumber) = authenticatedBattleTag.ParseBattleTag();
        var user = _dc.Net48Users.Single(x => x.Id == appUser.Id);
        var player = _dc.Players
            .Include(x => x.Net48Users)
            .SingleOrDefault(x => x.Name == bTagName && x.BattleTag == bTagNumber);

        if (player == null)
        {
            var error =
                @"<strong>Error: </strong>We can't update your player because we can't find him in any of the replays in our database. Please upload at least one replay with your player and try again.";
            return (false, 3, error);
        }

        if (user.IsBattleNetOauthAuthorized != 0ul && user.PlayerId.HasValue)
        {
            // We are already b.net authorized, so add an alt
            var existingAlts = _dc.PlayerAlts
                .Where(x => x.PlayerIdmain == user.PlayerId.Value).ToList();

            if (user.PlayerId.Value != player.PlayerId &&
                existingAlts.All(x => x.PlayerIdalt != player.PlayerId))
            {
                var alt = new PlayerAlt
                {
                    PlayerIdmain = user.PlayerId.Value,
                    PlayerIdalt = player.PlayerId,
                };
                _dc.PlayerAlts.Add(alt);
            }
        }
        else
        {
            // This is our first b.net auth, add a main
            user.PlayerId = player.PlayerId;
            user.IsBattleNetOauthAuthorized = 1ul;

            // Since we're adding a main, remove all alts from it (safety)
            var altsToRemove = _dc.PlayerAlts.Where(x => x.PlayerIdmain == player.PlayerId)
                .ToList();
            _dc.PlayerAlts.RemoveRange(altsToRemove);
        }

        /*
         * Update any other users who have selected this battle tag, and do the following
         * 1. Remove the association
         * 2. Set those users as not b.net authorized (if they were)
         *
         * Also, if it was marked as an alt for any other player, remove that association.
         */

        var disown = player.Net48Users.Where(x => x.Id != user.Id && x.PlayerId == user.PlayerId.Value).ToList();
        disown.ForEach(
            x =>
            {
                x.PlayerId = null;
                x.IsBattleNetOauthAuthorized = 0ul;
            });

        var altsToRemove2 = _dc.PlayerAlts
            .Where(
                x =>
                    x.PlayerIdmain != user.PlayerId.Value && // Not our own alt
                    x.PlayerIdalt == player.PlayerId)
            .ToList();

        _dc.PlayerAlts.RemoveRange(altsToRemove2);

        await _dc.SaveChangesAsync();

        return (true, 0, null);
        //Response.Redirect("~/Account/Manage", true);
#if false
                else
                {
                    // Succesfully Authorized, but not as the Logged In User
                    pErrorMessage.InnerHtml =
                        $@"<strong>Error: </strong>Your HOTS Logs player account is {playerBattleTag}, but you logged in to Battle.Net as {authenticatedBattleTag}.  Please go directly to <a href=""http://www.battle.net"" target=""_blank"">Battle.Net</a> and 'Log Out' using the link in the top right.  After you have logged out, <a href=""/Account/Manage"">follow this link</a> to try again.";
                }
#endif
    }

    public (bool, int, string) ChooseBnetId(AppUser appUser, string battleTag, int region)
    {
        if (!battleTag.TryParseBattleTag(out var name, out var tag))
        {
            return (false, 1, LocalizedText.ChooseBattleNetIdErrorMessage1);
        }

        var count = _dc.Players
            .Count(
                i => i.Name == name &&
                     i.BattleTag == tag &&
                     i.BattleNetRegionId == region);

        if (count == 0)
        {
            return (false, 2, LocalizedText.ChooseBattleNetIdErrorMessage2);
        }

        var player = _dc.Players
            .Include(x => x.Net48Users)
            .Include(x => x.LeaderboardOptOut)
            .Single(
                i =>
                    i.Name == name &&
                    i.BattleTag == tag &&
                    i.BattleNetRegionId == region);

        TimeSpan? banDuration = null;
        if (player.PlayerBanned != null)
        {
            // Player has been silenced by Blizzard; let's see if the ban time has passed
            banDuration = _redisClient.GetTimeToLive($"HOTSLogs:SilencedPlayerID:{player.PlayerId}");

            if (!banDuration.HasValue)
            {
                player.PlayerBanned = null;

                if (!_redisClient.ContainsKey(
                        $"HOTSLogs:SilencedPlayerIDAndExistingLeaderboardOptOut:{player.PlayerId}") &&
                    player.LeaderboardOptOut != null)
                {
                    _dc.LeaderboardOptOuts.Remove(player.LeaderboardOptOut);
                }
            }
        }

        var notAuthorized = player.Net48Users.Any(x => x.IsBattleNetOauthAuthorized != 0ul);

        if (notAuthorized)
        {
            return (false, 3, "The BattleTag you entered belongs to someone else.");
        }

        if (banDuration.HasValue)
        {
            var error =
                "The BattleTag you entered has recently been Silenced by Blizzard. " +
                "In an effort to improve the community, these players will be temporarily " +
                "hidden on HOTS Logs, and will not be eligible for the HOTS Logs Leaderboard. " +
                "This will last for the duration of the Silence, plus an additional 15 days. " +
                "You are welcome to use the rest of HOTSLogs.com during this time. " +
                "After this player has been restored to good standing, you are welcome to rejoin. " +
                "You will need to verify your account on the 'Manage Account' page and set your " +
                "Profile to 'Public' if you want to be re-added to the Leaderboard. " +
                "If your account is no longer Silenced, you will be able to reclaim this " +
                $"account in: {SiteMaster.GetUserFriendlyTimeSpanString(banDuration.Value)}";

            return (false, 4, error);
        }

        var user = _dc.Net48Users
            .Include(x => x.Player)
            .SingleOrDefault(i => i.Id == appUser.Id);

        if (user == null)
        {
            return (false, 5, "User not found in user database");
        }

        user.Player = player;

        try
        {
            _dc.SaveChanges();
        }
        catch
        {
            // The only exception I've seen here is when the user double clicks and the database attempts to write a duplicate key
            // We will still want to redirect them to their profile in this case
            return (false, 6, "Database key conflict, it's probably nothing.");
        }

        return (true, 0, null);
    }

    public async Task MakeMain(AppUser appUser, int newMainId)
    {
        var user = await _dc.Net48Users
            .Include(x => x.Player)
            .SingleAsync(x => x.Id == appUser.Id);
        var main = user.Player;
        var alts = _dc.PlayerAlts.Where(x => x.PlayerIdmain == main.PlayerId).ToList();

        _dc.PlayerAlts.RemoveRange(alts);

        var newAltIds = new List<int> { main.PlayerId };
        alts.ForEach(x => newAltIds.Add(x.PlayerIdalt));
        newAltIds.Remove(newMainId);

        user.PlayerId = newMainId;
        var newAlts = newAltIds.Select(
            x => new PlayerAlt
            {
                PlayerIdmain = newMainId,
                PlayerIdalt = x,
            });
        _dc.PlayerAlts.AddRange(newAlts);

        await _dc.SaveChangesAsync();
    }

    public async Task RemoveAlt(AppUser appUser, int altId)
    {
        var user = _dc.Net48Users
            .Include(x => x.Player)
            .Single(x => x.Id == appUser.Id);
        var main = user.Player;
        var altToRemove = _dc.PlayerAlts.SingleOrDefault(
            x => x.PlayerIdmain == main.PlayerId && x.PlayerIdalt == altId);

        _dc.PlayerAlts.Remove(altToRemove);

        await _dc.SaveChangesAsync();
    }

    // ReSharper disable UnusedAutoPropertyAccessor.Local
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Local
    private class BattleNetTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public int accountId { get; set; }
    }

    private class BattleNetBattleTagResponse
    {
        public string battletag { get; set; }
    }
    // ReSharper restore UnusedMember.Local
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedAutoPropertyAccessor.Local
}
