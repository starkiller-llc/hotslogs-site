using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.Overview.Models;
using HotsLogsApi.BL.Migration.ScoreResults;
using HotsLogsApi.BL.Migration.ScoreResults.Models;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotsLogsApi.BL.Migration.Overview;

public class Helper : HelperBase<OverviewResponse>, IDisposable
{
    private const string PlayerProfilePlayerRelationshipQueryTypeFriends = "=";
    private readonly AppUser _appUser;

    private readonly OverviewArgs _args;
    private readonly HeroesdataContext _dc;
    private readonly IServiceScope _scope;
    private readonly EventHelper _eventHelper;

    public Helper(OverviewArgs args, AppUser appUser, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
        _appUser = appUser;
        _scope = Svcp.CreateScope();
        _dc = HeroesdataContext.Create(_scope);
        _eventHelper = _scope.ServiceProvider.GetRequiredService<EventHelper>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _dc.Dispose();
    }

    public override OverviewResponse MainCalculation()
    {
        var res = new OverviewResponse();

        var queryPlayerId = _args.PlayerId ?? -1;
        var authPlayerId = _appUser?.MainPlayerId ?? -1;
        //var mainUser = authPlayerId.GetBnetUserOfPlayer(_dc);
        //var isUserVerified = (mainUser?.Id ?? -2) == _appUser?.Id;
        var playerId = queryPlayerId != -1 ? queryPlayerId : authPlayerId;
        var playerEntity = _dc.Players
            .Include(x => x.LeaderboardOptOut)
            .SingleOrDefault(i => i.PlayerId == playerId);
        var heOptedOut = playerEntity?.LeaderboardOptOut is not null;

        var isPlayerHeroOverview = false;
        var filterGameMode = GetSelectedGameMode();
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

        var gamesPlayedRequirement = int.Parse(_args.GamesTogether);

        var playerProfileFriends =
            GetPlayerProfilePlayerRelationship(
                    PlayerProfilePlayerRelationshipQueryTypeFriends,
                    new[] { playerId },
                    filterGameMode != 0 ? (GameMode?)filterGameMode : null,
                    profileFilterDateTimeStart,
                    profileFilterDateTimeEnd,
                    gamesPlayedRequirement)
                .OrderByDescending(i => i.GamesPlayedWith).ToArray();

        var playerKeys = playerProfileFriends
            .Select(x => x.PlayerID)
            .Concat(new[] { playerId })
            .ToArray();

        TeamProfile teamProfile;

        var isEvent = Global.IsEventGameMode(filterGameMode);

        if (isEvent)
        {
            // Event Overview
            var eventId = filterGameMode;

            var eventEntity = _dc.Events
                .Include(x => x.EventIdparentNavigation)
                .Include(x => x.InverseEventIdparentNavigation)
                .SingleOrDefault(i => i.EventId == eventId);

            if (eventEntity == null || eventEntity.IsEnabled == 0)
            {
                return res;
            }

            teamProfile = _eventHelper.GetEventProfile(eventEntity, forceCacheReset: true);

            res.Title = $@"Event Overview: {teamProfile.Name}";
            //LiteralHeaderLinks.Text = string.Empty;

            //if (eventEntity.EventIdparentNavigation != null)
            //{
            //    LiteralHeaderLinks.Text +=
            //        $@"<p>Parent Event: <a href=""/Event/Overview?EventID={eventEntity.EventIdparentNavigation.EventId}"">{Server.HtmlEncode(eventEntity.EventIdparentNavigation.EventName)}</a></p>";
            //}

            if (eventEntity.InverseEventIdparentNavigation.Any(i => i.IsEnabled != 0))
            {
                //var msg = string.Join(
                //    @", ",
                //    eventEntity.InverseEventIdparentNavigation
                //        .Where(i => i.IsEnabled != 0)
                //        .OrderBy(i => i.EventName)
                //        .Select(
                //            i =>
                //                $@"<span><a href=""/Event/Overview?EventID={i.EventId}"">{HttpUtility.HtmlEncode(i.EventName)}</a></span>"));
                //LiteralHeaderLinks.Text += $@"<p>Summarizes data from the following Events: {msg}</p>";
            }

            //foreach (MyGridView radGridWithProfileLink in new[]
            //         {
            //                     RadGridReplayCharacterScoreResultsAverage,
            //                     RadGridRoleTabTank,
            //                     RadGridRoleTabBruiser,
            //                     RadGridRoleTabHealer,
            //                     RadGridRoleTabSupport,
            //                     RadGridRoleTabMeleeAssassin,
            //                     RadGridRoleTabRangedAssassin,
            //                     RadGridTalentUpgradesNova,
            //                     RadGridTalentUpgradesGall,
            //                 })
            //{
            //    ((MyHyperLinkField)radGridWithProfileLink.Columns.FindByDataField("PlayerID"))
            //        .DataNavigateUrlFormatString +=
            //        $@"&EventID={eventId}";
            //}

            //divDropDownContainer.Visible = false;
        }
        else
        {
            if (_args.TeamId.HasValue)
            {
                var teamId = _args.TeamId.Value;
                //string submitTeam = null; //Request.Query[SubmitTeam.UniqueID];
                var redisClient = MyDbWrapper.Create(_scope);
                var teamSave =
                    redisClient.Get<TeamTemporarySave>($"HOTSLogs:TeamOverview:{teamId}");
                if (teamSave == null)
                {
                    //Response.Redirect("Overview.aspx", true);
                    return res;
                }

                playerKeys = teamSave.players.ToArray();
                var dbPlayers = _dc.Players
                    .Where(x => playerKeys.Contains(x.PlayerId))
                    .ToDictionary(x => x.PlayerId);
                //string host = Request.Url.Host;
                //if (Request.Url.Port != 80)
                //{
                //    host += $":{Request.Url.Port}";
                //}

                var playerDisplay = playerKeys.Select(
                    x => dbPlayers[x].BattleTag.HasValue
                        ? $"{dbPlayers[x].Name}#{dbPlayers[x].BattleTag}"
                        : $"/ang/Player/Profile?PlayerID={x}");

                //if (submitTeam == null)
                {
                    res.TeamMembers = string.Join("\n", playerDisplay);
                }

                teamProfile = _eventHelper.GetTeamProfile(
                    playerKeys: playerKeys,
                    gameModes: new[] { filterGameMode },
                    dateTimeStart: profileFilterDateTimeStart,
                    dateTimeEnd: profileFilterDateTimeEnd,
                    gamesPlayedRequired: gamesPlayedRequirement,
                    teamGamePartySize: int.Parse(_args.PartySize),
                    forceCacheReset: true);

                res.Title = @"Team Overview: custom team";

                //foreach (ListItem listItem in GamesPlayedFilter.Items)
                //{
                //    listItem.Text += @" Together";
                //}
            }
            else
            {
                if (heOptedOut)
                {
                    return res;
                }

                if (queryPlayerId == -1 || _args.TeamOverview)
                {
                    teamProfile = _eventHelper.GetTeamProfile(
                        playerKeys: playerKeys,
                        gameModes: new[] { filterGameMode },
                        dateTimeStart: profileFilterDateTimeStart,
                        dateTimeEnd: profileFilterDateTimeEnd,
                        requiredPlayerId: playerId,
                        gamesPlayedRequired: gamesPlayedRequirement,
                        teamGamePartySize: int.Parse(_args.PartySize),
                        forceCacheReset: true);

                    res.Title = $@"Team Overview: {playerEntity?.Name}";
                }
                else
                {
                    // Player Hero Overview
                    isPlayerHeroOverview = true;

                    if (playerEntity is null)
                    {
                        return res;
                    }

                    teamProfile = _eventHelper.GetTeamProfile(
                        playerKeys: new[] { playerId },
                        gameModes: new[] { filterGameMode },
                        dateTimeStart: profileFilterDateTimeStart,
                        dateTimeEnd: profileFilterDateTimeEnd,
                        gamesPlayedRequired: int.Parse(_args.GamesTogether),
                        forceCacheReset: true);

                    res.Title = $@"Hero Overview: {playerEntity.Name}";

                    var locDic = Global.GetLocalizationAliasesIdentifierIDDictionary();

                    teamProfile.Players = Global.GetLocalizationAlias()
                        .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero)
                        .ToDictionary(
                            i => i.IdentifierId,
                            i => new TeamProfilePlayer
                            {
                                PID = i.IdentifierId,
                                PN = SiteMaster.GetLocalizedString("GenericHero", i.PrimaryName),
                            });

                    foreach (var replay in teamProfile.Replays)
                    {
                        foreach (var replayCharacter in replay.RCs)
                        {
                            replayCharacter.PID = locDic[replayCharacter.C];
                        }
                    }

                    //foreach (MyGridView radGridWithProfileLink in new[]
                    //         {
                    //                     RadGridReplayCharacterScoreResultsAverage,
                    //                     RadGridRoleTabTank,
                    //                     RadGridRoleTabBruiser,
                    //                     RadGridRoleTabHealer,
                    //                     RadGridRoleTabSupport,
                    //                     RadGridRoleTabMeleeAssassin,
                    //                     RadGridRoleTabRangedAssassin,
                    //                 })
                    //{
                    //    var gridHyperLinkColumn =
                    //        radGridWithProfileLink.Columns.FindByDataField("PlayerID") as
                    //            MyHyperLinkField;
                    //    if (!(gridHyperLinkColumn is null))
                    //    {
                    //        gridHyperLinkColumn.DataNavigateUrlFormatString =
                    //            @"/ang/Sitewide/TalentDetails?Hero={0}";
                    //        gridHyperLinkColumn.DataNavigateUrlFields = new[] { "PlayerName" };
                    //        gridHyperLinkColumn.HeaderText = LocalizedText.GenericHero;
                    //    }
                    //}

                    //RadGridTalentUpgradesNova.Columns.FindByDataField("PlayerID").Visible = false;
                    //RadGridTalentUpgradesGall.Columns.FindByDataField("PlayerID").Visible = false;

                    //PartySizeFilter.Visible = false;

                    //liTabHeroAndMapSummary.Visible = false;
                    //RadGridCharacterStatistics.Visible = false;
                    //RadGridMapStatistics.Visible = false;
                }
            }
        }

