using HelperCore;
using Heroes.DataAccessLayer.CustomModels;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.MatchSummary.Models;
using HotsLogsApi.BL.Resources;
using HotsLogsApi.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotsLogsApi.BL.Migration.MatchSummary;

public class Helper : HelperBase<MatchSummaryResponse>
{
    private const int ReplayFileLifetimeInDays = 30;

    private const string MySqlCommandTextIsValidReplay =
        @"select
            r.ReplayID,
            r.GameMode,
            r.MapID,
            min(rs.ReplayShareID) is not null as IsReplayShared,
            group_concat(rc.PlayerID order by rc.PlayerID) as PlayerIDs,
            e.IsEnabled is not null and e.IsEnabled as IsEventReplayAndEnabled,
            r.ReplayLength,
            r.TimestampReplay,
            r.TimestampCreated,
            (select rthb.ReplayID from Replayteamheroban rthb where rthb.ReplayID = r.ReplayID limit 1) is not null as IsAnyReplayTeamHeroBans,
            (select rcue.ReplayID from ReplayCharacterUpgradeEventReplayLengthPercent rcue where rcue.ReplayID = r.ReplayID limit 1) is not null as IsAnyReplayCharacterUpgradeEventReplayLengthPercents,
            (select rpxb.ReplayID from ReplayPeriodicXPBreakdown rpxb where rpxb.ReplayID = r.ReplayID limit 1) is not null as IsAnyReplayPeriodicXPBreakdowns,
            (select rto.ReplayID from ReplayTeamObjective rto where rto.ReplayID = r.ReplayID limit 1) is not null as IsAnyReplayTeamObjectives
            from Replay r
            join ReplayCharacter rc on rc.ReplayID = r.ReplayID
            left join ReplayShare rs on rs.ReplayID = r.ReplayID
            left join `Event` e on e.EventID = r.GameMode
            where r.ReplayID = {0}
            group by r.ReplayID";

    private const bool IsAnonymous = false;
    private const bool DisplayScoreResults = true;
    private const bool DisplayAnonymousOrder = false;

    private readonly MatchSummaryArgs _args;
    [CanBeNull] private readonly AppUser _user;

    public Helper(MatchSummaryArgs args, AppUser user, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
        _user = user;
    }

