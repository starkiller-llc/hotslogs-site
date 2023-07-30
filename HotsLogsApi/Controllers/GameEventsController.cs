using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL;
using HotsLogsApi.Models;
using Microsoft.AspNetCore.Mvc;
using ServiceStackReplacement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotsLogsApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class GameEventsController : ControllerBase
{
    private readonly HeroesdataContext _dc;
    private readonly GameEventsRepository _gameEventsRepository;
    private readonly MyDbWrapper _redis;

    public GameEventsController(HeroesdataContext dc, MyDbWrapper redis, GameEventsRepository gameEventsRepository)
    {
        _dc = dc;
        _redis = redis;
        _gameEventsRepository = gameEventsRepository;
    }

    [HttpPost]
    public async Task AssociateGameEventPlayer(int eventId, int playerId, string name)
    {
        await _gameEventsRepository.AssociateGameEventPlayer(eventId, playerId, name);
    }

    [HttpPost]
    public async Task<GameEventTeam> AssociateGameEventTeam(int replayId, bool winningTeam, int teamId, string teamName)
    {
        return await _gameEventsRepository.AssociateGameEventTeam(replayId, winningTeam, teamId, teamName);
    }

    [HttpGet]
    public async Task<GameEventGamesAndInfo> GetGameEvent(int id)
    {
        var gameEventGamesAndInfo = await _gameEventsRepository.GetGameEvent(id);
        return gameEventGamesAndInfo;
    }

    [HttpGet]
    public async Task<IEnumerable<GameEvent>> GetGameEvents()
    {
        var gameEvents = await _gameEventsRepository.GetGameEvents();
        return gameEvents;
    }
}