        res.IsTruncated = teamProfile.IsTruncated;
        GetOverviewResult();

        return res;

        #region Local Functions

        CharacterStatisticsRow[] GetRadGridCharacterStatisticsDataSource()
        {
            var rc = Array.Empty<CharacterStatisticsRow>();

            var replayIdToReplayLengthTotalSecondsDictionary =
                teamProfile.Replays.ToDictionary(i => i.RID, i => i.RL.TotalSeconds);

            var replayCharacters = new List<TeamProfileReplayCharacter>();
            foreach (var replay in teamProfile.Replays)
            {
                replayCharacters.AddRange(replay.RCs);
            }

            if (replayCharacters.Count == 0)
            {
                return rc;
            }

            var radGridCharacterStatisticsDataSource =
                replayCharacters.GroupBy(i => i.C).ToArray();

            var gamesPlayedMin = radGridCharacterStatisticsDataSource.Min(i => i.Count());
            var gamesPlayedMax = radGridCharacterStatisticsDataSource.Max(i => i.Count());

            var winPercentMin =
                radGridCharacterStatisticsDataSource.Min(i => (decimal)i.Count(j => j.IsW) / i.Count());
            var winPercentMax =
                radGridCharacterStatisticsDataSource.Max(i => (decimal)i.Count(j => j.IsW) / i.Count());

            rc = radGridCharacterStatisticsDataSource.Select(
                i => new CharacterStatisticsRow
                {
                    HeroPortraitURL = Global.HeroPortraitImages[i.Key],
                    PrimaryName = i.Key,
                    Character = SiteMaster.GetLocalizedString("GenericHero", i.Key),
                    CharacterURL = i.Key,
                    CharacterLevel = i.Average(j => j.CL),
                    GamesPlayed = SiteMaster.GetGaugeHtml(
                        i.Count(),
                        gamesPlayedMin,
                        gamesPlayedMax,
                        TeamCompHelper.HeroRoleColorsDictionary["Support"],
                        "N0"),
                    GamesPlayedValue = i.Count(),
                    AverageLength =
                        TimeSpan.FromSeconds((int)i.Average(j => replayIdToReplayLengthTotalSecondsDictionary[j.RID])),
                    WinPercent = SiteMaster.GetGaugeHtml(
                        (decimal)i.Count(j => j.IsW) / i.Count(),
                        winPercentMin,
                        winPercentMax),
                    WinPercentValue = (decimal)i.Count(j => j.IsW) / i.Count(),
                }).ToArray();

            return rc;
        }

