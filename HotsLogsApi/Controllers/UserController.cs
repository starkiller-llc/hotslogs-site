using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL;
using HotsLogsApi.BL.Migration;
using HotsLogsApi.Models;
using Microsoft.AspNetCore.Mvc;
using ServiceStackReplacement;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotsLogsApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly HeroesdataContext _dc;
    private readonly MyDbWrapper _redis;
    private readonly UserRepository _userRepository;

    public UserController(HeroesdataContext dc, MyDbWrapper redis, UserRepository userRepository)
    {
        _dc = dc;
        _redis = redis;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<AppUser> GetUser()
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return null;
        }

        var appUser = await _userRepository.GetUser(int.Parse(userId));
        appUser.DefaultGameMode = (int)SiteMaster.UserDefaultGameMode(Request.Cookies["DefaultGameMode"]);
        return appUser;
    }
}
