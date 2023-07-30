using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.Models;
using HotsLogsApi.BL.Migration.Overview.Models;
using HotsLogsApi.BL.Migration.Profile.Models;
using HotsLogsApi.BL.Migration.TalentDetails.Models;
using HotsLogsApi.BL.Resources;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Player = Heroes.DataAccessLayer.Models.Player;

namespace HotsLogsApi.BL.Migration.Profile;

public class Helper : HelperBase<ProfileResponse>
{
    private readonly AppUser _appUser;
    private readonly ProfileArgs _args;
    private readonly HeroesdataContext _dc;
    private readonly ReplayCharacterHelper _rch;
    private readonly EventHelper _eh;
    private readonly ILogger<Helper> _logger;

    public Helper(ProfileArgs args, AppUser appUser, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
        _appUser = appUser;
        _dc = svcp.GetRequiredService<HeroesdataContext>();
        _rch = svcp.GetRequiredService<ReplayCharacterHelper>();
        _eh = svcp.GetRequiredService<EventHelper>();
        _logger = svcp.GetRequiredService<ILogger<Helper>>();
    }

    public override ProfileResponse MainCalculation()
    {
        var res = new ProfileResponse();

        var queryPlayerId = _args.PlayerId ?? -1; // specific player requested
        var authPlayerId = _appUser?.MainPlayerId ?? -1; // user is bnet registered
        var playerId = queryPlayerId != -1 ? queryPlayerId : authPlayerId; // player id requested
        var userOfPlayer = playerId.GetBnetUserOfPlayer(_dc);
        var playerEntity = _dc.Players
            .Include(x => x.LeaderboardRankings)
            .Include(x => x.LeaderboardOptOut)
            .Include(x => x.PlayerMmrMilestoneV3s)
            .SingleOrDefault(i => i.PlayerId == playerId);

        if (playerEntity is null)
        {
            _logger.LogError("Profile requested for nonexistent player (id {playerId})", playerId);
            return res;
        }

        var heOptedOut = playerEntity?.LeaderboardOptOut is not null;
        var weAreHim = _appUser is not null && _appUser?.Id == userOfPlayer?.Id;

        if (heOptedOut && !weAreHim)
        {
            res.Unauthorized = true;
            return res;
        }

        int? filterGameMode = GetSelectedGameMode();
        if (filterGameMode == 0)
        {
            filterGameMode = null;
        }

        var seasons1 = Flt.Season.OrderBy(x => x.ResetDate).ToList();
        var seasons2 = seasons1.Zip(seasons1.Skip(1).Concat(new[] { (PlayerMmrReset)null }), (a, b) => (a, b));
        var mmrSeasons = seasons2.ToDictionary(
            x => x.a.Title,
            y => new RankedSeason
            {
                Title = y.a.Title,
                DateTimeSeasonStart = y.a.ResetDate,
                DateTimeSeasonEnd = y.b?.ResetDate ?? DateTime.UtcNow,
            });


        var profileFilterDateTimeStart = DateTime.MinValue.ToUniversalTime();
        var profileFilterDateTimeEnd = DateTime.UtcNow;

        if (mmrSeasons.ContainsKey(_args.Time))
        {
            // User selected a Ranked Season for their profile date range
            var selectedRankedSeason = mmrSeasons[_args.Time];

            profileFilterDateTimeStart = selectedRankedSeason.DateTimeSeasonStart;
            profileFilterDateTimeEnd = selectedRankedSeason.DateTimeSeasonEnd;
        }
        else if (_args.Time != "1")
        {
            // User selected a normal option for their profile date range
            profileFilterDateTimeStart =
                DateTime.UtcNow.AddDays(int.Parse(_args.Time));
        }

        var isEventProfile = _args.EventId.HasValue;

        if (isEventProfile)
        {
            //divDropDowns.Visible = false;

            //liTabMMRMilestones.Visible = false;
            //liTabFriends.Visible = false;
            //liTabRivals.Visible = false;
            //liTabSharedReplays.Visible = false;
        }

        if (isEventProfile)
        {
            var eventEntity = _dc.Events
                .Include(x => x.EventIdparentNavigation)
                .Include(x => x.InverseEventIdparentNavigation)
                .SingleOrDefault(i => i.EventId == filterGameMode);

            if (eventEntity == null || eventEntity.IsEnabled == 0)
            {
                return res;
            }

            var literalHeaderLinksText = @"<h2>Event: " + HttpUtility.HtmlEncode(eventEntity.EventName) + @"</h2>";

            if (eventEntity.EventIdparentNavigation != null)
            {
                literalHeaderLinksText += string.Format(
                    @"<p>Parent Event: <a href=""/ang/Player/Profile?PlayerID={2}&EventID={0}"">{1}</a></p>",
                    eventEntity.EventIdparentNavigation.EventId,
                    HttpUtility.HtmlEncode(eventEntity.EventIdparentNavigation.EventName),
                    playerId);
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
                                    @"<span><a href=""/ang/Player/Profile?PlayerID={2}&EventID={0}"">{1}</a></span>",
                                    i.EventId,
                                    HttpUtility.HtmlEncode(i.EventName),
                                    playerId))) +
                    @"</p>";
            }

            literalHeaderLinksText +=
                $@"<p><a href=""/ang/Player/Profile?PlayerID={playerId}"">View Overall Profile</a></p>";

            res.HeaderLinks = literalHeaderLinksText;
        }

        using var scope = Svcp.CreateScope();
        var profileHelper = scope.ServiceProvider.GetRequiredService<ProfileHelper>();
        var profile = profileHelper.GetPlayerProfile(
            player: playerEntity,
            gameMode: (GameMode?)filterGameMode,
            dateTimeStart: profileFilterDateTimeStart,
            dateTimeEnd: profileFilterDateTimeEnd);

        res.GeneralInformation = GetGeneralInformation(profile, isEventProfile);

        res.Title = LocalizedText.GenericProfile;
        res.Title = (DataHelper.BattleNetRegionNames.ContainsKey(profile.BattleNetRegionId)
            ? DataHelper.BattleNetRegionNames[profile.BattleNetRegionId].Split(' ')[0] + " "
            : null) + res.Title + @": " + profile.PlayerName;

        res.RoleStats = profile.PlayerProfileCharacterRoleStatistics;

        switch (_args.Tab)
        {
            case 0 when _args.HeroDetails is not null:
            {
                var rc = GetHeroDetails(profile, weAreHim, _args.HeroDetails);
                res.HeroDetails = rc;
                break;
            }
            case 0:
            {
                var rc = GetCharacterStats(profile, _args.Time != "1");
                res.CharacterStats = rc;
                break;
            }
            case 1 when _args.MapDetails is not null:
            {
                var rc = GetMapDetails(profile, _args.MapDetails);
                res.MapDetails = rc;
                break;
            }
            case 1:
            {
                var rc = GetMapStats(profile);
                res.MapStats = rc;
                break;
            }
            case 2:
            {
                var rc = GetWinRateVsStats(profile);
                res.WinRateVsStats = rc;
                break;
            }
            case 3:
            {
                var rc = GetWinRateWithStats(profile);
                res.WinRateWithStats = rc;
                break;
            }
            case 4:
            {
                var gm = (GameMode)int.Parse(_args.GameModeForMmr);
                var rc = GetMilestonesChart(profile, gm);
                res.MilestoneChart = rc;
                break;
            }
            case 5:
            {
                var rc = GetWinRateChart(profile);
                res.WinRateChart = rc;
                break;
            }
            case 6:
            {
                var rc = GetFriendsStats(profile);
                res.FriendsStats = rc;
                break;
            }
            case 7:
            {
                var rc = GetRivalsStats(profile);
                res.RivalsStats = rc;
                break;
            }
            case 8 when _args.ReplayDetails is not null:
            {
                var rc = GetReplayDetails(_args.ReplayDetails.Value);
                res.ReplayDetails = rc;
                break;
            }
            case 8:
            {
                var rc = GetReplaySearchStats(profile);
                res.ReplaySearchStats = rc;
                break;
            }
        }

        return res;
    }

    private ProfileCharacterStatisticsRow[] GetCharacterStats(
        PlayerProfile profile,
        bool filterdByTime)
    {
        var rc = Array.Empty<ProfileCharacterStatisticsRow>();

        if (!profile.PlayerProfileCharacterStatistics.Any(
                i => profile.FilterGameMode.HasValue || filterdByTime || i.GamesPlayed >= 5))
        {
            return rc;
        }

        var gamesPlayedMin = profile.PlayerProfileCharacterStatistics.Min(i => i.GamesPlayed);
        var gamesPlayedMax = profile.PlayerProfileCharacterStatistics.Max(i => i.GamesPlayed);

        var winPercentMin = profile.PlayerProfileCharacterStatistics.Where(
            i => profile.FilterGameMode.HasValue || filterdByTime ||
                 i.GamesPlayed >= 5).Min(i => i.WinPercent);
        var winPercentMax = profile.PlayerProfileCharacterStatistics.Where(
            i => profile.FilterGameMode.HasValue || filterdByTime ||
                 i.GamesPlayed >= 5).Max(i => i.WinPercent);

        rc = profile.PlayerProfileCharacterStatistics.Select(
                i => new ProfileCharacterStatisticsRow
                {
                    HeroPortraitURL = i.HeroPortraitURL,
                    PrimaryName = i.Character,
                    Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                    CharacterURL = i.Character,
                    CharacterLevel = i.CharacterLevel,
                    GamesPlayed = SiteMaster.GetGaugeHtml(
                        i.GamesPlayed,
                        gamesPlayedMin,
                        gamesPlayedMax,
                        TeamCompHelper.HeroRoleColorsDictionary["Support"],
                        "N0"),
                    GamesPlayedValue = i.GamesPlayed,
                    AverageLength = i.AverageLength,
                    WinPercent = profile.FilterGameMode.HasValue || filterdByTime ||
                                 i.GamesPlayed >= 5
                        ? SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax)
                        : null,
                    WinPercentValue = profile.FilterGameMode.HasValue ||
                                      filterdByTime ||
                                      i.GamesPlayed >= 5
                        ? i.WinPercent
                        : null,
                })
            .OrderByDescending(r => r.GamesPlayedValue)
            .ToArray();

        return rc;
    }

    private ProfileFriendsRow[] GetFriendsStats(PlayerProfile profile)
    {
        var rc = Array.Empty<ProfileFriendsRow>();

        if (profile.PlayerProfileFriends.Length <= 0)
        {
            return rc;
        }

        var gamesPlayedMin = profile.PlayerProfileFriends.Min(i => i.GamesPlayedWith);
        var gamesPlayedMax = profile.PlayerProfileFriends.Max(i => i.GamesPlayedWith);

        var winPercentMin = profile.PlayerProfileFriends.Min(i => i.WinPercent);
        var winPercentMax = profile.PlayerProfileFriends.Max(i => i.WinPercent);

        rc = profile.PlayerProfileFriends
            .OrderByDescending(i => i.GamesPlayedWith).Select(
                i => new ProfileFriendsRow
                {
                    HeroPortraitURL = i.HeroPortraitURL,
                    PlayerID = i.PlayerID,
                    PlayerName = i.PlayerName,
                    FavoriteHero = SiteMaster.GetLocalizedString("GenericHero", i.FavoriteHero),
                    FavoriteHeroURL = i.FavoriteHero,
                    GamesPlayedWith = SiteMaster.GetGaugeHtml(
                        i.GamesPlayedWith,
                        gamesPlayedMin,
                        gamesPlayedMax,
                        TeamCompHelper.HeroRoleColorsDictionary["Support"],
                        "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                    CurrentMMR = i.CurrentMMR,
                }).ToArray();

        return rc;
    }

    private string[][] GetGeneralInformation(PlayerProfile profile, bool isEvent)
    {
        // Set General Information
        var generalInformationList = new List<(string, string)>();

        if (!isEvent)
        {
            var index = 0;
            var gameModeOrder =
                DataHelper.GameModeWithStatistics.ToDictionary(i => i, i => index++);
            var leaderboardRankings = profile
                .LeaderboardRankings
                .Where(i => i.LeagueID.HasValue)
                .OrderBy(i => gameModeOrder.ContainsKey(i.GameMode) ? gameModeOrder[i.GameMode] : -1);
            foreach (var leaderboardRanking in leaderboardRankings)
            {
                var gameMode = ((GameMode)leaderboardRanking.GameMode).GetGameMode();
                var leagueHtml = HttpUtility.HtmlEncode(
                    SiteMaster.GetLocalizedString("GenericLeague", $"{leaderboardRanking.LeagueID.Value}"));
                var leagueRank = leaderboardRanking.LeagueRank.HasValue
                    ? leaderboardRanking.LeagueRank.Value.ToString()
                    : @"<div style=""display: inline; font-weight: bold;"" title=""" + HttpUtility.HtmlEncode(
                        string.Format(
                            LocalizedText.LeaderboardLeagueGamesPlayedRequirement,
                            leaderboardRanking.LeagueRequiredGames) +
                        ", and at least 5 games within the past 30 days") + @""">*</div>";
                var currentMmr = leaderboardRanking.CurrentMMR.HasValue
                    ? @" (MMR:&nbsp;" + leaderboardRanking.CurrentMMR.Value + ")"
                    : null;
                var html =
                    $@"<img class=""divLeagueImage"" src=""/assets/Images/Leagues/{leaderboardRanking.LeagueID.Value}.png"">&nbsp;<span>{leagueHtml} {leagueRank}{currentMmr}</span>";
                generalInformationList.Add((gameMode, html));
            }

            var mvpPercent = MatchAwardType.MVP.GetMatchAwardTypeHtmlIcon() + "&nbsp;" +
                             profile.OverallMVPPercent.ToString("p1");
            generalInformationList.Add(("Overall MVP Percent", mvpPercent));

            generalInformationList.Add(
                (
                    "Overall Win Percent",
                    profile.OverallWinPercent.ToString("p1")));

            generalInformationList.Add(
                (
                    "Combined Hero Level",
                    profile.PlayerProfileCharacterStatistics.Sum(i => i.CharacterLevel).ToString()));
        }

        generalInformationList.Add(
            (
                LocalizedText.ProfileTotalGamesPlayed,
                profile.TotalGamesPlayed.ToString()));
        generalInformationList.Add(
            (
                LocalizedText.ProfileTotalTimePlayed,
                SiteMaster.GetUserFriendlyTimeSpanString(profile.TotalTimePlayed)));
        generalInformationList.Add(("Reputation", profile.Reputation.ToString()));
        return generalInformationList.Select(r => new[] { r.Item1, r.Item2 }).ToArray();
    }

    private TalentDetailsRow<string>[] GetHeroDetails(
        PlayerProfile profile,
        bool weAreHim,
        string hero)
    {
        var rc = Array.Empty<TalentDetailsRow<string>>();

        var localizationAliasesIdentifierIdDictionary =
            Global.GetLocalizationAliasesIdentifierIDDictionary();

        if (!localizationAliasesIdentifierIdDictionary.ContainsKey(hero))
        {
            // This is a new Hero that we haven't identified in our database properly
            return rc;
        }

        var gameModes =
            profile.FilterGameMode.HasValue &&
            (profile.FilterGameMode.Value != (int)GameMode.Custom || weAreHim)
                ? new[] { profile.FilterGameMode.Value }
                : DataHelper.GameModeWithStatistics;

        if (profile.FilterGameMode is > 1000)
        {
            var eventId = profile.FilterGameMode.Value;
            var eventEntity = _dc.Events
                .Include(x => x.EventIdparentNavigation)
                .Single(i => i.EventId == eventId);
            gameModes = _eh.GetChildEvents(_dc, eventEntity, includeDisabledEvents: false)
                .Select(i => i.EventId).ToArray();
        }

        var playerId = profile.PlayerID;
        var selectedPlayerIDs = PlayerExtensions.GetPlayerIdAlts(_dc, playerId);
        selectedPlayerIDs.Add(playerId);

        var detailTableViewDataSource = DataHelper.GetSitewideHeroTalentStatistics(
            profile.FilterDateTimeStart,
            profile.FilterDateTimeEnd,
            null,
            localizationAliasesIdentifierIdDictionary[hero],
            null,
            null,
            gameModes,
            selectedPlayerIDs.ToArray(),
            true).Select(
            i => new
            {
                i.TalentTier,
                TalentImageURL = Global.HeroTalentImages[i.Character, i.TalentName],
                i.TalentName,
                i.TalentDescription,
                i.GamesPlayed,
                i.WinPercent,
            }).Distinct().ToArray();

        if (detailTableViewDataSource.Length > 0)
        {
            var gamesPlayedMin = detailTableViewDataSource.Min(i => i.GamesPlayed);
            var gamesPlayedMax = detailTableViewDataSource.Max(i => i.GamesPlayed);

            var winPercentMin = detailTableViewDataSource.Min(i => i.WinPercent);
            var winPercentMax = detailTableViewDataSource.Max(i => i.WinPercent);

            rc = detailTableViewDataSource.Select(
                i => new TalentDetailsRow<string>
                {
                    TalentTier = i.TalentTier,
                    TalentImageURL = i.TalentImageURL,
                    TalentName = i.TalentName,
                    TalentDescription = i.TalentDescription,
                    GamesPlayed = SiteMaster.GetGaugeHtml(
                        i.GamesPlayed,
                        gamesPlayedMin,
                        gamesPlayedMax,
                        TeamCompHelper.HeroRoleColorsDictionary["Support"],
                        "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                }).ToArray();

            var headers = rc.Skip(1)
                .Zip(rc, (a, b) => (Talent: a, TierBoundary: a.TalentTier != b.TalentTier))
                .Where(x => x.TierBoundary).ToList();
            var chromieMod = hero == "Chromie" ? 2 : 0;
            headers.ForEach(
                x => x.Talent.HeaderStart =
                    $"Level: {Math.Max(1, x.Talent.TalentTier.GetValueOrDefault() - chromieMod)}");
            rc[0].HeaderStart = "Level: 1";
        }

        return rc;
    }

    private MapStatisticsDetailsRow[] GetMapDetails(PlayerProfile profile, string mapName)
    {
        var rc = Array.Empty<MapStatisticsDetailsRow>();

        var detailTableViewDataSource = profile
            .PlayerProfileMapStatistics
            .Single(i => i.Map == mapName)
            .MapDetailStatistics
            .OrderByDescending(i => i.WinPercent)
            .ToArray();

        if (detailTableViewDataSource.Length > 0)
        {
            var heroRoleConcurrentDictionary =
                Global.GetHeroRoleConcurrentDictionary();
            var heroAliasCsvConcurrentDictionary =
                Global.GetHeroAliasCSVConcurrentDictionary();

            var gamesPlayedMin = detailTableViewDataSource.Min(i => i.GamesPlayed);
            var gamesPlayedMax = detailTableViewDataSource.Max(i => i.GamesPlayed);

            var winPercentMin = detailTableViewDataSource.Min(i => i.WinPercent);
            var winPercentMax = detailTableViewDataSource.Max(i => i.WinPercent);

            rc = detailTableViewDataSource.Select(
                i => new MapStatisticsDetailsRow
                {
                    Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                    CharacterURL = i.Character,
                    GamesPlayed = SiteMaster.GetGaugeHtml(
                        i.GamesPlayed,
                        gamesPlayedMin,
                        gamesPlayedMax,
                        TeamCompHelper.HeroRoleColorsDictionary["Support"],
                        "N0"),
                    AverageLength = i.AverageLength,
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                    Role = heroRoleConcurrentDictionary.ContainsKey(i.Character)
                        ? heroRoleConcurrentDictionary[i.Character]
                        : null,
                    AliasCSV = heroAliasCsvConcurrentDictionary.ContainsKey(i.Character)
                        ? heroAliasCsvConcurrentDictionary[i.Character]
                        : null,
                }).ToArray();
        }

        return rc;
    }

    private MapStatisticsRow[] GetMapStats(PlayerProfile profile)
    {
        var rc = Array.Empty<MapStatisticsRow>();

        if (profile.PlayerProfileMapStatistics.Length <= 0)
        {
            return rc;
        }

        var gamesPlayedMin = profile.PlayerProfileMapStatistics.Min(i => i.GamesPlayed);
        var gamesPlayedMax = profile.PlayerProfileMapStatistics.Max(i => i.GamesPlayed);

        var winPercentMin = profile.PlayerProfileMapStatistics.Min(i => i.WinPercent);
        var winPercentMax = profile.PlayerProfileMapStatistics.Max(i => i.WinPercent);

        rc = profile.PlayerProfileMapStatistics.Select(
                i => new MapStatisticsRow
                {
                    MapImageURL = i.Map.PrepareForImageURL(),
                    Map = i.Map,
                    MapNameLocalized = SiteMaster.GetLocalizedString("GenericMapName", i.Map),
                    GamesPlayed = SiteMaster.GetGaugeHtml(
                        i.GamesPlayed,
                        gamesPlayedMin,
                        gamesPlayedMax,
                        TeamCompHelper.HeroRoleColorsDictionary["Support"],
                        "N0"),
                    GamesPlayedValue = i.GamesPlayed,
                    AverageLength = i.AverageLength,
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                    WinPercentValue = i.WinPercent,
                })
            .OrderByDescending(r => r.GamesPlayedValue)
            .ToArray();

        return rc;
    }

    private string GetMilestonesChart(PlayerProfile profile, GameMode gameMode, int takeAmount = 100)
    {
        var milestones = profile.PlayerProfileMMRMilestonesV3
            .Where(i => i.GameMode == (int)gameMode)
            .ToArray();
        if (milestones.Length >= takeAmount)
        {
            // Only display a max of 100 points on the graph
            var multipleData = (int)((decimal)milestones.Length / takeAmount);
            var milestoneIndex = 0;
            for (var i = 0; i < milestones.Length; i++)
            {
                if (i % multipleData == 0)
                {
                    milestones[milestoneIndex++] = milestones[i];
                }
            }

            milestones[milestoneIndex - 1] =
                milestones[milestones.Length - 1];
            milestones = milestones.Take(milestoneIndex).ToArray();
        }

        // Set up Player MMR Reset markers
        var mmrDic = new Dictionary<DateTime, PlayerMmrReset>();
        var playerMMRResets = Global.GetPlayerMMRResets().Where(
            i => milestones.Length != 0 &&
                 i.ResetDate >= milestones[0].MilestoneDate).ToArray();
        var playerMMRResetsIndex = 0;

        for (var i = 0; i < milestones.Length && playerMMRResetsIndex < playerMMRResets.Length; i++)
        {
            if (milestones[i].MilestoneDate >=
                playerMMRResets[playerMMRResetsIndex].ResetDate)
            {
                mmrDic[milestones[i].MilestoneDate] = playerMMRResets[playerMMRResetsIndex];

                // RadHtmlChartMMRMilestones.PlotArea.XAxis.PlotBands.Add(new PlotBand(i, i, Color.Blue, 240));
                playerMMRResetsIndex++;
            }
        }

        var radHtmlChartMMRMilestonesDataSource = milestones.Select(
            i => new
            {
                i.MilestoneDate,
                i.MMRRating,
                TooltipText = @"<table>" +
                              (mmrDic.ContainsKey(i.MilestoneDate)
                                  ? @"<tr><td style='text-align: right;'><strong>MMR Change: </strong></td><td>" +
                                    mmrDic[i.MilestoneDate]
                                        .Title + @"</td></tr>"
                                  : null) + @"<tr><td style='text-align: right;'><strong>" +
                              LocalizedText.GenericDateTime + @": </strong></td><td>" +
                              i.MilestoneDate.ToString("d") +
                              @"</td></tr><tr><td style='text-align: right;'><strong>" +
                              LocalizedText.ProfileCurrentMMR + @": </strong></td><td>" + i.MMRRating +
                              @"</td></tr></table>",
            }).ToArray();

        var rc1 = radHtmlChartMMRMilestonesDataSource.Select(
            r => new RadChartDataRow<DateTime, int>
            {
                X = r.MilestoneDate,
                WinPercent = r.MMRRating,
            });

        var rc = new RadChartDef<DateTime, int>
        {
            Type = RadChartType.Date,
            Data = rc1,
            MinY = 0,
            MaxY = 3000,
            YType = RadChartYType.Number,
            YTitle = "Current MMR",
        };
        var json1 = rc.ToJson();
        return json1;
    }

    private ProfileSharedReplayDetailRow[] GetReplayDetails(int replayId)
    {
        var rc = _rch.GetReplayCharacterDetails(replayId).Select(
            i => new ProfileSharedReplayDetailRow
            {
                ReplayID = i.ReplayID,
                PlayerID = !i.IsLeaderboardOptOut ? i.PlayerID : null,
                PlayerName = i.PlayerName,
                Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                CharacterURL = i.Character,
                CharacterLevel = i.CharacterLevel,
                TalentImageURL01 = i.TalentImageURL01,
                TalentImageURL04 = i.TalentImageURL04,
                TalentImageURL07 = i.TalentImageURL07,
                TalentImageURL10 = i.TalentImageURL10,
                TalentImageURL13 = i.TalentImageURL13,
                TalentImageURL16 = i.TalentImageURL16,
                TalentImageURL20 = i.TalentImageURL20,
                TalentNameDescription01 = i.TalentNameDescription01,
                TalentNameDescription04 = i.TalentNameDescription04,
                TalentNameDescription07 = i.TalentNameDescription07,
                TalentNameDescription10 = i.TalentNameDescription10,
                TalentNameDescription13 = i.TalentNameDescription13,
                TalentNameDescription16 = i.TalentNameDescription16,
                TalentNameDescription20 = i.TalentNameDescription20,
                TalentName01 = i.TalentName01,
                TalentName04 = i.TalentName04,
                TalentName07 = i.TalentName07,
                TalentName10 = i.TalentName10,
                TalentName13 = i.TalentName13,
                TalentName16 = i.TalentName16,
                TalentName20 = i.TalentName20,
                Team = i.IsWinner,
                MMRBefore = i.MMRBefore,
                MMRChange = i.MMRChange,
            }).ToArray();

        return rc;
    }

    private ProfileSharedReplayRow[] GetReplaySearchStats(PlayerProfile profile)
    {
        var rc = profile.PlayerProfileSharedReplays?.Select(
            i => new ProfileSharedReplayRow
            {
                ReplayShareID = i.ReplayShareID,
                ReplayID = i.ReplayID,
                UpvoteScore = i.UpvoteScore,
                GameMode = ((GameMode)i.GameMode).GetGameMode(),
                Title = i.Title,
                Map = i.Map,
                ReplayLength = i.ReplayLength,
                ReplayLengthMinutes = i.ReplayLengthMinutes,
                Characters = i.Characters,
                AverageCharacterLevel = i.AverageCharacterLevel,
                AverageMMR = i.AverageMMR,
                TimestampReplay = i.TimestampReplay,
                TimestampReplayDate = i.TimestampReplay.Date,
            }).ToArray();

        return rc;
    }

    private ProfileFriendsRow[] GetRivalsStats(PlayerProfile profile)
    {
        var rc = Array.Empty<ProfileFriendsRow>();
        if (profile.PlayerProfileRivals.Length <= 0)
        {
            return rc;
        }

        var gamesPlayedMin = profile.PlayerProfileRivals.Min(i => i.GamesPlayedWith);
        var gamesPlayedMax = profile.PlayerProfileRivals.Max(i => i.GamesPlayedWith);

        var winPercentMin = profile.PlayerProfileRivals.Min(i => i.WinPercent);
        var winPercentMax = profile.PlayerProfileRivals.Max(i => i.WinPercent);

        rc = profile.PlayerProfileRivals
            .OrderByDescending(i => i.GamesPlayedWith).Select(
                i => new ProfileFriendsRow
                {
                    HeroPortraitURL = i.HeroPortraitURL,
                    PlayerID = i.PlayerID,
                    PlayerName = i.PlayerName,
                    FavoriteHero = SiteMaster.GetLocalizedString("GenericHero", i.FavoriteHero),
                    FavoriteHeroURL = i.FavoriteHero,
                    GamesPlayedWith = SiteMaster.GetGaugeHtml(
                        i.GamesPlayedWith,
                        gamesPlayedMin,
                        gamesPlayedMax,
                        TeamCompHelper.HeroRoleColorsDictionary["Support"],
                        "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                    CurrentMMR = i.CurrentMMR,
                }).ToArray();

        return rc;
    }

    private string GetWinRateChart(PlayerProfile profile)
    {
        var radHtmlChartWinRateByGameTimeDataSource = profile.PlayerProfileGameTimeWinRate.Select(
            i => new
            {
                i.GameTimeMinuteBegin,
                i.GamesPlayed,
                i.WinPercent,
                TooltipText = @"<table><tr><td style='text-align: right;'><strong>" +
                              LocalizedText.GenericWinPercent + @": </strong></td><td>" +
                              i.WinPercent.ToString("p1") + @"</td></tr><tr><td><strong>" +
                              LocalizedText.GenericGamesPlayed + @": </strong></td><td>" + i.GamesPlayed +
                              @"</td></tr></table>",
            });

        var rc1X =
            radHtmlChartWinRateByGameTimeDataSource.Select(
                i => new RadChartDataRow<int, decimal>
                {
                    X = i.GameTimeMinuteBegin,
                    GamesPlayed = i.GamesPlayed,
                    WinPercent = i.WinPercent,
                }).ToList();

        var rcx = new RadChartDef<int, decimal>
        {
            Name = "Game Length in Minutes",
            Type = RadChartType.Number,
            Data = rc1X,
            MinY = 0.35m,
            MaxY = 0.7m,
        };
        var json1X = rcx.ToJson();
        return json1X;
    }

    private WinRateWithOrVsRow[] GetWinRateVsStats(PlayerProfile profile)
    {
        var rc = Array.Empty<WinRateWithOrVsRow>();
        var
            heroRoleConcurrentDictionary = Global.GetHeroRoleConcurrentDictionary();
        var heroAliasCsvConcurrentDictionary =
            Global.GetHeroAliasCSVConcurrentDictionary();

        var stats = profile.PlayerProfileCharacterWinPercentVsOtherCharacters;

        if (stats.Length <= 0)
        {
            return rc;
        }

        var gamesPlayedMin =
            stats.Min(i => i.GamesPlayed);
        var gamesPlayedMax =
            stats.Max(i => i.GamesPlayed);

        var winPercentMin =
            stats.Min(i => i.WinPercent);
        var winPercentMax =
            stats.Max(i => i.WinPercent);

        rc = stats.OrderByDescending(i => i.WinPercent).Select(
                i => new WinRateWithOrVsRow
                {
                    HeroPortraitURL = i.HeroPortraitURL,
                    Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                    CharacterURL = i.Character,
                    GamesPlayed = SiteMaster.GetGaugeHtml(
                        i.GamesPlayed,
                        gamesPlayedMin,
                        gamesPlayedMax,
                        TeamCompHelper.HeroRoleColorsDictionary["Support"],
                        "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                    Role = heroRoleConcurrentDictionary.ContainsKey(i.Character)
                        ? heroRoleConcurrentDictionary[i.Character]
                        : null,
                    AliasCSV = heroAliasCsvConcurrentDictionary.ContainsKey(i.Character)
                        ? heroAliasCsvConcurrentDictionary[i.Character]
                        : null,
                })
            .OrderByDescending(r => r.GamesPlayed)
            .ToArray();

        return rc;
    }

    private WinRateWithOrVsRow[] GetWinRateWithStats(PlayerProfile profile)
    {
        var rc = Array.Empty<WinRateWithOrVsRow>();
        var
            heroRoleConcurrentDictionary = Global.GetHeroRoleConcurrentDictionary();
        var heroAliasCsvConcurrentDictionary =
            Global.GetHeroAliasCSVConcurrentDictionary();

        if (profile.PlayerProfileCharacterWinPercentWithOtherCharacters.Length <= 0)
        {
            return rc;
        }

        var gamesPlayedMin =
            profile.PlayerProfileCharacterWinPercentWithOtherCharacters.Min(i => i.GamesPlayed);
        var gamesPlayedMax =
            profile.PlayerProfileCharacterWinPercentWithOtherCharacters.Max(i => i.GamesPlayed);

        var winPercentMin =
            profile.PlayerProfileCharacterWinPercentWithOtherCharacters.Min(i => i.WinPercent);
        var winPercentMax =
            profile.PlayerProfileCharacterWinPercentWithOtherCharacters.Max(i => i.WinPercent);

        rc = profile
            .PlayerProfileCharacterWinPercentWithOtherCharacters.OrderByDescending(i => i.WinPercent).Select(
                i => new WinRateWithOrVsRow
                {
                    HeroPortraitURL = i.HeroPortraitURL,
                    Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                    CharacterURL = i.Character,
                    GamesPlayed = SiteMaster.GetGaugeHtml(
                        i.GamesPlayed,
                        gamesPlayedMin,
                        gamesPlayedMax,
                        TeamCompHelper.HeroRoleColorsDictionary["Support"],
                        "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                    Role = heroRoleConcurrentDictionary.ContainsKey(i.Character)
                        ? heroRoleConcurrentDictionary[i.Character]
                        : null,
                    AliasCSV = heroAliasCsvConcurrentDictionary.ContainsKey(i.Character)
                        ? heroAliasCsvConcurrentDictionary[i.Character]
                        : null,
                })
            .OrderByDescending(r => r.GamesPlayed)
            .ToArray();

        return rc;
    }
}