        MapStatisticsRow[] GetRadGridMapStatisticsDataSource()
        {
            var rc = Array.Empty<MapStatisticsRow>();

            if (teamProfile.Replays.Length == 0)
            {
                return rc;
            }

            var radGridMapStatisticsDataSource =
                teamProfile.Replays.GroupBy(i => i.M).ToArray();

            var gamesPlayedMin = radGridMapStatisticsDataSource.Min(i => i.Count());
            var gamesPlayedMax = radGridMapStatisticsDataSource.Max(i => i.Count());

            rc = radGridMapStatisticsDataSource.Select(
                i =>
                {
                    var wins = i.Count(z => z.RCs.All(y => y.IsW));
                    var colorDic = TeamCompHelper.HeroRoleColorsDictionary;
                    var gamesPlayed = SiteMaster.GetGaugeHtml(
                        i.Count(),
                        gamesPlayedMin,
                        gamesPlayedMax,
                        colorDic["Support"],
                        "N0");
                    var winPercent = (decimal)wins / i.Count();
                    return new MapStatisticsRow
                    {
                        MapImageURL = i.Key.PrepareForImageURL(),
                        Map = i.Key,
                        MapNameLocalized = SiteMaster.GetLocalizedString("GenericMapName", i.Key),
                        GamesPlayed = gamesPlayed,
                        GamesPlayedValue = i.Count(),
                        AverageLength = TimeSpan.FromSeconds((int)i.Average(j => j.RL.TotalSeconds)),
                        WinPercent = SiteMaster.GetGaugeHtml(winPercent),
                        WinPercentValue = winPercent,
                    };
                }).ToArray();

            return rc;
        }