    public override MatchSummaryResponse MainCalculation()
    {
        MatchSummaryResponse res = new();
        var replayId = _args.ReplayId;

        if (replayId == -1)
        {
            return res;
        }

        //if (RadGridReplayCharacterScoreResults is null)
        //{
        //    return;
        //}

        //RadGridReplayCharacterScoreResults.CssClass +=
        //    " " + RadGridReplayCharacterScoreResultsDataTableUniqueCssClass;

        void Internal()
        {
            bool ValidateCustomGameRequirements(QueryContainerIsValidReplay queryContainerIsValidReplay1)
            {
                // If this is a Custom game, make sure the user played in it, or make sure it's been shared
                if (DataHelper.GameModeWithStatistics.All(i => i != queryContainerIsValidReplay1.GameMode) &&
                    queryContainerIsValidReplay1.GameMode < 1000 && !queryContainerIsValidReplay1.IsReplayShared)
                {
                    // No permalink for a private match
                    res.PermalinkVisible = false;

                    // Make sure they verified their account
                    if (!(_user?.IsBnetAuthorized ?? false))
                    {
                        return true;
                    }

                    var playerId = _user.MainPlayerId!.Value; // If we're bnet authorized, we have a player id

                    if (queryContainerIsValidReplay1.PlayerIDs.All(i => i != playerId))
                    {
                        return true;
                    }
                }

                // If this is an Event replay, let's make sure the Event is public
                if (queryContainerIsValidReplay1.GameMode > 1000)
                {
                    if (!queryContainerIsValidReplay1.IsEventReplayAndEnabled)
                    {
                        return true;
                    }

                    // Add Download link for Event replays
                    if (queryContainerIsValidReplay1.TimestampCreated >
                        DateTime.UtcNow.AddDays(ReplayFileLifetimeInDays * -1))
                    {
                        res.ReplayDownloadVisible = true;
                        res.ReplayDownloadHref =
                            "/Replays/DownloadReplay?ReplayID=" + queryContainerIsValidReplay1.ReplayID;
                    }
                }

                return false;
            }

            using var scope = Svcp.CreateScope();
            var dc = HeroesdataContext.Create(scope);
            var dbReplay = dc.Replays.SingleOrDefault(r => r.ReplayId == replayId);

            if (dbReplay == null)
            {
                return;
            }

            res.ReplayLength = dbReplay.ReplayLength;
            res.ReplayTime = new DateTimeOffset(dbReplay.TimestampReplay, TimeSpan.Zero);

            var queryContainerIsValidReplay = dc.QueryContainerIsValidReplays
                .FromSqlRaw(MySqlCommandTextIsValidReplay, replayId).SingleOrDefault();

            if (queryContainerIsValidReplay is null)
            {
                // No record for this ReplayID
                return;
            }

            if (ValidateCustomGameRequirements(queryContainerIsValidReplay))
            {
                return;
            }

            // This is a valid Replay

            // First let's determine if we can show the Replay Viewer
            var mapString = queryContainerIsValidReplay.MapID != 0
                ? Global.GetLocalizationAliasesPrimaryNameDictionary()[queryContainerIsValidReplay.MapID]
                : "Unknown";

            // ReSharper disable RedundantLogicalConditionalExpressionOperand
            res.PanelReplayViewerVisible = !IsAnonymous && DisplayScoreResults &&
                                           DataParser.MapOffsets.ContainsKey(mapString) &&
                                           queryContainerIsValidReplay.TimestampCreated >
                                           DateTime.UtcNow.AddDays(ReplayFileLifetimeInDays * -1);
            // ReSharper restore RedundantLogicalConditionalExpressionOperand

            // Now let's populate the RadGrids
            res.MapName = GetLocalizedString("GenericMap", mapString);

            res.PermalinkHref = @"/Player/MatchSummaryContainer?ReplayID=" + replayId;

            var rch = scope.ServiceProvider.GetRequiredService<ReplayCharacterHelper>();
            var replayCharacterDetails = rch.GetReplayCharacterDetails(replayId)
                .ToDictionary(i => i.PlayerID, i => i);

            // Estimate MMR if it hasn't yet been calculated
            if (replayCharacterDetails.Values.Any(i => !i.MMRBefore.HasValue))
            {
                var replayCharacterDetailsDictionary = replayCharacterDetails
                    .Values
                    .Where(i => !i.MMRBefore.HasValue).ToDictionary(i => i.PlayerID, i => i);

                var gameModeForMMR =
                    DataHelper.GameModeWithMMR.Any(i => i == queryContainerIsValidReplay.GameMode)
                        ? (GameMode)queryContainerIsValidReplay.GameMode
                        : GameMode.StormLeague;

                var keys = replayCharacterDetailsDictionary.Keys;
                var q3 = dc.LeaderboardRankings
                    .Where(r => r.GameMode == (int)gameModeForMMR && keys.Contains(r.PlayerId));
                foreach (var e3 in q3)
                {
                    replayCharacterDetailsDictionary[e3.PlayerId].MMRBefore = e3.CurrentMmr;
                }
            }

            var averageHeroLevelPerTeam = replayCharacterDetails.Values
                .GroupBy(i => i.IsWinner).ToDictionary(
                    i => i.Key,
                    i => i.Average(j => j.CharacterLevel != 0 ? j.CharacterLevel : (double?)null));

            var votes = new Dictionary<int, Heroes.DataAccessLayer.Models.Vote>();
            int? authPlayerId = null;
            var participated = false;
            if (_user?.MainPlayerId != null)
            {
                authPlayerId = _user.MainPlayerId;
                votes = dc.Votes
                    .Where(x => x.VotingPlayerId == authPlayerId && x.TargetReplayId == replayId)
                    .ToDictionary(x => x.TargetPlayerId);

                participated =
                    dc.ReplayCharacters.Any(x => x.ReplayId == replayId && x.PlayerId == authPlayerId);
            }

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            // ReSharper disable HeuristicUnreachableCode
#pragma warning disable CS0162
            var ds = replayCharacterDetails.Values.Select(
                i => new ReplayPlayerRecord
                {
                    MatchAwards = !IsAnonymous
                        ? string.Join(
                            string.Empty,
                            i.MatchAwards.Select(
                                j => ((MatchAwardType)j).GetMatchAwardTypeHtmlIcon(i.IsWinner ? 0 : 1)))
                        // ReSharper disable once HeuristicUnreachableCode
                        : null,
                    MatchAwards2 = IsAnonymous
                        ? Array.Empty<AwardRow>()
                        : i.MatchAwards.Select(aw => GetAwardName(i.IsWinner, aw)).ToArray(),
                    // ReSharper disable once RedundantLogicalConditionalExpressionOperand
                    PlayerID = !i.IsLeaderboardOptOut && !IsAnonymous ? i.PlayerID : null,
                    RealPlayerID = i.PlayerID,
                    PlayerName = !IsAnonymous ? i.PlayerName : null,
                    VoteUp = votes.ContainsKey(i.PlayerID) && votes[i.PlayerID].Up.HasValue &&
                             votes[i.PlayerID].Up.Value != 0,
                    VoteDown = votes.ContainsKey(i.PlayerID) && votes[i.PlayerID].Up is 0,
                    ShowVoteIcons = authPlayerId.HasValue && authPlayerId != i.PlayerID && participated,
                    Reputation = i.Reputation,
                    Character = GetLocalizedString("GenericHero", i.Character),
                    CharacterURL = i.Character,
                    HeroPortraitImageURL = Global.HeroPortraitImages[i.Character],
                    CharacterLevel = i.CharacterLevel,
                    CharacterLevelNumber = i.CharacterLevel != 0 ? i.CharacterLevel :
                        averageHeroLevelPerTeam.ContainsKey(i.IsWinner) ? averageHeroLevelPerTeam[i.IsWinner] :
                        null,
                    TalentImageURL01 = i.TalentImageURL01,
                    TalentImageURL04 = i.TalentImageURL04,
                    TalentImageURL07 = i.TalentImageURL07,
                    TalentImageURL10 = i.TalentImageURL10,
                    TalentImageURL13 = !IsAnonymous ? i.TalentImageURL13 : null,
                    TalentImageURL16 = !IsAnonymous ? i.TalentImageURL16 : null,
                    TalentImageURL20 = !IsAnonymous ? i.TalentImageURL20 : null,
                    TalentNameDescription01 = i.TalentNameDescription01,
                    TalentNameDescription04 = i.TalentNameDescription04,
                    TalentNameDescription07 = i.TalentNameDescription07,
                    TalentNameDescription10 = i.TalentNameDescription10,
                    TalentNameDescription13 = !IsAnonymous ? i.TalentNameDescription13 : null,
                    TalentNameDescription16 = !IsAnonymous ? i.TalentNameDescription16 : null,
                    TalentNameDescription20 = !IsAnonymous ? i.TalentNameDescription20 : null,
                    TalentName01 = i.TalentName01,
                    TalentName04 = i.TalentName04,
                    TalentName07 = i.TalentName07,
                    TalentName10 = i.TalentName10,
                    TalentName13 = !IsAnonymous ? i.TalentName13 : null,
                    TalentName16 = !IsAnonymous ? i.TalentName16 : null,
                    TalentName20 = !IsAnonymous ? i.TalentName20 : null,
                    Team = !DisplayAnonymousOrder ? i.IsWinner : GetIsTeam1Winner(i.ReplayID) == i.IsWinner,
                    MMRBefore = !IsAnonymous ? i.MMRBefore ?? 1700 : (i.MMRBefore ?? 1700) / 100 * 100,
                    MMRChange = !IsAnonymous ? i.MMRChange : null,
                }).ToArray();
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            // ReSharper restore HeuristicUnreachableCode
#pragma warning restore CS0162

            var gp = ds.GroupBy(x => x.Team).ToDictionary(
                x => x.Key,
                x =>
                {
                    var mmr = x.Average(y => y.MMRBefore).ToString("0");
                    var anyLvl = x.Any(y => y.CharacterLevelNumber.HasValue);
                    var lvl = anyLvl
                        ? x.Where(y => y.CharacterLevelNumber.HasValue).Average(y => y.CharacterLevelNumber.Value)
                            .ToString("0.0")
                        : string.Empty;
                    return $"MMR: {mmr}; Hero Lvl: {lvl}";
                });
            if (ds.Length > 0)
            {
                ds[0].HeaderStart = gp[true];
                ds.First(x => !x.Team).HeaderStart = gp[false];
            }

            res.MatchDetails = ds;

            if (
                //!Request.Browser.IsMobileDevice && DisplayScoreResults &&
                replayCharacterDetails.Any() &&
                replayCharacterDetails.Values.All(i => i.ReplayCharacterScoreResult != null))
            {
                var takedownsMax = replayCharacterDetails.Values.Max(i => i.ReplayCharacterScoreResult.Takedowns);
                var soloKillsMax = replayCharacterDetails.Values.Max(i => i.ReplayCharacterScoreResult.SoloKills);
                var assistsMax = replayCharacterDetails.Values.Max(i => i.ReplayCharacterScoreResult.Assists);
                var deathsMax = replayCharacterDetails.Values.Max(i => i.ReplayCharacterScoreResult.Deaths);
                var deathsMin = replayCharacterDetails.Values.Min(i => i.ReplayCharacterScoreResult.Deaths);

                var heroDamageMax =
                    replayCharacterDetails.Values.Max(i => i.ReplayCharacterScoreResult.HeroDamage);
                var siegeDamageMax =
                    replayCharacterDetails.Values.Max(i => i.ReplayCharacterScoreResult.SiegeDamage);
                var healingMax = replayCharacterDetails.Values.Max(i => i.ReplayCharacterScoreResult.Healing);
                var selfHealingMax =
                    replayCharacterDetails.Values.Max(i => i.ReplayCharacterScoreResult.SelfHealing);
                var damageTakenMax =
                    replayCharacterDetails.Values.Max(i => i.ReplayCharacterScoreResult.DamageTaken);
                var experienceContributionMax =
                    replayCharacterDetails.Values.Max(i => i.ReplayCharacterScoreResult.ExperienceContribution);
                var heroContributionMax = replayCharacterDetails.Values.Max(
                    i => i.ReplayCharacterScoreResult.HeroDamage + (i.ReplayCharacterScoreResult.Healing ?? 0) +
                         i.ReplayCharacterScoreResult.SelfHealing +
                         ((i.ReplayCharacterScoreResult.DamageTaken ?? 0) /
                          (i.ReplayCharacterScoreResult.Deaths != 0
                              ? i.ReplayCharacterScoreResult.Deaths
                              : 1)));

                var scoreResults = replayCharacterDetails.Values.Select(
                        i => new
                        {
                            PlayerID = !i.IsLeaderboardOptOut ? (int?)i.PlayerID : null,
                            i.PlayerName,
                            Character = GetLocalizedString("GenericHero", i.Character),
                            CharacterURL = i.Character,
                            Team = i.IsWinner,
                            Takedowns = SiteMaster.GetGaugeHtml(
                                i.ReplayCharacterScoreResult.Takedowns,
                                0,
                                takedownsMax,
                                TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"],
                                "N0"),
                            SoloKills = SiteMaster.GetGaugeHtml(
                                i.ReplayCharacterScoreResult.SoloKills,
                                0,
                                soloKillsMax,
                                TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"],
                                "N0"),
                            Assists = SiteMaster.GetGaugeHtml(
                                i.ReplayCharacterScoreResult.Assists,
                                0,
                                assistsMax,
                                TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"],
                                "N0"),
                            Deaths = SiteMaster.GetGaugeHtml(
                                i.ReplayCharacterScoreResult.Deaths,
                                0,
                                deathsMax,
                                TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"],
                                "N0"),
                            i.ReplayCharacterScoreResult.TimeSpentDead,
                            HeroDamage = SiteMaster.GetGaugeHtml(
                                i.ReplayCharacterScoreResult.HeroDamage,
                                0,
                                heroDamageMax,
                                TeamCompHelper.HeroRoleColorsDictionary["Melee Assassin"],
                                "N0"),
                            SiegeDamage = SiteMaster.GetGaugeHtml(
                                i.ReplayCharacterScoreResult.SiegeDamage,
                                0,
                                siegeDamageMax,
                                TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"],
                                "N0"),
                            Healing = SiteMaster.GetGaugeHtml(
                                i.ReplayCharacterScoreResult.Healing,
                                0,
                                healingMax,
                                TeamCompHelper.HeroRoleColorsDictionary["Healer"],
                                "N0"),
                            SelfHealing = SiteMaster.GetGaugeHtml(
                                i.ReplayCharacterScoreResult.SelfHealing,
                                0,
                                selfHealingMax,
                                TeamCompHelper.HeroRoleColorsDictionary["Support"],
                                "N0"),
                            DamageTaken = SiteMaster.GetGaugeHtml(
                                i.ReplayCharacterScoreResult.DamageTaken,
                                0,
                                damageTakenMax,
                                TeamCompHelper.HeroRoleColorsDictionary["Tank"],
                                "N0"),
                            i.ReplayCharacterScoreResult.MercCampCaptures,
                            ExperienceContribution = SiteMaster.GetGaugeHtml(
                                i.ReplayCharacterScoreResult.ExperienceContribution,
                                0,
                                experienceContributionMax,
                                TeamCompHelper.HeroRoleColorsDictionary["Support"],
                                "N0"),
                            ScoreSoloKills = soloKillsMax != 0
                                ? (decimal)i.ReplayCharacterScoreResult.SoloKills / soloKillsMax
                                : 0m,
                            ScoreTakedowns = takedownsMax != 0
                                ? (decimal)i.ReplayCharacterScoreResult.Takedowns / takedownsMax
                                : 0m,
                            ScoreDeaths = deathsMax != 0 && deathsMax - deathsMin != 0
                                ? 1 - ((decimal)(i.ReplayCharacterScoreResult.Deaths - deathsMin) /
                                       (deathsMax - deathsMin))
                                : 1m,
                            ScoreHeroContribution = (decimal)(i.ReplayCharacterScoreResult.HeroDamage +
                                                              (i.ReplayCharacterScoreResult.Healing ?? 0) +
                                                              i.ReplayCharacterScoreResult.SelfHealing +
                                                              ((i.ReplayCharacterScoreResult.DamageTaken ?? 0) /
                                                               (i.ReplayCharacterScoreResult.Deaths != 0
                                                                   ? i.ReplayCharacterScoreResult.Deaths
                                                                   : 1))) / heroContributionMax,
                            ScoreSiegeContribution = siegeDamageMax != 0
                                ? (decimal)i.ReplayCharacterScoreResult.SiegeDamage / siegeDamageMax
                                : 0m,
                            ScoreXPContribution = experienceContributionMax != 0
                                ? (decimal)i.ReplayCharacterScoreResult.ExperienceContribution /
                                  experienceContributionMax
                                : 0m,
                        })
                    .Select(
                        i => new RadGridReplayCharacterScoreResultsRow
                        {
                            PlayerID = i.PlayerID,
                            PlayerName = i.PlayerName,
                            Character = i.Character,
                            CharacterURL = i.CharacterURL,
                            Team = i.Team,
                            Takedowns = i.Takedowns,
                            SoloKills = i.SoloKills,
                            Assists = i.Assists,
                            Deaths = i.Deaths,
                            TimeSpentDead = i.TimeSpentDead,
                            HeroDamage = i.HeroDamage,
                            SiegeDamage = i.SiegeDamage,
                            Healing = i.Healing,
                            SelfHealing = i.SelfHealing,
                            DamageTaken = i.DamageTaken,
                            MercCampCaptures = i.MercCampCaptures,
                            ExperienceContribution = i.ExperienceContribution,
                            ScoreTooltip = "<span class='titlePopupHover'>" + SiteMaster.GetGaugeHtml(
                                               (i.ScoreSoloKills + i.ScoreTakedowns + i.ScoreDeaths +
                                                i.ScoreHeroContribution +
                                                i.ScoreSiegeContribution + i.ScoreXPContribution) / 6) +
                                           $@"<span class=""titlePopup titlePopupHidden""><table>
                                <tr><td><strong>Kills</strong></td><td>{i.ScoreSoloKills:P1}</td></tr>
                                <tr><td><strong>Teamwork (Takedowns)</strong></td><td>{i.ScoreTakedowns:P1}</td></tr>
                                <tr><td><strong>Deaths</strong></td><td>{i.ScoreDeaths:P1}</td></tr>
                                <tr><td><strong>Role<br>(Hero Dmg + Heal/Self Heal +<br>Warrior Dmg Taken per Death)</strong></td><td>{i.ScoreHeroContribution:P1}</td></tr>
                                <tr><td><strong>Siege</strong></td><td>{i.ScoreSiegeContribution:P1}</td></tr>
                                <tr><td><strong>XP</strong></td><td>{i.ScoreXPContribution:P1}</td></tr></table>
                            </span>" + "</span>",
                        }).ToArray();

                res.ScoreResults = scoreResults;

                var teamTotals = replayCharacterDetails.Values
                    .ToLookup(x => x.IsWinner)
                    .Select(
                        x => new ScoreResultsTotals
                        {
                            Team = x.Key ? "Winning Team" : "Losing Team",
                            Takedowns = x.Sum(y => y.ReplayCharacterScoreResult.Takedowns).ToString("N0"),
                            SoloKills = x.Sum(y => y.ReplayCharacterScoreResult.SoloKills).ToString("N0"),
                            Assists = x.Sum(y => y.ReplayCharacterScoreResult.Assists).ToString("N0"),
                            Deaths = x.Sum(y => y.ReplayCharacterScoreResult.Deaths).ToString("N0"),
                            TimeSpentDead = x.Select(y => y.ReplayCharacterScoreResult.TimeSpentDead)
                                .Aggregate((y, z) => y + z),
                            HeroDamage = x.Sum(y => y.ReplayCharacterScoreResult.HeroDamage).ToString("N0"),
                            SiegeDamage = x.Sum(y => y.ReplayCharacterScoreResult.SiegeDamage).ToString("N0"),
                            Healing = x.Sum(y => y.ReplayCharacterScoreResult.Healing)?.ToString("N0"),
                            SelfHealing = x.Sum(y => y.ReplayCharacterScoreResult.SelfHealing).ToString("N0"),
                            DamageTaken = x.Sum(y => y.ReplayCharacterScoreResult.DamageTaken)?.ToString("N0"),
                            MercCampCaptures = x.Sum(y => y.ReplayCharacterScoreResult.MercCampCaptures)
                                .ToString("N0"),
                            ExperienceContribution = x.Sum(y => y.ReplayCharacterScoreResult.ExperienceContribution)
                                .ToString("N0"),
                        }).ToArray();

                res.CharacterScoreResultsTotals = teamTotals;
            }
            //else
            //{
            //    res.PanelScoreResultsVisible = false;
            //}

            var localizationAliasPrimaryNameDictionary =
                Global.GetLocalizationAliasesPrimaryNameDictionary();
            localizationAliasPrimaryNameDictionary[0] = "Unknown";

            // Hero Bans
            // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            if (DisplayScoreResults && queryContainerIsValidReplay.IsAnyReplayTeamHeroBans)
            {
                var heroBans =
                    GetReplayTeamHeroBan(dc, queryContainerIsValidReplay.ReplayID).Select(
                        i => new HeroBanRow
                        {
                            Team = i.IsWinner != 0,
                            BanPhase = i.BanPhase + 1,
                            HeroPortraitURL =
                                Global.HeroPortraitImages[localizationAliasPrimaryNameDictionary[i.CharacterId]],
                            Character = GetLocalizedString(
                                "GenericHero",
                                localizationAliasPrimaryNameDictionary[i.CharacterId]),
                            CharacterURL = localizationAliasPrimaryNameDictionary[i.CharacterId],
                        }).OrderByDescending(i => i.Team).ThenBy(i => i.BanPhase).ToArray();

                res.HeroBans = heroBans;
            }
            //else
            //{
            //    PanelHeroBans.Visible = false;
            //}

            // Talent Upgrades
            // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            if (DisplayScoreResults &&
                queryContainerIsValidReplay.IsAnyReplayCharacterUpgradeEventReplayLengthPercents)
            {
                var
                    replayCharacterUpgradeEventReplayLengthPercents =
                        GetReplayCharacterUpgradeEventReplayLengthPercent(dc, queryContainerIsValidReplay.ReplayID);

                var radGridTalentUpgradesDataSource = replayCharacterUpgradeEventReplayLengthPercents.Where(
                    i => i.UpgradeEventType == (int)UpgradeEventType.NovaSnipeMasterDamageUpgrade ||
                         i.UpgradeEventType == (int)UpgradeEventType.GallTalentDarkDescentUpgrade).GroupBy(
                    i => new
                    {
                        i.ReplayId,
                        i.PlayerId,
                        replayCharacterDetails[i.PlayerId].PlayerName,
                        replayCharacterDetails[i.PlayerId].Character,
                        replayCharacterDetails[i.PlayerId].IsWinner,
                        UpgradeEventType = (UpgradeEventType)i.UpgradeEventType,
                    }).Select(
                    i => new TalentUpgradesRow
                    {
                        PlayerId = i.Key.PlayerId,
                        PlayerName = i.Key.PlayerName,
                        HeroPortraitImageURL = Global.HeroPortraitImages[i.Key.Character],
                        Character = GetLocalizedString("GenericHero", i.Key.Character),
                        CharacterURL = i.Key.Character,
                        Team = i.Key.IsWinner,
                        TalentImageURL =
                            Global.HeroTalentImages[i.Key.Character, i.Key.UpgradeEventType.GetTalentName()],
                        TalentName = i.Key.UpgradeEventType.GetTalentName(),
                        ReplayLengthPercentAtValue0 = i.Any(j => j.UpgradeEventValue == 0)
                            ? i.Single(j => j.UpgradeEventValue == 0).ReplayLengthPercent
                            : 0.0m,
                        ReplayLengthPercentAtValue1 = i.Any(j => j.UpgradeEventValue == 1)
                            ? i.Single(j => j.UpgradeEventValue == 1).ReplayLengthPercent
                            : 0.0m,
                        ReplayLengthPercentAtValue2 = i.Any(j => j.UpgradeEventValue == 2)
                            ? i.Single(j => j.UpgradeEventValue == 2).ReplayLengthPercent
                            : 0.0m,
                        ReplayLengthPercentAtValue3 = i.Any(j => j.UpgradeEventValue == 3)
                            ? i.Single(j => j.UpgradeEventValue == 3).ReplayLengthPercent
                            : 0.0m,
                        ReplayLengthPercentAtValue4 = i.Any(j => j.UpgradeEventValue == 4)
                            ? i.Single(j => j.UpgradeEventValue == 4).ReplayLengthPercent
                            : 0.0m,
                        ReplayLengthPercentAtValue5 = i.Any(j => j.UpgradeEventValue >= 5)
                            ? i.Where(j => j.UpgradeEventValue >= 5).Sum(j => j.ReplayLengthPercent)
                            : i.Key.UpgradeEventType == UpgradeEventType.GallTalentDarkDescentUpgrade
                                ? null
                                : 0.0m,
                    }).OrderByDescending(i => i.Team).ThenBy(i => i.Character).ThenBy(i => i.PlayerName).ToArray();

                if (radGridTalentUpgradesDataSource.Length > 0)
                {
                    res.TalentUpgrades = radGridTalentUpgradesDataSource;
                }
                //else
                //{
                //    RadGridTalentUpgrades.Visible = false;
                //}

                if (!radGridTalentUpgradesDataSource.Any(i => i.ReplayLengthPercentAtValue5.HasValue))
                {
                    res.LengthPercentAtValue5Hide = true;
                }

                var radGridTalentUpgradesStacksDataSource = replayCharacterUpgradeEventReplayLengthPercents.Where(
                    i => i.UpgradeEventType == (int)UpgradeEventType.RegenMasterStacks ||
                         i.UpgradeEventType == (int)UpgradeEventType.MarksmanStacks).Select(
                    i =>
                    {
                        var talentName = ((UpgradeEventType)i.UpgradeEventType).GetTalentName();
                        var heroName = replayCharacterDetails[i.PlayerId].Character;
                        return new TalentUpgradesStacksRow
                        {
                            PlayerId = i.PlayerId,
                            PlayerName = replayCharacterDetails[i.PlayerId].PlayerName,
                            HeroPortraitImageURL = Global.HeroPortraitImages[heroName],
                            Character = GetLocalizedString("GenericHero", heroName),
                            CharacterURL = heroName,
                            Team = replayCharacterDetails[i.PlayerId].IsWinner,
                            TalentImageURL = Global.HeroTalentImages[heroName, talentName],
                            TalentName = talentName,
                            Stacks = i.UpgradeEventValue,
                        };
                    }).OrderByDescending(i => i.Team).ThenBy(i => i.Character).ThenBy(i => i.PlayerName).ToArray();

                if (radGridTalentUpgradesStacksDataSource.Length > 0)
                {
                    res.TalentUpgradesStacks = radGridTalentUpgradesStacksDataSource;
                }
                //else
                //{
                //    RadGridTalentUpgradesStacks.Visible = false;
                //}
            }
            //else
            //{
            //    PanelTalentUpgrades.Visible = false;
            //}

            // Match Log
            // ReSharper disable RedundantLogicalConditionalExpressionOperand
            var isMatchLogAvailable = !IsAnonymous && DisplayScoreResults &&
                                      queryContainerIsValidReplay.TimestampCreated >
                                      DateTime.UtcNow.AddDays(ReplayFileLifetimeInDays * -1);
            // ReSharper restore RedundantLogicalConditionalExpressionOperand
            res.PanelMatchLogVisible = isMatchLogAvailable;

            // XP Summary
            // Ahli dropbox for Team level experience: https://www.dropbox.com/s/fhuk775m9zmvap5/teamlevel%20experience.txt?dl=0
            // Reddit post: https://www.reddit.com/r/heroesofthestorm/comments/2p0c6k/datamined_mercenary_stats/cmsdglg
            // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            if (!isMatchLogAvailable && DisplayScoreResults &&
                queryContainerIsValidReplay.IsAnyReplayPeriodicXPBreakdowns)
            {
                var chartXpSummary =
                    GetReplayPeriodicXpBreakdown(dc, queryContainerIsValidReplay.ReplayID)
                        .GroupBy(i => i.GameTimeMinute).Select(
                            i => new
                            {
                                GameTimeMinute = i.Key,
                                WinnerMinionXP = i.Sum(j => j.IsWinner != 0 ? j.MinionXp : 0),
                                WinnerCreepXP = i.Sum(j => j.IsWinner != 0 ? j.CreepXp : 0),
                                WinnerStructureXP = i.Sum(j => j.IsWinner != 0 ? j.StructureXp : 0),
                                WinnerHeroXP = i.Sum(j => j.IsWinner != 0 ? j.HeroXp : 0),
                                WinnerTrickleXP = i.Sum(j => j.IsWinner != 0 ? j.TrickleXp : 0),
                                LoserMinionXP = i.Sum(j => j.IsWinner == 0 ? j.MinionXp : 0),
                                LoserCreepXP = i.Sum(j => j.IsWinner == 0 ? j.CreepXp : 0),
                                LoserStructureXP = i.Sum(j => j.IsWinner == 0 ? j.StructureXp : 0),
                                LoserHeroXP = i.Sum(j => j.IsWinner == 0 ? j.HeroXp : 0),
                                LoserTrickleXP = i.Sum(j => j.IsWinner == 0 ? j.TrickleXp : 0),
                            }).Select(
                            i => new ChartXpSummaryRow
                            {
                                GameTimeMinute = i.GameTimeMinute,
                                WinnerMinionAndCreepXP = i.WinnerMinionXP + i.WinnerCreepXP,
                                WinnerStructureXP = i.WinnerStructureXP,
                                WinnerHeroXP = i.WinnerHeroXP,
                                WinnerTrickleXP = i.WinnerTrickleXP,
                                MinionAndCreepXPTooltip =
                                    @"<table><tr><td style='text-align: right;'><strong>Minion XP: </strong></td><td><strong>" +
                                    i.WinnerMinionXP +
                                    @"</strong></td></tr><tr><td style='text-align: right;'><strong>Creep XP: </strong></td><td><strong>" +
                                    i.WinnerCreepXP +
                                    @"</strong></td></tr><tr><td style='text-align: right;'>Minion XP: </td><td>" +
                                    i.LoserMinionXP +
                                    @"</td></tr><tr><td style='text-align: right;'>Creep XP: </td><td>" +
                                    i.LoserCreepXP + @"</td></tr></table>",
                                StructureXPTooltip =
                                    @"<table><tr><td style='text-align: right;'><strong>Structure XP: </strong></td><td><strong>" +
                                    i.WinnerStructureXP +
                                    @"</strong></td></tr><tr><td style='text-align: right;'>Structure XP: </td><td>" +
                                    i.LoserStructureXP + @"</td></tr></table>",
                                HeroXPTooltip =
                                    @"<table><tr><td style='text-align: right;'><strong>Hero Kill XP: </strong></td><td><strong>" +
                                    i.WinnerHeroXP +
                                    @"</strong></td></tr><tr><td style='text-align: right;'>Hero Kill XP: </td><td>" +
                                    i.LoserHeroXP + @"</td></tr></table>",
                                TrickleXPTooltip =
                                    @"<table><tr><td style='text-align: right;'><strong>Trickle XP: </strong></td><td><strong>" +
                                    i.WinnerTrickleXP +
                                    @"</strong></td></tr><tr><td style='text-align: right;'>Trickle XP: </td><td>" +
                                    i.LoserTrickleXP + @"</td></tr></table>",
                                LoserMinionAndCreepXP = i.LoserMinionXP + i.LoserCreepXP,
                                LoserStructureXP = i.LoserStructureXP,
                                LoserHeroXP = i.LoserHeroXP,
                                LoserTrickleXP = i.LoserTrickleXP,
                            }).OrderBy(i => i.GameTimeMinute).ToArray();

                var json = JsonConvert.SerializeObject(chartXpSummary);
                res.ChartXpSummaryJson = json;
                //RadHtmlChartXPSummary.DataSource = _chartXPSummary;
                //RadHtmlChartXPSummary.ID += Guid.NewGuid().ToString();

                //if (!Page.IsPostBack)
                //{
                //    //foreach (PlotBand plotBand in RadHtmlChartXPSummary.PlotArea.YAxis.PlotBands)
                //    //{
                //    //    plotBand.From -= 500;
                //    //    plotBand.To += 500;
                //    //}
                //}
            }
            //else
            //{
            //    PanelExperienceSummary.Visible = false;
            //}

