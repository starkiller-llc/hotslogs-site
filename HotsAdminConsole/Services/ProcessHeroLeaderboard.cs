using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Hero Leaderboard", Sort = 8, AutoStart = true, KeepRunning = true, Port = 17010)]
public class ProcessHeroLeaderboard : ServiceBase
{
    private const string HeroLeaderboardQuery =
        @"select
            q.PlayerID,
            q.`Name`,
            q.CurrentMMR,
            count(*) as GamesPlayedAsHero,
            sum(rc.IsWinner) / count(*) as WinPercentAsHero,
            rankingb(q.CurrentMMR,sum(rc.IsWinner),count(*), @gamesFactor, @magicFactor) as RankingScore,
            n48u.PremiumSupporterSince
            from (
                select pa.PlayerID, p.`Name`, lr.MMRRating CurrentMMR
                from PlayerAggregate pa
                join playermmrmilestonev3 lr on lr.PlayerID=pa.playerid and lr.GameMode=pa.gamemode and lr.MilestoneDate=
                (
                    select max(milestonedate) 
                    from playermmrmilestonev3 
                    where playerid=pa.playerid and gamemode=pa.gamemode and milestonedate>=@dateStart and milestonedate<@dateEnd
                )
                join Player p on p.PlayerID = pa.PlayerID
                where pa.GameMode = @gameMode
                and p.BattleNetRegionId = @regionId
            ) q
            left join net48_users n48u on n48u.playerid = q.playerid
            join ReplayCharacter rc on rc.PlayerID = q.PlayerID and rc.CharacterID = @characterId
            join Replay r on r.ReplayID = rc.ReplayID and r.GameMode = @gameMode
            where r.TimestampReplay >= @dateStart and r.TimestampReplay < @dateEnd
            group by q.PlayerID, q.`Name`, q.CurrentMMR, n48u.PremiumSupporterSince
            having RankingScore > 0
            order by RankingScore desc
            limit 100";

    private readonly DateTime _march2019 = new(2019, 3, 1);
    private CancellationToken _token;

    protected override async Task RunOnce(CancellationToken token = default)
    {
        _token = token;
        await using var scope = Svcp.CreateAsyncScope();

        var heroesEntity = HeroesdataContext.Create(scope);
        var heroIds = heroesEntity.LocalizationAliases
            .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero).ToArray();
        var seasons1 = heroesEntity.PlayerMmrResets
            .Where(x => x.ResetDate > _march2019)
            .OrderBy(x => x.ResetDate)
            .ToList();
        seasons1.Add(
            new PlayerMmrReset
            {
                Title = "All Seasons",
                ResetDate = DateTime.MaxValue,
            });
        seasons1.Add(new PlayerMmrReset { Title = "Terminator" });
        var seasons = seasons1
            .Zip(seasons1.Skip(1), (start, end) => (start, end))
            .Reverse().ToList();

        var init = false;
        State state = null;

        Restore();

        await using var mySqlConnection = new MySqlConnection(ConnectionString);
        await mySqlConnection.OpenAsync(token);

        foreach (var regionId in DataHelper.BattleNetRegionNames.Keys)
        {
            if (init && regionId != state.RegionId)
            {
                AddServiceOutput($"Skipping region {regionId}");
                continue;
            }

            foreach (var hero in heroIds)
            {
                if (init && hero.IdentifierId != state.HeroId)
                {
                    AddServiceOutput($"Skipping {hero.IdentifierId} {hero.PrimaryName}");
                    continue;
                }

                init = false;
                AddServiceOutput($"Processing {hero.IdentifierId} region {regionId} {hero.PrimaryName}");
                foreach (var (start, end) in seasons)
                {
                    await RetryIfTmpTableFull(ReadFromSql);

                    async Task ReadFromSql()
                    {
                        await DoOneAsync(scope, mySqlConnection, hero, start, end, regionId, token);
                        Save(regionId, hero.IdentifierId);
                    }
                }
            }
        }

        ClearState(scope);

        void Save(int regionId, int heroId)
        {
            SaveState(
                scope,
                new State
                {
                    RegionId = regionId,
                    HeroId = heroId,
                });
        }