        void GetOverviewResult()
        {
            var replayCharacters = new List<TeamProfileReplayCharacter>();
            foreach (var replay in teamProfile.Replays)
            {
                replayCharacters.AddRange(replay.RCs);
            }

            // Gather Hero Role and Role dictionaries
            var heroRoleConcurrentDictionary = Global.GetHeroRoleConcurrentDictionary();

            var scoreHelper = new ScoreHelper
            {
                TeamProfile = teamProfile,
            };

            var replayCharactersGroupedByPlayerId = replayCharacters
                .GroupBy(i => i.PID)
                .Where(i => !isPlayerHeroOverview || i.Count() >= teamProfile.GamesPlayedRequired)
                .ToDictionary(i => i.Key, i => i.ToArray());

            if (_args.Tab[0] == 0)
            {
                if (replayCharactersGroupedByPlayerId.Count > 0)
                {
                    res.MatchStats =
                        scoreHelper.CalcStatsGenericTeam(replayCharactersGroupedByPlayerId);
                }
            }
            else if (_args.Tab[0] == 1)
            {
                // Role statistics
                var roleStatistics = replayCharacters
                    .Where(
                        i => replayCharactersGroupedByPlayerId.ContainsKey(i.PID) &&
                             heroRoleConcurrentDictionary.ContainsKey(i.C))
                    .GroupBy(i => heroRoleConcurrentDictionary[i.C]).ToDictionary(
                        i => i.Key,
                        i => i.GroupBy(j => j.PID).ToArray());

                var calcDic = new List<(string key, Func<TeamStatsResultType, string> sortKeySelector)>
                {
                    ("Tank", x => x.DamageTaken),
                    ("Bruiser", x => x.DamageTaken),
                    ("Healer", x => x.Healing),
                    ("Support", x => x.Healing),
                    ("Melee Assassin", x => x.Takedowns),
                    ("Ranged Assassin", x => x.HeroDamage),
                };

                res.RoleStats = new Dictionary<string, TeamStatsResultType[]>();
                calcDic.ForEach(
                    x =>
                    {
                        var (key, sortKeySelector) = x;
                        if (roleStatistics.ContainsKey(key))
                        {
                            res.RoleStats[key] = scoreHelper.CalcStatsRoleTeam(roleStatistics[key], sortKeySelector);
                        }
                    });
            }
            else if (_args.Tab[0] == 2 && _args.Tab[1] == 0)
            {
                if (_args.HeroDetails is not null)
                {
                    res.HeroDetails = GetHeroDetails(_args.HeroDetails);
                }
                else
                {
                    res.HeroStats = GetRadGridCharacterStatisticsDataSource();
                }
            }
            else if (_args.Tab[0] == 2 && _args.Tab[1] == 1)
            {
                if (_args.MapDetails is not null)
                {
                    res.MapDetails = GetMapDetails(_args.MapDetails);
                }
                else
                {
                    res.MapStats = GetRadGridMapStatisticsDataSource();
                }
            }
            else if (_args.Tab[0] == 3)
            {
                decimal Pct(
                    IEnumerable<TeamProfilePlayerAverageReplayCharacterUpgradeEventReplayLengthPercents> i,
                    int v) =>
                    i.SingleOrDefault(j => j.V == v)?.P ?? 0.0m;

                if (_args.Tab[1] == 0)
                {
                    // Nova
                    res.NovaStats = teamProfile
                        .PlayerAverageReplayCharacterUpgradeEventReplayLengthPercents
                        .Where(i => i.T == (int)UpgradeEventType.NovaSnipeMasterDamageUpgrade).GroupBy(i => i.PID)
                        .Select(
                            i => new TalentUpgradesNovaRow
                            {
                                PlayerID = isPlayerHeroOverview || teamProfile.Players[i.Key].IsLOO == false
                                    ? i.Key
                                    : null,
                                PlayerName = isPlayerHeroOverview ? null : teamProfile.Players[i.Key].PN,
                                GamesPlayed = i.Max(j => j.GP),
                                WinPercent = (decimal)i.Max(j => j.GW) / i.Max(j => j.GP),
                                ReplayLengthPercentAtValue0 = Pct(i, 0),
                                ReplayLengthPercentAtValue1 = Pct(i, 1),
                                ReplayLengthPercentAtValue2 = Pct(i, 2),
                                ReplayLengthPercentAtValue3 = Pct(i, 3),
                                ReplayLengthPercentAtValue4 = Pct(i, 4),
                                ReplayLengthPercentAtValue5 = Pct(i, 5),
                            }).OrderByDescending(i => i.ReplayLengthPercentAtValue5).ToArray();
                }
                else if (_args.Tab[1] == 1)
                {
                    // Gall

                    res.GallStats = teamProfile
                        .PlayerAverageReplayCharacterUpgradeEventReplayLengthPercents
                        .Where(i => i.T == (int)UpgradeEventType.GallTalentDarkDescentUpgrade).GroupBy(i => i.PID)
                        .Select(
                            i => new TalentUpgradesGallRow
                            {
                                PlayerID = isPlayerHeroOverview || teamProfile.Players[i.Key].IsLOO == false
                                    ? i.Key
                                    : null,
                                PlayerName = isPlayerHeroOverview ? null : teamProfile.Players[i.Key].PN,
                                GamesPlayed = i.Max(j => j.GP),
                                WinPercent = (decimal)i.Max(j => j.GW) / i.Max(j => j.GP),
                                ReplayLengthPercentAtValue0 = Pct(i, 0),
                                ReplayLengthPercentAtValue1 = Pct(i, 1),
                                ReplayLengthPercentAtValue2 = Pct(i, 2),
                                ReplayLengthPercentAtValue3 = Pct(i, 3),
                                ReplayLengthPercentAtValue4 = Pct(i, 4),
                            }).OrderByDescending(i => i.ReplayLengthPercentAtValue4).ToArray();
                }
            }
        }

