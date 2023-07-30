using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL;
using HotsLogsApi.Models;
using Microsoft.AspNetCore.Mvc;
using ServiceStackReplacement;
using System.Threading.Tasks;

namespace HotsLogsApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class PlayerController : ControllerBase
{
    private readonly HeroesdataContext _dc;
    private readonly PlayerRepository _playerRepository;
    private readonly MyDbWrapper _redis;

    public PlayerController(HeroesdataContext dc, MyDbWrapper redis, PlayerRepository playerRepository)
    {
        _dc = dc;
        _redis = redis;
        _playerRepository = playerRepository;
    }

    [HttpGet]
    public async Task<PlayerProfileSlim> GetProfile(int playerId)
    {
        var profile = await _playerRepository.GetProfile(playerId);
        return profile;
    }
}
