using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Shared Replays", Sort = 2, Port = 17016)]
public class RedisSharedReplays : ServiceBase
{
    public RedisSharedReplays(IServiceProvider svcp) : base(svcp) { }

    protected override Task RunOnce(CancellationToken token = default)
    {
        using var scope = Svcp.CreateScope();
        var heroesEntity = HeroesdataContext.Create(scope);
        heroesEntity.Database.SetCommandTimeout(MMR.LongCommandTimeout);

        var locDic = heroesEntity.LocalizationAliases.ToDictionary(i => i.IdentifierId, i => i.PrimaryName);

        var last60Days = DateTime.UtcNow.AddDays(-60);

        var replayShares = heroesEntity.ReplayShares
            .Include(x => x.Replay)
            .ThenInclude(x => x.ReplayCharacters)
            .Include(x => x.PlayerIdsharedByNavigation)
            .Where(
                i => i.Replay.TimestampReplay > last60Days &&
                     i.Replay.ReplayCharacters.All(j => j.Player.LeaderboardOptOut == null))
            .ToList()
            .Select(
                i => new
                {
                    i.PlayerIdsharedByNavigation.BattleNetRegionId,
                    i.ReplayShareId,
                    i.Replay.ReplayId,
                    i.UpvoteScore,
                    i.Replay.GameMode,
                    i.Title,
                    i.Replay.MapId,
                    i.Replay.ReplayLength,
                    Characters = string.Join(
                        ",",
                        i.Replay.ReplayCharacters.Select(j => j.CharacterId).ToArray().Select(j => locDic[j])
                            .OrderBy(k => k)),
                    AverageCharacterLevel = i.Replay.ReplayCharacters.Any(j => j.IsAutoSelect == 0)
                        ? i.Replay.ReplayCharacters.Where(j => j.IsAutoSelect == 0).Average(j => j.CharacterLevel)
                        : -1,
                    AverageMMR = i.Replay.ReplayCharacters.Average(j => j.Mmrbefore),
                    i.Replay.TimestampReplay,
                })
            .ToArray();

        var redisClient = MyDbWrapper.Create(scope);
        foreach (var battleNetRegionId in DataHelper.BattleNetRegionNames.Keys)
        {
            redisClient.TrySet(
                "HOTSLogs:ReplaySharesV2:" + battleNetRegionId,
                new ReplayShares
                {
                    ReplaySharesList = replayShares.Where(i => i.BattleNetRegionId == battleNetRegionId).Select(
                        i => new ReplaySharePOCO
                        {
                            ReplayShareID = i.ReplayShareId,
                            ReplayID = i.ReplayId,
                            UpvoteScore = i.UpvoteScore,
                            GameMode = i.GameMode,
                            Title = i.Title,
                            Map = locDic[i.MapId],
                            ReplayLength = i.ReplayLength,
                            ReplayLengthMinutes = TimeSpan.FromMinutes(i.ReplayLength.Minutes),
                            Characters = i.Characters,
                            AverageCharacterLevel = i.AverageCharacterLevel,
                            AverageMMR = i.AverageMMR,
                            TimestampReplay = i.TimestampReplay,
                            TimestampReplayDate = i.TimestampReplay.Date,
                        }).ToArray(),
                    BattleNetRegionId = battleNetRegionId,
                    LastUpdated = DateTime.UtcNow,
                },
                TimeSpan.FromDays(45));
        }

        return Task.CompletedTask;
    }
}