        CharacterStatisticsDetailsRow[] GetHeroDetails(string sel)
        {
            var rc = Array.Empty<CharacterStatisticsDetailsRow>();

            var masterData = GetRadGridCharacterStatisticsDataSource();
            var charItem = masterData.Single(r => r.PrimaryName == sel);

            var locDic = Global.GetLocalizationAliasesIdentifierIDDictionary();

            if (!locDic.ContainsKey(charItem.PrimaryName))
            {
                throw new Exception($"Unable to find key in LocalizationAlias dictionary: {charItem.PrimaryName}");
            }

            var characterId = locDic[charItem.PrimaryName];
            var heroName = Global.GetLocalizationAliasesPrimaryNameDictionary()[characterId];
            var eventEntity = _dc.Events.SingleOrDefault(x => x.EventId == filterGameMode);

            var detailTableViewDataSource = isEvent

                // Event
                ? DataHelper.GetSitewideHeroTalentStatistics(
                    DateTime.MinValue,
                    DateTime.UtcNow,
                    null,
                    characterId,
                    null,
                    null,
                    _eventHelper.GetChildEvents(_dc, eventEntity, includeDisabledEvents: false).Select(i => i.EventId)
                        .ToArray(),
                    null,
                    true).Select(
                    i => new
                    {
                        i.TalentTier,
                        TalentImageURL = Global.HeroTalentImages[heroName, i.TalentName],
                        i.TalentName,
                        i.TalentDescription,
                        i.GamesPlayed,
                        i.WinPercent,
                    }).Distinct().ToArray()

                // Team
                : DataHelper.GetSitewideHeroTalentStatistics(
                    teamProfile.FilterDateTimeStart,
                    teamProfile.FilterDateTimeEnd,
                    null,
                    characterId,
                    null,
                    null,
                    teamProfile.FilterGameModes ?? DataHelper.GameModeWithStatistics,
                    teamProfile.Players.Keys.ToArray(),
                    true,
                    teamProfile.Replays.Select(i => i.RID).ToArray()).Select(
                    i => new
                    {
                        i.TalentTier,
                        TalentImageURL = Global.HeroTalentImages[heroName, i.TalentName],
                        i.TalentName,
                        i.TalentDescription,
                        i.GamesPlayed,
                        i.WinPercent,
                    }).Distinct().ToArray();

            if (detailTableViewDataSource.Length == 0)
            {
                return rc;
            }

            var gamesPlayedMax = detailTableViewDataSource.Max(i => i.GamesPlayed);

            var winPercentMin = detailTableViewDataSource.Min(i => i.WinPercent);
            var winPercentMax = detailTableViewDataSource.Max(i => i.WinPercent);

            rc = detailTableViewDataSource.Select(
                i => new CharacterStatisticsDetailsRow
                {
                    TalentTier = i.TalentTier,
                    TalentImageURL = i.TalentImageURL,
                    TalentName = i.TalentName,
                    TalentDescription = i.TalentDescription,
                    GamesPlayed = SiteMaster.GetGaugeHtml(
                        i.GamesPlayed,
                        0,
                        gamesPlayedMax,
                        TeamCompHelper.HeroRoleColorsDictionary["Support"],
                        "N0"),
                    WinPercent = SiteMaster.GetGaugeHtml(i.WinPercent, winPercentMin, winPercentMax),
                }).ToArray();

            var headers = rc.Skip(1)
                .Zip(rc, (a, b) => (Talent: a, TierBoundary: a.TalentTier != b.TalentTier))
                .Where(x => x.TierBoundary).ToList();
            var selectedHero = _args.Hero;
            var chromieMod = selectedHero == "Chromie" ? 2 : 0;
            headers.ForEach(
                x => x.Talent.HeaderStart =
                    $"Level: {Math.Max(1, x.Talent.TalentTier.GetValueOrDefault() - chromieMod)}");
            rc[0].HeaderStart = "Level: 1";

            return rc;
        }

