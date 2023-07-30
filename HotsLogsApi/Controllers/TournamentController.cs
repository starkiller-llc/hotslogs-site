using Heroes.DataAccessLayer.Data;
using HotsLogsApi.Auth.IsAdmin;
using HotsLogsApi.BL;
using HotsLogsApi.Models;
using Microsoft.AspNetCore.Mvc;
using ServiceStackReplacement;
using System.Threading.Tasks;

namespace HotsLogsApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[IsAdmin]
public class TournamentController : ControllerBase
{
    private readonly HeroesdataContext _dc;
    private readonly MyDbWrapper _redis;
    private readonly TournamentRepository _tournamentRepository;

    public TournamentController(HeroesdataContext dc, MyDbWrapper redis, TournamentRepository tournamentRepository)
    {
        _dc = dc;
        _redis = redis;
        _tournamentRepository = tournamentRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTournament([FromBody] Tournament t)
    {
        try
        {
            var result = await _tournamentRepository.CreateTournament(t);
            return result == 0 ? Ok() : Content("Error");
        }
        catch
        {
            return Content("Error");
        }
    }

    [HttpGet]
    public async Task<int> CreateTournamentMatches()
    {
        var tournament = await _tournamentRepository.CreateTournamentMatches();
        return 1;
    }

    [HttpGet]
    public async Task<TournamentMatch[]> GetMatchesForTournament(int TournamentId)
    {
        var matches = await _tournamentRepository.GetMatchesForTournament(TournamentId);
        return matches;
    }

    [HttpGet]
    public async Task<PlayerTournamentMatch[]> GetPlayerTournaments(int playerId)
    {
        var matches = await _tournamentRepository.GetPlayerTournaments(playerId);
        return matches;
    }

    [HttpGet]
    public async Task<Tournament> GetTournamentById(int TournamentId)
    {
        var tournament = await _tournamentRepository.GetTournamentById(TournamentId);
        return tournament;
    }

    [HttpGet]
    public async Task<Tournament[]> GetTournaments()
    {
        var tournaments = await _tournamentRepository.GetTournaments();
        return tournaments;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterForTournament([FromBody] TournamentRegistrationApplication r)
    {
        try
        {
            var result = await _tournamentRepository.RegisterForTournament(r);
            return result == 0 ? Ok() : Content("Error");
        }
        catch
        {
            return Content("Error");
        }
    }

    [HttpGet]
    public async Task<TournamentMatch> SetMatchWinner(int MatchId, int WinningTeamId)
    {
        var match = await _tournamentRepository.SetMatchWinner(MatchId, WinningTeamId);
        return match;
    }

    [HttpGet]
    public async Task<TournamentMatch> SetReplayId(int MatchId, string ReplayGuid)
    {
        var match = await _tournamentRepository.SetReplayId(MatchId, ReplayGuid);
        return match;
    }

    [HttpGet]
    public async Task<int> UpdateTournamenMatchDeadline(int TournamentId)
    {
        return await _tournamentRepository.UpdateTournamenMatchDeadline(TournamentId);
    }

    [HttpGet]
    public async Task<int> UpdateTournamentRegistrationDeadline(int TournamentId)
    {
        return await _tournamentRepository.UpdateTournamentRegistrationDeadline(TournamentId);
    }
}
