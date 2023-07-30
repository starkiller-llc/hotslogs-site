using HelperCore;
using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL;
using HotsLogsApi.BL.Migration;
using HotsLogsApi.BL.Migration.Default;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.HeroAndMap;
using HotsLogsApi.BL.Migration.HeroRankings;
using HotsLogsApi.BL.Migration.MapObjectives;
using HotsLogsApi.BL.Migration.MatchAwards;
using HotsLogsApi.BL.Migration.MatchHistory;
using HotsLogsApi.BL.Migration.MatchSummary;
using HotsLogsApi.BL.Migration.Models;
using HotsLogsApi.BL.Migration.Overview;
using HotsLogsApi.BL.Migration.PlayerSearch;
using HotsLogsApi.BL.Migration.Profile;
using HotsLogsApi.BL.Migration.Rankings;
using HotsLogsApi.BL.Migration.Reputation;
using HotsLogsApi.BL.Migration.ScoreResults;
using HotsLogsApi.BL.Migration.TalentDetails;
using HotsLogsApi.BL.Migration.TeamCompositions;
using HotsLogsApi.BL.Migration.Vote;
using HotsLogsApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using static Heroes.ReplayParser.DataParser;
using Helper = HotsLogsApi.BL.Migration.HeroAndMap.Helper;

namespace HotsLogsApi.MigrationControllers;

[ApiController]
[Route("mig/[controller]")]
[Migration]
public class DefaultController : ControllerBase
{
    private readonly ILogger<DefaultController> _logger;
    private readonly HeroesdataContext _dc;
    private readonly UserRepository _userRepository;
    private readonly BnetHelper _bnetHelper;
    private readonly PlayerNameDictionary _pnd;
    private readonly UploadHelper _uploadHelper;
    private readonly IServiceProvider _svcp;

    public DefaultController(
        UserRepository userRepository,
        BnetHelper bnetHelper,
        PlayerNameDictionary pnd,
        UploadHelper uploadHelper,
        IServiceProvider svcp,
        ILogger<DefaultController> logger,
        HeroesdataContext dc)
    {
        _userRepository = userRepository;
        _bnetHelper = bnetHelper;
        _pnd = pnd;
        _uploadHelper = uploadHelper;
        _svcp = svcp;
        _logger = logger;
        _dc = dc;
    }

    [HttpPost("bnetid")]
    public async Task<ActionResult> ChooseBnetId(string battleTag, int region)
    {
        AppUser appUser = null;
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            appUser = await _userRepository.GetUser(int.Parse(userId));
            appUser.DefaultGameMode = (int)SiteMaster.UserDefaultGameMode(Request.Cookies["DefaultGameMode"]);
        }

        var (success, code, error) = _bnetHelper.ChooseBnetId(appUser, battleTag, region);

        if (!success)
        {
            var rc = Problem(
                detail: error,
                statusCode: (int)HttpStatusCode.Conflict);

            var pd = (ProblemDetails)rc.Value;

            pd!.Extensions["code"] = code;

            return rc;
        }