            // Team Objectives
            // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            if (!isMatchLogAvailable && DisplayScoreResults &&
                queryContainerIsValidReplay.IsAnyReplayTeamObjectives)
            {
                var replayTeamObjectives = GetReplayTeamObjective(dc, queryContainerIsValidReplay.ReplayID);

                if (replayTeamObjectives.All(i => i.PlayerId == null))
                {
                    res.HideTeamObjectivesHeroField = true;
                }

                var infernalEvents = replayTeamObjectives.Where(
                        i =>
                            i.TeamObjectiveType is >= (int)TeamObjectiveType
                                    .InfernalShrinesPunisherKilledWithPunisherType
                                and <= (int)TeamObjectiveType.InfernalShrinesPunisherKilledWithHeroDamageDone)
                    .GroupBy(i => i.TimeSpan).Select(
                        i => new Tmp1
                        {
                            Team = i.First().IsWinner != 0,
                            TimeSpan = i.First().TimeSpan,
                            PlayerName = null,
                            Character = null,
                            CharacterURL = null,
                            HeroPortraitURL = null,
                            TeamObjectiveType = ((TeamObjectiveType)i.First().TeamObjectiveType)
                                .GetTeamObjectiveTypeString(),
                            Value = string.Join(
                                ", ",
                                i.OrderBy(j => j.TeamObjectiveType).Select(
                                    j => ((TeamObjectiveType)j.TeamObjectiveType)
                                        .GetTeamObjectiveValueString(j.Value))),
                        }).ToArray();

                var a = replayTeamObjectives.Where(
                    i =>
                        i.TeamObjectiveType <
                        (int)TeamObjectiveType.InfernalShrinesPunisherKilledWithPunisherType ||
                        i.TeamObjectiveType >
                        (int)TeamObjectiveType.InfernalShrinesPunisherKilledWithHeroDamageDone).Select(
                    i => new
                    {
                        ReplayTeamObjective = i,
                        TeamObjectiveType = (TeamObjectiveType)i.TeamObjectiveType,
                        Character = i.PlayerId == null ? null : replayCharacterDetails[i.PlayerId.Value].Character,
                    }).Select(
                    i => new Tmp1
                    {
                        Team = i.ReplayTeamObjective.IsWinner != 0,
                        TimeSpan = i.ReplayTeamObjective.TimeSpan,
                        PlayerName = i.ReplayTeamObjective.PlayerId.HasValue
                            ? replayCharacterDetails[i.ReplayTeamObjective.PlayerId.Value].PlayerName
                            : null,
                        Character = i.Character,
                        CharacterURL = i.Character == null ? null : GetLocalizedString("GenericHero", i.Character),
                        HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                        TeamObjectiveType = i.TeamObjectiveType.GetTeamObjectiveTypeString(),
                        Value = i.TeamObjectiveType.GetTeamObjectiveValueString(i.ReplayTeamObjective.Value),
                    }).ToArray();
                Tmp1[] b =
                {
                    new()
                    {
                        Team = true,
                        TimeSpan = queryContainerIsValidReplay.ReplayLength,
                        PlayerName = null,
                        Character = null,
                        CharacterURL = null,
                        HeroPortraitURL = null,
                        TeamObjectiveType = "Match End",
                        Value = null,
                    },
                };


                var teamObjs = a.Union(infernalEvents).Union(b).OrderBy(i => i.TimeSpan).ToArray();
                res.TeamObjectives = teamObjs;
            }
            //else
            //{
            //    PanelTeamObjectives.Visible = false;
            //}
        }

        Internal();

        return res;
    }

    private static bool GetIsTeam1Winner(int replayId)
    {
        return new Random(replayId).NextDouble() > 0.5d;
    }

    private AwardRow GetAwardName(bool isWinner, int award)
    {
        var team = isWinner ? 0 : 1;
        var (code, text) = ((MatchAwardType)award).GetAwardInfo(team);
        var rc = new AwardRow
        {
            Code = code,
            Text = text,
        };
        return rc;
    }

    private string GetLocalizedString(string keyPrefix, string value)
    {
        // Duplicate code from Master page inaccessible from here
        return LocalizedText.ResourceManager.GetString(keyPrefix + value.PrepareForImageURL()) ?? value;
    }

    private List<ReplayCharacterUpgradeEventReplayLengthPercent> GetReplayCharacterUpgradeEventReplayLengthPercent(
        HeroesdataContext dc,
        int replayId)
    {
        var q = dc.ReplayCharacterUpgradeEventReplayLengthPercents.Where(r => r.ReplayId == replayId);

        return q.ToList();
    }

    private List<ReplayPeriodicXpBreakdown> GetReplayPeriodicXpBreakdown(
        HeroesdataContext dc,
        int replayId)
    {
        return dc.ReplayPeriodicXpBreakdowns.Where(r => r.ReplayId == replayId).ToList();
    }

    private List<ReplayTeamHeroBan> GetReplayTeamHeroBan(
        HeroesdataContext dc,
        int replayId)
    {
        var q = dc.ReplayTeamHeroBans.Where(r => r.ReplayId == replayId);

        return q.ToList();
    }

    private List<ReplayTeamObjective> GetReplayTeamObjective(
        HeroesdataContext dc,
        int replayId)
    {
        return dc.ReplayTeamObjectives.Where(r => r.ReplayId == replayId).ToList();
    }
}