        void Restore()
        {
            try
            {
                state = RestoreState<State>(scope);
                init = true;
            }
            catch
            {
                /* ignored */
            }
        }
    }

    private async Task DoOneAsync(
        IServiceScope scope,
        MySqlConnection mySqlConnection,
        LocalizationAlias hero,
        PlayerMmrReset start,
        PlayerMmrReset end,
        int regionId,
        CancellationToken token)
    {
        AddServiceOutput($"...{start.Title}");
        var allSeasons = start.Title == "All Seasons";
        var dateStart = allSeasons ? DateTime.MinValue : start.ResetDate;
        var dateEnd = allSeasons ? DateTime.MaxValue : end.ResetDate;

        var gamesFactor = allSeasons ? 50.0 : 20.0;

        int[] gameModes = { 3, 6, 8 };

        foreach (var gameMode in gameModes)
        {
            var sitewideHeroLeaderboardStatisticsList =
                new List<SitewideHeroLeaderboardStatistic>();
            var redisKey =
                "HOTSLogs:SitewideHeroLeaderboardStatisticsV2" +
                $":{regionId}" +
                $":{hero.IdentifierId}" +
                $":{start.Title}" +
                $":{gameMode}";
            await using (var mySqlCommand =
                         new MySqlCommand(HeroLeaderboardQuery, mySqlConnection)
                         {
                             CommandTimeout = MMR.LongCommandTimeout,
                         })
            {
                mySqlCommand.Parameters.AddWithValue("@regionId", regionId);
                mySqlCommand.Parameters.AddWithValue("@characterId", hero.IdentifierId);
                mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                mySqlCommand.Parameters.AddWithValue("@dateStart", dateStart);
                mySqlCommand.Parameters.AddWithValue("@dateEnd", dateEnd);
                mySqlCommand.Parameters.AddWithValue("@gamesFactor", gamesFactor);
                mySqlCommand.Parameters.AddWithValue("@magicFactor", 1 / gamesFactor);

                await using var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token);
                while (await mySqlDataReader.ReadAsync(token))
                {
                    var entry = new SitewideHeroLeaderboardStatistic
                    {
                        LR = sitewideHeroLeaderboardStatisticsList.Count + 1,
                        N = mySqlDataReader.GetString("Name"),
                        GP = (int)mySqlDataReader.GetInt64("GamesPlayedAsHero"),
                        WP = mySqlDataReader.GetDecimal("WinPercentAsHero"),
                        R = mySqlDataReader.GetInt32("CurrentMMR"),
                        PID = mySqlDataReader.GetInt32("PlayerID"),
                        TSS = !(mySqlDataReader["PremiumSupporterSince"] is DBNull)
                            ? mySqlDataReader.GetDateTime("PremiumSupporterSince")
                            : null,
                    };
                    sitewideHeroLeaderboardStatisticsList.Add(entry);
                }
            }

            // Cache the new leaderboards
            var redisClient = MyDbWrapper.Create(scope);
            redisClient.TrySet(
                redisKey,
                new SitewideHeroLeaderboardStatistics
                {
                    BattleNetRegionId = regionId,
                    CharacterID = hero.IdentifierId,
                    LastUpdated = DateTime.UtcNow,
                    SitewideHeroLeaderboardStatisticArray =
                        sitewideHeroLeaderboardStatisticsList.ToArray(),
                },
                TimeSpan.FromDays(30));
        }
    }

    private void LogTmpTableFails()
    {
        AddServiceOutput("Retrying due to MySql tmp table full error.");
    }

    private async Task RetryIfTmpTableFull(Func<Task> act)
    {
        await TmpTableFullRetryHandler.RetryIfTmpTableFull(act, LogTmpTableFails, _token);
    }

    private void RetryIfTmpTableFull(Action act)
    {
        TmpTableFullRetryHandler.RetryIfTmpTableFull(act, LogTmpTableFails, _token);
    }

    public class State
    {
        public int RegionId { get; set; }
        public int HeroId { get; set; }
    }

    public ProcessHeroLeaderboard(IServiceProvider svcp) : base(svcp) { }
}
