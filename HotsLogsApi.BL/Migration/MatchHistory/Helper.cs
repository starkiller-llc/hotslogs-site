using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.MatchHistory.Models;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Player = Heroes.DataAccessLayer.Models.Player;

namespace HotsLogsApi.BL.Migration.MatchHistory;

public class Helper : HelperBase<MatchHistoryResponse>, IDisposable
{
    private const int RedisPlayerProfileCacheExpireInMinutes = 10;
    private const int ReplayFileLifetimeInDays = 30;

    private readonly AppUser _appUser;
    private readonly MatchHistoryArgs _args;
    private readonly HeroesdataContext _heroesEntity;
    private readonly int _playerId = -1;
    private readonly IServiceScope _scope;
    private readonly MatchHistoryHelper _mhh;
    private readonly EventHelper _eventHelper;

    public Helper(MatchHistoryArgs args, AppUser appUser, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
        _appUser = appUser;
        _scope = Svcp.CreateScope();
        _heroesEntity = HeroesdataContext.Create(_scope);
        _mhh = _scope.ServiceProvider.GetRequiredService<MatchHistoryHelper>();
        _eventHelper = _scope.ServiceProvider.GetRequiredService<EventHelper>();
        if (_args.EventId is null)
        {
            _playerId = _args.PlayerId ?? _appUser?.MainPlayerId ?? -1;
        }
    }

    public void Dispose()
    {
        _scope.Dispose();
    }