        return Ok();
    }

    [HttpGet]
    public ActionResult<DefaultHelperResult> Get([FromQuery] DefaultHelperArgs args)
    {
        var helper = new DefaultHelper(args);
        return helper.MainCalculation();
    }

    [HttpGet("filters")]
    public ActionResult<FilterDataSources> Get()
    {
        var rc = new FilterDataSources(_dc);
        return rc;
    }

    [HttpPost("heroandmap")]
    public ActionResult<HeroAndMapResponse> GetHeroAndMapStatistics(HeroAndMapArgs req)
    {
        var helper = new Helper(req, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("herorankings")]
    public async Task<ActionResult<HeroRankingsResponse>> GetHeroRankings(
        HeroRankingsArgs req)
    {
        AppUser appUser = null;
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            appUser = await _userRepository.GetUser(int.Parse(userId));
            appUser.DefaultGameMode = (int)SiteMaster.UserDefaultGameMode(Request.Cookies["DefaultGameMode"]);
        }

        var helper = new BL.Migration.HeroRankings.Helper(req, appUser, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpGet("lang")]
    public LanguageDescription GetLanguage()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        var code = culture.IetfLanguageTag;
        if (!code.StartsWith("zh"))
        {
            while (!string.IsNullOrWhiteSpace(culture.Parent.IetfLanguageTag))
            {
                culture = culture.Parent;
                code = culture.IetfLanguageTag;
            }
        }

        return new LanguageDescription
        {
            LanguageCode = code,
            Strings = SiteMaster.GetAllStrings(),
        };
    }

    [HttpPost("mapobjectives")]
    public ActionResult<MapObjectivesResponse> GetMapObjectives(MapObjectivesArgs req)
    {
        var helper = new BL.Migration.MapObjectives.Helper(req, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("matchawards")]
    public async Task<ActionResult<MatchAwardsResponse>> GetMatchAwards(
        MatchAwardsArgs req)
    {
        AppUser appUser = null;
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            appUser = await _userRepository.GetUser(int.Parse(userId));
            appUser.DefaultGameMode = (int)SiteMaster.UserDefaultGameMode(Request.Cookies["DefaultGameMode"]);
        }

        var helper = new BL.Migration.MatchAwards.Helper(req, appUser, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("matchhistory")]
    public async Task<ActionResult<MatchHistoryResponse>> GetMatchHistory(
        MatchHistoryArgs req)
    {
        AppUser appUser = null;
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            appUser = await _userRepository.GetUser(int.Parse(userId));
            appUser.DefaultGameMode = (int)SiteMaster.UserDefaultGameMode(Request.Cookies["DefaultGameMode"]);
        }

        using var helper = new BL.Migration.MatchHistory.Helper(req, appUser, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("matchsummary")]
    public async Task<ActionResult<MatchSummaryResponse>> GetMatchSummary(MatchSummaryArgs req)
    {
        AppUser appUser = null;
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            appUser = await _userRepository.GetUser(int.Parse(userId));
            appUser.DefaultGameMode = (int)SiteMaster.UserDefaultGameMode(Request.Cookies["DefaultGameMode"]);
        }

        var helper = new BL.Migration.MatchSummary.Helper(req, appUser, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("overview")]
    public async Task<ActionResult<OverviewResponse>> GetOverview(
        OverviewArgs req)
    {
        AppUser appUser = null;
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            appUser = await _userRepository.GetUser(int.Parse(userId));
            appUser.DefaultGameMode = (int)SiteMaster.UserDefaultGameMode(Request.Cookies["DefaultGameMode"]);
        }

        using var helper = new BL.Migration.Overview.Helper(req, appUser, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("search")]
    public ActionResult<PlayerSearchResponse> GetPlayerSearch(PlayerSearchArgs req)
    {
        var helper = new BL.Migration.PlayerSearch.Helper(req, _pnd, _dc);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("profile")]
    public async Task<ActionResult<ProfileResponse>> GetProfile(
        ProfileArgs req)
    {
        AppUser appUser = null;
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            appUser = await _userRepository.GetUser(int.Parse(userId));
            appUser.DefaultGameMode = (int)SiteMaster.UserDefaultGameMode(Request.Cookies["DefaultGameMode"]);
        }

        if (req.PlayerId is null && appUser is null)
        {
            return Forbid();
        }

        using var scope = _svcp.CreateScope();
        var helper = new BL.Migration.Profile.Helper(req, appUser, scope.ServiceProvider);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("rankings")]
    public async Task<ActionResult<RankingsResponse>> GetRankings(
        RankingsArgs req)
    {
        AppUser appUser = null;
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            appUser = await _userRepository.GetUser(int.Parse(userId));
            appUser.DefaultGameMode = (int)SiteMaster.UserDefaultGameMode(Request.Cookies["DefaultGameMode"]);
        }

        var helper = new BL.Migration.Rankings.Helper(req, appUser, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("reputation")]
    public ActionResult<ReputationResponse> GetReputation(ReputationArgs req)
    {
        var helper = new BL.Migration.Reputation.Helper(req, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("scoreresults")]
    public ActionResult<ScoreResultsResponse> GetScoreResults(ScoreResultsArgs req)
    {
        var helper = new BL.Migration.ScoreResults.Helper(req, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("talentdetails")]
    public ActionResult<TalentDetailsResponse> GetTalentDetails(TalentDetailsArgs req)
    {
        var helper = new BL.Migration.TalentDetails.Helper(req, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("teamcompositions")]
    public ActionResult<TeamCompositionsResponse> GetTeamCompositions(TeamCompositionsArgs req)
    {
        var helper = new BL.Migration.TeamCompositions.Helper(req, _svcp);
        var res = helper.MainCalculation();
        return res;
    }

    [HttpPost("lang/{lang}")]
    public LanguageDescription SetLanguage(string lang)
    {
        Response.Cookies.Append("CultureInfo", lang, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
        var cult = new CultureInfo(lang);
        Thread.CurrentThread.CurrentCulture = cult;
        Thread.CurrentThread.CurrentUICulture = cult;
        return new LanguageDescription
        {
            LanguageCode = Thread.CurrentThread.CurrentCulture.IetfLanguageTag,
            Strings = SiteMaster.GetAllStrings(),
        };
    }

    [HttpPost("upload")]
    public async Task<ActionResult<UploadResult>> Upload(List<IFormFile> file, [FromForm] int? eventId)
    {
        if (eventId.HasValue)
        {
            AppUser appUser = null;
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                appUser = await _userRepository.GetUser(int.Parse(userId));
            }

            if (appUser is not { IsAdmin: true })
            {
                return Forbid();
            }
        }

        var basePath = "upload";
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        ReplayParseResult? parseResult = null;
        Exception exception = null;
        foreach (var f in file)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "<null>";
            var stream = f.OpenReadStream();
            (parseResult, exception, _) = await _uploadHelper.AddReplay(stream, ip, "Web", eventId);
        }

        var status = parseResult == ReplayParseResult.Success
            ? "Ok"
            : "Error";
        var rc = new UploadResult
        {
            Status = status,
            Result = parseResult?.ToString(),
            Exception = exception,
        };
        return Ok(rc);
    }

    [HttpPost("vote")]
    public async Task<ActionResult<VoteResponse>> Vote(VoteArgs req)
    {
        AppUser appUser = null;
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            appUser = await _userRepository.GetUser(int.Parse(userId));
            appUser.DefaultGameMode = (int)SiteMaster.UserDefaultGameMode(Request.Cookies["DefaultGameMode"]);
        }

        var helper = new VoteHelper(req, appUser, _dc);
        var res = helper.Vote();
        return res;
    }
}
