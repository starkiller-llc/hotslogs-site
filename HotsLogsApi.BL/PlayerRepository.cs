using Heroes.DataAccessLayer.Data;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using ServiceStackReplacement;
using System.Threading.Tasks;

namespace HotsLogsApi.BL;

public class PlayerRepository
{
    private readonly HeroesdataContext _dc;
    private readonly MyDbWrapper _redis;

    public PlayerRepository(HeroesdataContext dc, MyDbWrapper redis)
    {
        _dc = dc;
        _redis = redis;
    }

    public async Task<PlayerProfileSlim> GetProfile(int playerId)
    {
        var dbPlayer = await _dc.Players.SingleOrDefaultAsync(r => r.PlayerId == playerId);
        if (dbPlayer == null)
        {
            return null;
        }

        var rc = new PlayerProfileSlim
        {
            Id = dbPlayer.PlayerId,
            Name = dbPlayer.Name,
            Region = dbPlayer.BattleNetRegionId,
            BattleTag = dbPlayer.BattleTag,
        };

        return rc;
    }
}