        MapStatisticsDetailsRow[] GetMapDetails(string mapName)
        {
            var replaysDictionary = teamProfile.Replays.ToDictionary(i => i.RID, i => i);

            var replayCharacters = new List<TeamProfileReplayCharacter>();
            foreach (var replay in teamProfile.Replays)
            {
                replayCharacters.AddRange(replay.RCs);
            }

            var gamesPlayedMax = replayCharacters.Where(i => replaysDictionary[i.RID].M == mapName).GroupBy(i => i.C)
                .Max(i => i.Count());

            var rc = replayCharacters.Where(i => replaysDictionary[i.RID].M == mapName)
                .GroupBy(i => i.C).Select(
                    i => new MapStatisticsDetailsRow
                    {
                        Character = SiteMaster.GetLocalizedString("GenericHero", i.Key),
                        CharacterURL = i.Key,
                        GamesPlayed = SiteMaster.GetGaugeHtml(
                            i.Count(),
                            0,
                            gamesPlayedMax,
                            TeamCompHelper.HeroRoleColorsDictionary["Support"],
                            "N0"),
                        AverageLength =
                            TimeSpan.FromSeconds((int)i.Average(j => replaysDictionary[j.RID].RL.TotalSeconds)),
                        WinPercent = SiteMaster.GetGaugeHtml((decimal)i.Count(j => j.IsW) / i.Count()),
                    }).ToArray();
            return rc;
        }