    public override MatchHistoryResponse MainCalculation()
    {
        var res = new MatchHistoryResponse();
        if (_playerId == -1 && _appUser is null)
        {
            res.Unauthorized = true;
            return res;
        }

        var eventId = _args.EventId ?? -1;

        var showShareColumn = _appUser is not null && eventId == -1 ? null : (bool?)false;

        Event eventEntity = null;
        PlayerMatchHistory matchHistory;
        if (_playerId == -1 && eventId != -1)
        {
            // Overall Event Match History
            eventEntity = _heroesEntity.Events
                .Include(x => x.EventIdparentNavigation)
                .Include(x => x.InverseEventIdparentNavigation)
                .SingleOrDefault(i => i.EventId == eventId);

            if (eventEntity == null || eventEntity.IsEnabled == 0)
            {
                //// ReSharper disable Html.PathError
                //Response.Redirect("/ang/default", true);
                return res;
            }

            matchHistory = _mhh.GetMatchHistory(eventEntity);

            if (matchHistory is null)
            {
                //Response.Redirect("/ang/default", true);

                //// ReSharper restore Html.PathError
                return res;
            }

            //MatchHistoryGrid.Columns.FindByDataField("Character").Visible = false;
            //MatchHistoryGrid.Columns.FindByDataField("CharacterWithoutHyperLink").Visible = true;
            //MatchHistoryGrid.Columns.FindByDataField("MMRBefore").Visible = false;
            //MatchHistoryGrid.Columns.FindByDataField("MMRChange").Visible = false;

            res.HideMessageLineVersion2 = true;
            //pMatchHistoryMessageLine2Version2.Visible = false;

            var literalHeaderLinksText = "";

            if (eventEntity.EventIdparentNavigation != null)
            {
                literalHeaderLinksText +=
                    $@"
<p>Parent Event: 
    <a href=""/Event/MatchHistory?EventID={eventEntity.EventIdparentNavigation.EventId}"">
        {HttpUtility.HtmlEncode(eventEntity.EventIdparentNavigation.EventName)}
    </a>
</p>";
            }

            if (eventEntity.InverseEventIdparentNavigation.Any(i => i.IsEnabled != 0))
            {
                var join = string.Join(
                    ", ",
                    eventEntity.InverseEventIdparentNavigation.Where(i => i.IsEnabled != 0)
                        .OrderBy(i => i.EventName).Select(
                            i =>
                                $@"<span><a href=""/Event/MatchHistory?EventID={i.EventId}"">{HttpUtility.HtmlEncode(i.EventName)}</a></span>"));
                literalHeaderLinksText += $@"<p>Summarizes data from the following Events: {join}</p>";
            }

            res.LiteralHeaderLinks = literalHeaderLinksText;
        }
        else
        {
            var player = _heroesEntity.Players
                .Include(x => x.LeaderboardOptOut)
                .SingleOrDefault(i => i.PlayerId == _playerId);

            if (player is null)
            {
                return res;
            }

            var mainUser = _playerId.GetBnetUserOfPlayer(_heroesEntity);
            var weAreHim = (mainUser?.Id ?? -2) == _appUser?.Id;

            var heOptedOut = player.LeaderboardOptOut is not null;

            if (heOptedOut && !weAreHim)
            {
                res.Unauthorized = true;
                return res;
            }

            // Check if the user is requesting to view their Match History with other players
            int[] otherPlayerIDs = null;

            var premium = _appUser?.IsPremium ?? false;
            var hasOthers = _args.OtherPlayerIds is { Length: > 0 };

            if (hasOthers && premium)
            {
                otherPlayerIDs = _args.OtherPlayerIds.OrderBy(i => i).ToArray();
            }

            res.OtherPlayerIds = otherPlayerIDs;

            // Get their Match History
            matchHistory = _mhh.GetMatchHistory(
                player,
                otherPlayerIDs);

            var title = matchHistory.PlayerName;

            if (matchHistory.OtherPlayerNames != null)
            {
                title += " and " + string.Join(", ", matchHistory.OtherPlayerNames);
            }

            res.Title = title;

            showShareColumn ??= weAreHim;

            res.ShowShareColumn = showShareColumn;

            if (mainUser?.Premium == 1 && mainUser.Expiration > DateTime.UtcNow)
            {
                res.PremiumSupporterSince = mainUser.PremiumSupporterSince;
                //Master.MasterImagePremiumAccountTitle.Visible = true;
                //Master.MasterImagePremiumAccountTitle.Attributes["title"] = "HOTS Logs Supporter since " +
                //                                                            user.PremiumSupporterSince?.ToString("d");
            }

            if (eventId != -1)
            {
                // Player Match History for a specific Event
                res.HideMessageLineVersion2 = true;
                //pMatchHistoryMessageLine2Version2.Visible = false;

                eventEntity = _heroesEntity.Events.SingleOrDefault(i => i.EventId == eventId);

                if (eventEntity == null || eventEntity.IsEnabled == 0)
                {
                    return res;
                    //Response.Redirect("/ang/default", true);
                }

                var literalHeaderLinksText =
                    "<h2>Event: " + HttpUtility.HtmlEncode(eventEntity.EventName) + "</h2>";

                if (eventEntity.EventIdparentNavigation != null)
                {
                    literalHeaderLinksText += string.Format(
                        @"<p>Parent Event: <a href=""/Player/MatchHistory?PlayerID={2}&EventID={0}"">{1}</a></p>",
                        eventEntity.EventIdparentNavigation.EventId,
                        HttpUtility.HtmlEncode(eventEntity.EventIdparentNavigation.EventName),
                        _playerId);
                }

                if (eventEntity.InverseEventIdparentNavigation.Any(i => i.IsEnabled != 0))
                {
                    literalHeaderLinksText +=
                        @"<p>
                                    Summarizes data from the following Events: " +
                        string.Join(
                            ", ",
                            eventEntity.InverseEventIdparentNavigation.Where(i => i.IsEnabled != 0)
                                .OrderBy(i => i.EventName).Select(
                                    i => string.Format(
                                        @"<span><a href=""/Player/MatchHistory?PlayerID={2}&EventID={0}"">{1}</a></span>",
                                        i.EventId,
                                        HttpUtility.HtmlEncode(i.EventName),
                                        _playerId))) +
                        @"</p>";
                }

                literalHeaderLinksText += @"<p><a href=""/Player/MatchHistory?PlayerID=" + _playerId +
                                          @""">View Overall Match History</a></p>";

                // Since many of these pages are small, let's hide the middle advertisement
                //advertisementBannerMonkeyBrokerTopReplacement.Visible = false;
                res.LiteralHeaderLinks = literalHeaderLinksText;
            }
        }

        if (matchHistory?.PlayerMatches is null)
        {
            return res;
        }

        var gm = GetSelectedGameMode();
        var selectedGameMode = Global.IsEventGameMode(gm) ? GameMode.Event : (GameMode)gm;
        var selectedMatches = matchHistory.PlayerMatches
            .Where(i => i.GM == selectedGameMode || selectedGameMode == GameMode.Event).ToArray();

        if (_playerId != -1 && eventId != -1)
        {
            // Player Match History for an Event
            var childEventIDs = _eventHelper.GetChildEvents(_heroesEntity, eventEntity, includeDisabledEvents: false)
                .Select(i => i.EventId).ToArray();

            selectedMatches = selectedMatches.Where(i => childEventIDs.Any(j => j == (int)i.GM)).ToArray();
        }
        else if (_playerId != -1 && selectedMatches.Length > 0)
        {
            // Player Match History
            selectedMatches = selectedMatches
                .OrderByDescending(i => i.TR).ToArray();
        }

        var replayFileLifetime = DateTime.UtcNow.AddDays(ReplayFileLifetimeInDays * -1);
        var heroRoleConcurrentDictionary = Global.GetHeroRoleConcurrentDictionary();

        var rc = selectedMatches.Select(
            i => new MatchHistoryRow
            {
                ReplayID = i.RID,
                Map = SiteMaster.GetLocalizedString("GenericMapName", i.M),
                MapURL = i.M,
                ReplayLength = i.RL,
                ReplayLengthMinutes = TimeSpan.FromMinutes(i.RL.Minutes),
                Character = SiteMaster.GetLocalizedString("GenericHero", i.C),
                CharacterURL = i.C,
                CharacterLevel = i.CL,
                Result = i.IsW ? 1 : 0,
                MMRBefore = i.MMRB,
                MMRChange = i.MMRC,
                ReplayShare = i.IsRS && i.TR > replayFileLifetime ? i.RID : null,
                TimestampReplay =
                    new DateTimeOffset(i.TR, TimeSpan.Zero)
                        .UtcDateTime, // TimeZoneInfo.ConvertTimeFromUtc(i.TR, userTimeZone),
                TimestampReplayDate = new DateTimeOffset(i.TR, TimeSpan.Zero).UtcDateTime.Date,
                Role = heroRoleConcurrentDictionary.ContainsKey(i.C) ? heroRoleConcurrentDictionary[i.C] : null,
                TimestampReplayTicks = i.TR.Ticks,
                Season = i.GM == GameMode.Unknown,
            }).ToArray();

        var flt = string.IsNullOrWhiteSpace(_args.Filter)
            ? rc
            : rc.Where(
                r =>
                    (r.Map?.ContainsIgnoreCase(_args.Filter) ?? true) ||
                    (r.MapURL?.ContainsIgnoreCase(_args.Filter) ?? true) ||
                    (r.Role?.ContainsIgnoreCase(_args.Filter) ?? true) ||
                    (r.Character?.ContainsIgnoreCase(_args.Filter) ?? true) ||
                    (r.CharacterURL?.ContainsIgnoreCase(_args.Filter) ?? true)).ToArray();

        var sorted = false;
        if (_args.Sort?.direction is "asc" or "desc")
        {
            var desc = _args.Sort.direction is "desc";
            var prop = typeof(MatchHistoryRow).GetProperty(_args.Sort.active);
            if (prop != null)
            {
                flt = desc
                    ? flt.OrderByDescending(r => prop.GetValue(r)).ToArray()
                    : flt.OrderBy(r => prop.GetValue(r)).ToArray();

                sorted = true;
            }
        }

        res.Total = flt.Length;

        var skipCount = _args.Page.pageSize * _args.Page.pageIndex;

        var statsFlt = flt.Skip(skipCount).Take(_args.Page.pageSize).ToArray();

        if (!sorted && statsFlt.Length > 0)
        {
            // Add in rows for applicable Player MMR Resets
            var seasonSeparators = GetSeasonSeparators(statsFlt);
            statsFlt = statsFlt.Union(seasonSeparators).OrderByDescending(i => i.TimestampReplay).ToArray();
        }

        res.Stats = statsFlt;

        return res;
    }

    private static MatchHistoryRow[] GetSeasonSeparators(MatchHistoryRow[] selectedMatches)
    {
        var timestampReplayMin = selectedMatches.Select(i => i.TimestampReplay).Min();
        var timestampReplayMax = selectedMatches.Select(i => i.TimestampReplay).Max();
        var playerMMRResets = Global.GetPlayerMMRResets()
            .Where(i => i.ResetDate >= timestampReplayMin && i.ResetDate <= timestampReplayMax).ToArray();

        var seasonSeparators = playerMMRResets.Select(
            i => new PlayerMatch
            {
                M = i.Title,
                C = " ",
                TR = i.ResetDate,
                GM = GameMode.Unknown,
            });

        var rc = seasonSeparators.Select(
            i => new MatchHistoryRow
            {
                Map = SiteMaster.GetLocalizedString("GenericMapName", i.M),
                MapURL = i.M,
                TimestampReplay =
                    new DateTimeOffset(i.TR, TimeSpan.Zero)
                        .UtcDateTime, // TimeZoneInfo.ConvertTimeFromUtc(i.TR, userTimeZone),
                TimestampReplayDate = new DateTimeOffset(i.TR, TimeSpan.Zero).UtcDateTime.Date,
                TimestampReplayTicks = i.TR.Ticks,
                Season = i.GM == GameMode.Unknown,
            }).ToArray();

        return rc;
    }
}
