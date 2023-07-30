using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using HelperCore;
using Heroes.DataAccessLayer.Data;
using Heroes.ReplayParser;
using HotsLogsApi.Auth;
using HotsLogsApi.BL.Migration;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.MigrationControllers;
using HotsLogsApi.OldControllers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class MatchDetailsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HeroesdataContext _dc;
    private readonly AmazonS3Client _s3Client;
    private readonly ReplayCharacterHelper _rch;

    public MatchDetailsController(
        UserManager<ApplicationUser> userManager,
        HeroesdataContext dc,
        AmazonS3Client s3Client,
        ReplayCharacterHelper rch)
    {
        _userManager = userManager;
        _dc = dc;
        _s3Client = s3Client;
        _rch = rch;
    }

    [HttpGet]
    public ActionResult Get([FromQuery] int replayId)
    {
#if !LOCALDEBUG
        var identity = User.Identity;
        if (!identity.IsAuthenticated)
        {
            return Forbid();
            //return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Must be logged in");
        }
#endif

#if !LOCALDEBUG
        var userId = _userManager.GetUserId(User);
        var user = _dc.Net48Users
            .Include(x => x.Player)
            .Single(i => i.Id.ToString() == userId);

        var isAuthorized = true;
#endif
        var replay = _dc.Replays
            .Include(x => x.ReplayCharacters)
            .ThenInclude(x => x.Player)
            .Include(x => x.ReplayShares)
            .SingleOrDefault(i => i.ReplayId == replayId);

        if (replay == null)
        {
            return NotFound();
        }

#if !LOCALDEBUG
        if (DataHelper.GameModeWithStatistics.All(i => i != replay.GameMode) && replay.GameMode < 1000 &&
            !replay.ReplayShares.Any())
        {
            // If this is a Custom game, make sure the user played in it, or make sure it's been shared

            // Make sure they verified their account
            if (user.IsBattleNetOauthAuthorized == 0ul)
            {
                isAuthorized = false;
            }
            else
            {
                var player = user.Player;

                if (player == null || replay.ReplayCharacters.All(i => i.Player != player))
                {
                    isAuthorized = false;
                }
            }
        }
        else if (replay.GameMode > 1000)
        {
            // If this is an Event replay, let's make sure the Event is public
            var eventEntity = _dc.Events.SingleOrDefault(i => i.EventId == replay.GameMode);

            if (eventEntity == null || eventEntity.IsEnabled == 0)
            {
                isAuthorized = false;
            }
        }

        if (!isAuthorized)
        {
            return Forbid();
            //return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Not authorized");
        }
#endif

        // User is authorized to view this replay
        var single = _dc.Replays.Single(i => i.ReplayId == replayId);
        using var getObjectResponse = _s3Client.GetObject(
            new GetObjectRequest
            {
                BucketName = "heroesreplays",
                Key = single.ReplayHash.ToGuid().ToString(),
            });
        if (getObjectResponse?.ResponseStream is null)
        {
            return Problem(
                detail: "Replay file not found, replays files are only kept for 30 days.",
                statusCode: (int)HttpStatusCode.NotFound);
            //return Request.CreateErrorResponse(
            //    HttpStatusCode.NotFound,
            //    "Replay file not found, replays files are only kept for 30 days.");
        }

        var (replayParseResult, replayData) = DataParser.ParseReplay(
            getObjectResponse.ResponseStream.ReadFully(),
            allowPTRRegion: true);

        if (replayParseResult != DataParser.ReplayParseResult.Success)
        {
            return BadRequest();
            //return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Replay couldn't be parsed");
        }

        var players = replayData.Players;
        var gameMode = replayData.GameMode;
        var teamObjectives = replayData.TeamObjectives;
        var xpBrkDn = replayData.TeamPeriodicXPBreakdown;

        // Change Map and Hero Name to English Version
        var map = GetPrimaryName(replayData.Map, DataHelper.LocalizationAliasType.Map);
        foreach (var player in players)
        {
            player.Character = GetPrimaryName(player.Character, DataHelper.LocalizationAliasType.Hero);
        }

        // New Match Log details
        var permanentHeroes = players
            .SelectMany(
                i =>
                    i.HeroUnits.Where(
                        j =>
                            j.TimeSpanDied.HasValue &&
                            j.PlayerControlledBy != null &&
                            (!j.Name.StartsWith("HeroChen") || j.Name == "HeroChen") &&
                            (j.PlayerControlledBy.Character != "Abathur" || j.Name == "HeroAbathur")));

        var heroesWhoDied =
            gameMode == GameMode.Brawl &&
            map == "Escape From Braxis"

                // 'Escape From Braxis' PvE Brawl has CPU enemies who play multiple different heroes
                ? permanentHeroes.Select(CreateTempHeroForEscapeFromBraxis)

                // Normal Matches
                : permanentHeroes.Select(CreateTempHeroForNormalMap);

        var heroDeathData = heroesWhoDied
            .GroupBy(
                i => new
                {
                    i.TeamMultiplier,
                    i.TimeSpanTotalSecondsStacked,
                })
            .Select(
                i =>

                    // Get list of death times per hero, and sort it.
                    i.OrderBy(j => j.TimeSpanTotalSeconds).ToArray())

            // Sort all lists by the first time of death of the hero.
            .OrderBy(i => i.First().TimeSpanTotalSeconds)
            .ToArray();

        var teamObjectivesWithTeam = teamObjectives[0]
            .Select(i => (Team: 0, TeamObjective: i))
            .Concat(teamObjectives[1].Select(i => (Team: 1, TeamObjective: i)))
            .Where(
                i =>
                {
                    var t = i.TeamObjective.TeamObjectiveType;
                    return (gameMode != GameMode.Brawl ||
                            map != "Escape From Braxis" ||
                            t != TeamObjectiveType.BraxisHoldoutZergRushWithLosingZergStrength) &&
                           t != TeamObjectiveType.InfernalShrinesPunisherKilledWithPunisherType &&
                           t != TeamObjectiveType.InfernalShrinesPunisherKilledWithSiegeDamageDone &&
                           t != TeamObjectiveType.InfernalShrinesPunisherKilledWithHeroDamageDone;
                })
            .GroupBy(i => (int)i.TeamObjective.TimeSpan.TotalSeconds / 30 * 30)
            .Select(i => i.ToArray())
            .OrderBy(i => i.First().TeamObjective.TimeSpan).ToArray();

        var xpDifferenceList = new List<(int Duration, int XpDiff)> { (0, 0) };

        var xpb0 = xpBrkDn[0];
        var xpb1 = xpBrkDn[1];
        if (xpBrkDn.Count(i => i != null) == 2 && xpb0.Count == xpb1.Count)
        {
            for (var i = 0; i < xpb0.Count; i++)
            {
                xpDifferenceList.Add(((int)xpb0[i].TimeSpan.TotalSeconds, xpb0[i].TotalXP - xpb1[i].TotalXP));
            }
        }

        var maxXpDifference = (xpDifferenceList.Max(i => Math.Abs(i.XpDiff)) / 1000 * 1000) + 1000;
        if (maxXpDifference < 5000)
        {
            maxXpDifference = 5000;
        }

        var jsonData = new MatchLogData { MaxXpDifference = maxXpDifference };

        foreach (var distinctHero in players
                     .Select(i => i.Character.PrepareForImageURL())
                     .Union(
                         players.SelectMany(
                             i =>
                                 i.HeroUnits.Select(
                                     j =>
                                         j.Name.Replace("Hero", string.Empty).PrepareForImageURL())))
                     .Distinct())
        {
            jsonData.Heroes.Add(distinctHero);
        }

        var objs = teamObjectivesWithTeam
            .SelectMany(i => i)
            .Select(i => (i.Team, i.TeamObjective.TeamObjectiveType))
            .Distinct();

        foreach (var (team, objType) in objs)
        {
            var v1 = objType.ToString().PrepareForImageURL() + team;
            var v2 = objType.GetTeamObjectiveImageString(team);
            jsonData.TeamObjectiveNames.Add(v1);
            jsonData.TeamObjectiveImages.Add(v2);
        }

        foreach (var teamObjectivesWithTeamGroup in teamObjectivesWithTeam)
        {
            var currentYValue = -0.125 * maxXpDifference * (teamObjectivesWithTeamGroup.Length - 1);

            foreach (var teamObjectiveWithTeam in teamObjectivesWithTeamGroup)
            {
                var xPt = (int)teamObjectiveWithTeam.TeamObjective.TimeSpan.TotalSeconds / 30 * 30;
                jsonData.XValues.Add(xPt);
                jsonData.YValues.Add(currentYValue);
                currentYValue += 0.25 * maxXpDifference;
            }
        }

        var teamObjectiveStyles = teamObjectivesWithTeam
            .SelectMany(i => i)
            .Select(i => i.TeamObjective.TeamObjectiveType.ToString().PrepareForImageURL() + i.Team)
            .ToList();

        jsonData.TeamObjectiveStyles = teamObjectiveStyles;

        var heroDeathStyles = heroDeathData
            .SelectMany(i => i)
            .Select(i => "hero" + i.ImageName)
            .ToList();

        jsonData.HeroDeathStyles = heroDeathStyles;

        foreach (var heroDeathDataGroup in heroDeathData)
        {
            var currentYValue = -0.92 * maxXpDifference * heroDeathDataGroup[0].TeamMultiplier;

            foreach (var heroDeathDataItem in heroDeathDataGroup)
            {
                var xPt = heroDeathDataItem.TimeSpanTotalSecondsStacked;
                jsonData.XHeroDeaths.Add(xPt);
                jsonData.YHeroDeaths.Add(currentYValue);
                currentYValue += 0.175 * maxXpDifference * heroDeathDataItem.TeamMultiplier;
            }
        }

        var eventLabelData = teamObjectivesWithTeam.SelectMany(i => i).Select(
            i =>
            {
                var t = i.TeamObjective;
                var tt = t.TeamObjectiveType;
                var ttt = tt.GetTeamObjectiveTypeString();
                var ttv = tt.GetTeamObjectiveValueString(t.Value);
                return $"{ttt}{(ttv != string.Empty ? ": " + ttv : null)}";
            }).ToList();

        jsonData.EventLabels = eventLabelData;

        var matchEventTimerData = teamObjectivesWithTeam
            .SelectMany(i => i)
            .Select(i => (int)i.TeamObjective.TimeSpan.TotalSeconds)
            .ToList();

        jsonData.MatchEventTimers = matchEventTimerData;

        var deathTimerData = heroDeathData
            .SelectMany(i => i)
            .Select(i => i.TimeSpanTotalSeconds)
            .ToList();

        jsonData.DeathTimers = deathTimerData;

        var deathLabelData = heroDeathData
            .SelectMany(i => i)
            .Select(i => i.Tooltip)
            .ToList();

        jsonData.DeathLabels = deathLabelData;
        jsonData.XDiff = xpDifferenceList.Select(x => x.Duration).ToList();
        jsonData.YDiff = xpDifferenceList.Select(x => x.XpDiff).ToList();

        var maxXpDiffTick = xpDifferenceList.Last().Duration + 10;

        jsonData.MaxXpDiffTick = maxXpDiffTick;

        var json = JsonConvert.SerializeObject(jsonData);
        return Content(json, "application/json");
    }

    [HttpGet("Details/{id:int}")]
    public object GetMatchDetailsByID(int id)
    {
        var replay = _dc.Replays
            .Include(x => x.ReplayCharacters)
            .ThenInclude(x => x.Player)
            .ThenInclude(x => x.LeaderboardOptOut)
            .SingleOrDefault(
                i =>
                    i.ReplayId == id && i.ReplayCharacters.All(j => j.Player.LeaderboardOptOut == null));

        if (replay == null || (replay.GameMode != (int)GameMode.QuickMatch &&
                               replay.GameMode != (int)GameMode.HeroLeague &&
                               replay.GameMode != (int)GameMode.UnrankedDraft &&
                               replay.GameMode != (int)GameMode.TeamLeague &&
                               replay.GameMode != (int)GameMode.StormLeague))
        {
            return null;
        }

        var replayCharacterDetails = _rch.GetReplayCharacterDetails(id);

        foreach (var replayCharacterDetail in replayCharacterDetails)
        {
            replayCharacterDetail.BattleNetId = -1;

            if (replayCharacterDetail.ReplayCharacterScoreResult != null)
            {
                replayCharacterDetail.ReplayCharacterScoreResult.ReplayCharacter = null;
            }
        }

        return replayCharacterDetails;
    }

    private static TempHero CreateTempHeroForEscapeFromBraxis(Unit i)
    {
        return new TempHero
        {
            ImageName = i.Name.Replace("Hero", string.Empty).PrepareForImageURL(),
            Tooltip = i.Name.Replace("Hero", string.Empty).AddSpacesToSentence() + " (" +
                      i.PlayerControlledBy.Name + ") killed by " + (i.UnitKilledBy != null
                          ? i.UnitKilledBy.Name.Replace("Hero", string.Empty).AddSpacesToSentence()
                          : i.PlayerKilledBy != null
                              ? i.PlayerKilledBy.Character + " (" + i.PlayerKilledBy.Name + ")"
                              : string.Empty),

            // i.TimeSpanDied is not null, because the caller makes sure of that
            // ReSharper disable once PossibleInvalidOperationException
            TimeSpanTotalSeconds = (int)i.TimeSpanDied.Value.TotalSeconds,
            TimeSpanTotalSecondsStacked = (int)i.TimeSpanDied.Value.TotalSeconds / 45 * 45,
            TeamMultiplier = i.PlayerControlledBy.Team == 0 ? -1 : i.PlayerControlledBy.Team,
        };
    }

    private static TempHero CreateTempHeroForNormalMap(Unit i)
    {
        return new TempHero
        {
            ImageName = i.PlayerControlledBy.Character.PrepareForImageURL(),
            Tooltip = i.PlayerControlledBy.Character + " (" + i.PlayerControlledBy.Name + ") killed by " +
                      (i.PlayerKilledBy != null ? i.PlayerKilledBy.Character + " (" + i.PlayerKilledBy.Name + ")" :
                          i.UnitKilledBy != null ? i.UnitKilledBy.Name : string.Empty),

            // i.TimeSpanDied is not null, because the caller makes sure of that
            // ReSharper disable once PossibleInvalidOperationException
            TimeSpanTotalSeconds = (int)i.TimeSpanDied.Value.TotalSeconds,
            TimeSpanTotalSecondsStacked = (int)i.TimeSpanDied.Value.TotalSeconds / 45 * 45,
            TeamMultiplier = i.PlayerControlledBy.Team == 0 ? -1 : i.PlayerControlledBy.Team,
        };
    }

    private static string GetPrimaryName(string arg, DataHelper.LocalizationAliasType type)
    {
        var argNormalized = arg.Replace(",", string.Empty).ToUpper();
        var localizationAliasesScrubbed = Global.GetLocalizationAlias()
            .Select(
                i => new
                {
                    i.Type,
                    i.PrimaryName,
                    Aliases = i.AliasesCsv?.Split(','),
                })
            .Where(i => i.Type == (int)type)
            .ToList();
        if (localizationAliasesScrubbed.Any(
                i =>
                    i.Aliases.Any(j => j.ToUpper() == argNormalized)))
        {
            return localizationAliasesScrubbed.Single(
                i =>
                    i.Aliases.Any(j => j.ToUpper() == argNormalized)).PrimaryName;
        }

        return arg;
    }
}
