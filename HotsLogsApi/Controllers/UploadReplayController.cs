using HelperCore;
using Heroes.DataAccessLayer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace HotsLogsApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UploadReplayController : ControllerBase
{
    private readonly HeroesdataContext _dc;
    private readonly ILogger<UploadReplayController> _logger;

    public UploadReplayController(HeroesdataContext dc, ILogger<UploadReplayController> logger)
    {
        _dc = dc;
        _logger = logger;
    }

    [HttpPost]
    [DisableRequestSizeLimit]
    public IActionResult UploadReplay()
    {
        try
        {
            var file = Request.Form.Files[0];
            var pathToSave = Path.Combine("Resources", "Replays");
            var AVG_SIZE = 1000000;
            var MAX_SIZE = AVG_SIZE * 4;

            if (file.Length > 0 && file.Length <= MAX_SIZE)
            {
                var fileName = Guid.NewGuid() + ".StormReplay";
                var fullPath = Path.Combine(pathToSave, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

                var localizationAliases = _dc.LocalizationAliases.Where(
                    i => i.Type == (int)DataHelper.LocalizationAliasType.Hero ||
                         i.Type == (int)DataHelper.LocalizationAliasType.Map).ToArray();

                var (parseResult, replayGuid) = DataHelper.AddReplay(
                    localizationAliases,
                    fullPath,
                    deleteFile: true,
                    ipAddress: ipAddress,
                    eventId: null,
                    logFunction: x => _logger.LogInformation($"{x}"));
                return Ok(replayGuid);
            }

            return BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex}");
        }
    }
}