        #endregion
    }

    private PlayerProfilePlayerRelationship[] GetPlayerProfilePlayerRelationship(
        string playerProfilePlayerRelationshipQueryType,
        int[] playerIDs,
        GameMode? gameMode,
        DateTime dateTimeStart,
        DateTime dateTimeEnd,
        int gamesPlayedTogetherRequirement = 5)
    {
        const string relationshipQuery =
            @"select p.PlayerID, p.`Name` as PlayerName, pa.FavoriteCharacter, count(*) as GamesPlayedWith, sum(rc.IsWinner) / count(*) as WinPercent, lr.CurrentMMR
                    from ReplayCharacter rc
                    join ReplayCharacter rcPlayedWith
                        on rcPlayedWith.ReplayID = rc.ReplayID
                        and rcPlayedWith.PlayerID != rc.PlayerID
                        and rcPlayedWith.IsWinner {0} rc.IsWinner
                    join Replay r on r.ReplayID = rc.ReplayID
                    join Player p on p.PlayerID = rcPlayedWith.PlayerID
                    join PlayerAggregate pa on pa.PlayerID = p.PlayerID and pa.GameMode = {2}
                    join LeaderboardRanking lr on lr.PlayerID = p.PlayerID and (lr.GameMode = {2} or ({2} = 7 and lr.GameMode = 3))
                    left join LeaderboardOptOut l on l.PlayerID = p.PlayerID
                    where r.TimestampReplay >= {{0}} and r.TimestampReplay < {{1}} and l.PlayerID is null and rc.PlayerID in ({3}) {1}
                    group by p.PlayerID, PlayerName, lr.CurrentMMR
                    having count(*) >= {4}
                    order by count(*) desc";

        var locDic =
            Global.GetLocalizationAliasesPrimaryNameDictionary();

        var gameModeTerm1 = gameMode.HasValue ? "and r.GameMode = " + (int)gameMode.Value : null;
        var gameModeTerm2 = gameMode.HasValue ? (int)gameMode.Value : (int)GameMode.QuickMatch;

        var commandText = string.Format(
            relationshipQuery,
            playerProfilePlayerRelationshipQueryType,
            gameModeTerm1,
            gameModeTerm2,
            string.Join(",", playerIDs),
            gamesPlayedTogetherRequirement);

        var relationships = _dc.PlayerRelationshipCustoms.FromSqlRaw(commandText, dateTimeStart, dateTimeEnd)
            .Select(
                x => new PlayerProfilePlayerRelationship
                {
                    PlayerID = x.PlayerID,
                    HeroPortraitURL = locDic.ContainsKey(x.FavoriteCharacter)
                        ? Global.HeroPortraitImages[locDic[x.FavoriteCharacter]]
                        : "/Images/Heroes/Portraits/Unknown.png",
                    PlayerName = x.PlayerName,
                    FavoriteHero = locDic.ContainsKey(x.FavoriteCharacter)
                        ? locDic[x.FavoriteCharacter]
                        : "Unknown",
                    GamesPlayedWith = (int)x.GamesPlayedWith,
                    WinPercent = x.WinPercent,
                    CurrentMMR = x.CurrentMMR,
                }).ToArray();

        return relationships;
    }
}
