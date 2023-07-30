using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class EventsController : ControllerBase
{
    private readonly HeroesdataContext _dc;
    private readonly EventHelper _helper;

    public EventsController(HeroesdataContext dc, EventHelper helper)
    {
        this._dc = dc;
        _helper = helper;
    }

    [HttpGet]
    public object GetData()
    {
        return _dc.Events.Where(i => i.IsEnabled != 0).Select(
            i => new
            {
                i.EventId,
                i.EventIdparent,
                i.EventName,
                i.EventOrder,
                i.EventGamesPlayed,
            }).ToArray();
    }

    [HttpGet("{id}")]
    public object GetDataByString(int id)
    {
        var eventEntity = _dc.Events.SingleOrDefault(i => i.EventId == id && i.IsEnabled != 0);

        if (eventEntity == null)
        {
            return null;
        }

        var eventProfile = _helper.GetEventProfile(eventEntity);

        foreach (var player in eventProfile.Players.Values)
        {
            player.BT = null;
            player.IsLOO = null;
        }

        return eventProfile;
    }
}
