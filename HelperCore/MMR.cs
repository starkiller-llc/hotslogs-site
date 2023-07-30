// Copyright (c) StarKiller LLC. All rights reserved.

using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moserware.Skills;
using MySqlConnector;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Player = Moserware.Skills.Player;

namespace HelperCore;

public class MMR
{
    private readonly IServiceProvider _svcp;
    private readonly string _connectionString;

    public const int LongCommandTimeout = 500000;

#if !LOCALDEBUG
    public DateTime Now => DateTime.UtcNow;
#else
    public DateTime Now { get; } = new(2022, 5, 3, 12, 0, 0, DateTimeKind.Utc);
#endif

    public MMR(IServiceProvider svcp)
    {
        _svcp = svcp;
        var config = svcp.GetRequiredService<IConfiguration>();
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task UpdatePlayersCurrentMMRAndLeagueRankEligibilityAsync(
        int battleNetRegionId,
        int gameMode,
        bool recalculateAggregateForAllPlayers = false,
        bool dryRun = false,
        CancellationToken token = default,
        Action<string> logFunction = null)
    {
        var logFileName = "LeagueCalculation Region " + battleNetRegionId + " GameMode " + gameMode + ".txt";

        const int playersToProcessAtATime = 10000;

        const int daysIncludedInGamesPlayedRecently = 30;

        var events = new List<string>();

        void Log(string msg, bool debug = false)
        {
            if (dryRun)
            {
                msg = $"(dryrun) {msg}";
            }

            DataHelper.LogApplicationEvents(msg, logFileName, debug);
            logFunction?.Invoke(msg);
        }

        var queryLogger = new SqlQueryLogger(Log) { IsActive = false };

        // Calculate Games Played Total and Games Played With MMR for each recently active Player
        Log("Start");
        Log("Calculate Games Played Total and Games Played With MMR for each recently active Player", debug: true);

        var connString = _connectionString;
        await using (var mySqlConnection = new MySqlConnection(connString))
        {
            await mySqlConnection.OpenAsync(token);

            const string activePlayerIdQuery =
                @"select distinct(rc.PlayerID) as PlayerID
                    from Replay r
                    join ReplayCharacter rc on rc.ReplayID = r.ReplayID
                    join Player p on p.PlayerID = rc.PlayerID
                    where p.BattleNetRegionId = @battleNetRegionId
                    and r.GameMode = @gameMode {0}";

            // Gather a list of recently active players to update
            var playerIdList = new List<int>();
            var prm = recalculateAggregateForAllPlayers
                ? null
                : "and r.TimestampReplay > date_add(now(), interval -10 day)";
            var cmdText1 = string.Format(activePlayerIdQuery, prm);
            await using (var mySqlCommand =
                         new MySqlCommand(cmdText1, mySqlConnection) { CommandTimeout = LongCommandTimeout })
            {
                mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);

                queryLogger.LogSqlCommand(mySqlCommand);
                await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
                {
                    while (await mySqlDataReader.ReadAsync(token))
                    {
                        playerIdList.Add(mySqlDataReader.GetInt32("PlayerID"));
                    }
                }
            }

            playerIdList = playerIdList.OrderBy(i => i).ToList();

            Log("Gathered PlayerIDs to Calculate.  Total unique players: " + playerIdList.Count, debug: true);

            const string playerAggregateSelectQuery =
                @"select q.*,
                        (select rc.CharacterID
                            from ReplayCharacter rc
                            where rc.PlayerID = q.PlayerID
                            group by rc.CharacterID
                            order by count(*) desc limit 1
                        ) as FavoriteCharacter
                    from (select rc.PlayerID,
                                 count(*) as GamesPlayedTotal,
                                 count(rc.MMRBefore) as GamesPlayedWithMMR,
                                 sum(case when r.TimestampReplay > date_add(now(), interval -{1} day) then 1 else 0 end) as GamesPlayedRecently
                            from ReplayCharacter rc
                            join Replay r on r.ReplayID = rc.ReplayID
                            where rc.PlayerID in ({0}) and r.GameMode = @gameMode
                            group by rc.PlayerID
                         ) q";

            const string playerAggregateUpdateQueryTemplate =
                @"insert into PlayerAggregate values({0},{1},{2},{3},{4},{5},now()) on duplicate key update GamesPlayedTotal={2},GamesPlayedWithMMR={3},GamesPlayedRecently={4},FavoriteCharacter={5},TimestampLastUpdated=now();";

            // Calculate and update player aggregate data
            for (var i = 0; i < playerIdList.Count; i += playersToProcessAtATime)
            {
                var playerAggregateUpdateQuery = string.Empty;

                var cmdText2 = string.Format(
                    playerAggregateSelectQuery,
                    string.Join(",", playerIdList.Skip(i).Take(playersToProcessAtATime)),
                    daysIncludedInGamesPlayedRecently);
                await using (var mySqlCommand =
                             new MySqlCommand(cmdText2, mySqlConnection) { CommandTimeout = LongCommandTimeout })
                {
                    mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);

                    queryLogger.LogSqlCommand(mySqlCommand);
                    // @query
                    await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
                    {
                        while (await mySqlDataReader.ReadAsync(token))
                        {
                            var obj = new
                            {
                                PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                                GamesPlayedTotal = mySqlDataReader.GetInt64("GamesPlayedTotal"),
                                GamesPlayedWithMMR = mySqlDataReader.GetInt64("GamesPlayedWithMMR"),
                                GamesPlayedRecently = mySqlDataReader.GetDecimal("GamesPlayedRecently"),
                                FavoriteCharacter = mySqlDataReader.GetInt32("FavoriteCharacter"),
                            };

                            playerAggregateUpdateQuery += string.Format(
                                playerAggregateUpdateQueryTemplate,
                                obj.PlayerID,
                                gameMode,
                                obj.GamesPlayedTotal,
                                obj.GamesPlayedWithMMR,
                                (int)obj.GamesPlayedRecently,
                                obj.FavoriteCharacter);
                        }
                    }
                }

                if (!dryRun)
                {
                    await using var mySqlCommand =
                        new MySqlCommand(playerAggregateUpdateQuery, mySqlConnection)
                        {
                            CommandTimeout = LongCommandTimeout,
                        };
                    await mySqlCommand.ExecuteNonQueryAsync(token);
                }

                Log("Aggregated Player " + i + " of " + playerIdList.Count, debug: true);
            }

            if (!dryRun)
            {
                // Set 'GamesPlayedRecently' to 0 if their PlayerAggregate hasn't been calculated in awhile; if the user hasn't played in awhile
                await using var mySqlCommand = new MySqlCommand(
                    "update PlayerAggregate set GamesPlayedRecently = 0 where GameMode = " + gameMode +
                    " and TimestampLastUpdated < date_add(now(), interval -" +
                    daysIncludedInGamesPlayedRecently +
                    " day) and TimestampLastUpdated > date_add(now(), interval -" +
                    (daysIncludedInGamesPlayedRecently + 10) + " day)",
                    mySqlConnection)
                {
                    CommandTimeout = LongCommandTimeout,
                };
                await mySqlCommand.ExecuteNonQueryAsync(token);
            }
        }

        Log("All done aggregating player data", debug: true);

        // Set the user's current Leaderboard Ranking MMR as the most recent milestone
        Log("Set the user's current MMR as the most recent milestone", debug: true);

        await using (var mySqlConnection =
                     new MySqlConnection(_connectionString))
        {
            await mySqlConnection.OpenAsync(token);

            int maxPlayerId;
            const string cmdText3 = "select max(PlayerID) from Player where BattleNetRegionId = @battleNetRegionId";
            await using (var mySqlCommand =
                         new MySqlCommand(cmdText3, mySqlConnection) { CommandTimeout = LongCommandTimeout })
            {
                mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                var scalarResult = await mySqlCommand.ExecuteScalarAsync(token);
                scalarResult = scalarResult == DBNull.Value ? null : scalarResult;
                maxPlayerId = Convert.ToInt32(scalarResult);
            }

            const string playersWithMMRMilestonesQuery =
                @"select outerQ.PlayerID, outerPM.MMRRating
                        from (select pm.PlayerID, max(pm.MilestoneDate) as MaxMilestoneDate
                                from PlayerMMRMilestoneV3 pm
                                join Player p on p.PlayerID = pm.PlayerID
                                where p.PlayerID > @minPlayerID and
                                      p.PlayerID <= @maxPlayerID and
                                      p.BattleNetRegionId = @battleNetRegionId and
                                      pm.GameMode = @gameMode
                                group by pm.PlayerID
                             ) as outerQ
                        join PlayerMMRMilestoneV3 outerPM on outerPM.PlayerID = outerQ.PlayerID and
                                                             outerPM.MilestoneDate = outerQ.MaxMilestoneDate and
                                                             outerPM.GameMode = @gameMode";

            for (var i = 0; i <= maxPlayerId; i += playersToProcessAtATime)
            {
                var currentPlayerIdCap = i + playersToProcessAtATime;

                var playersWithMMRMilestones = new List<(int playerId, int mmrRating)>();
                await using (var mySqlCommand = new MySqlCommand(playersWithMMRMilestonesQuery, mySqlConnection)
                {
                    CommandTimeout = LongCommandTimeout,
                })
                {
                    mySqlCommand.Parameters.AddWithValue("@minPlayerID", i);
                    mySqlCommand.Parameters.AddWithValue("@maxPlayerID", currentPlayerIdCap);
                    mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                    mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);

                    queryLogger.LogSqlCommand(mySqlCommand);
                    await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
                    {
                        while (await mySqlDataReader.ReadAsync(token))
                        {
                            var obj = new
                            {
                                PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                                MMRRating = mySqlDataReader.GetInt32("MMRRating"),
                            };

                            playersWithMMRMilestones.Add((obj.PlayerID, obj.MMRRating));
                        }
                    }
                }

                var sqlQueryUpdate = string.Empty;
                for (var k = 0; k < playersWithMMRMilestones.Count; k++)
                {
                    await ThrowIfCancellationRequested(token);

                    sqlQueryUpdate += "insert into LeaderboardRanking values(" + playersWithMMRMilestones[k].playerId +
                                      "," + gameMode + "," + playersWithMMRMilestones[k].mmrRating +
                                      ",null,null,false) on duplicate key update CurrentMMR=" +
                                      playersWithMMRMilestones[k].mmrRating + ";";

                    if (k % 1000 == 0 || k + 1 == playersWithMMRMilestones.Count)
                    {
                        if (!dryRun)
                        {
                            await using var mySqlCommand =
                                new MySqlCommand(sqlQueryUpdate, mySqlConnection)
                                {
                                    CommandTimeout = LongCommandTimeout,
                                };
                            await mySqlCommand.ExecuteNonQueryAsync(token);
                        }

                        sqlQueryUpdate = string.Empty;
                        Log(
                            "Calculating Current Player MMR, Overall Progress: " + i + " / " + maxPlayerId +
                            ", Current Progress: " + (k + 1) + " / " + playersWithMMRMilestones.Count, debug: true);
                    }
                }
            }
        }

        Log("All Done Calculating Current Player MMR", debug: true);

        var leagueRequiredGames = new List<int>();
        int minRequiredGames;

        // Require a certain number of games played recently
        const int minRequiredGamesPlayedRecently = 5;

        int eligibleLeaguePlayerCount;
        await using (var mySqlConnection =
                     new MySqlConnection(_connectionString))
        {
            await mySqlConnection.OpenAsync(token);

            const string leagueRequiredGamesQuery = @"select RequiredGames from League order by LeagueID asc";

            await using (var mySqlCommand =
                         new MySqlCommand(leagueRequiredGamesQuery, mySqlConnection) { CommandTimeout = LongCommandTimeout })
            {
                queryLogger.LogSqlCommand(mySqlCommand);
                await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
                {
                    while (await mySqlDataReader.ReadAsync(token))
                    {
                        leagueRequiredGames.Add(mySqlDataReader.GetInt32("RequiredGames"));
                    }
                }
            }

            minRequiredGames = leagueRequiredGames.Min(i => i);

            const string eligibleLeaguePlayerCountQuery =
                @"select count(*)
                    from Player p
                    join LeaderboardRanking lr     on lr.PlayerID = p.PlayerID   and   lr.GameMode = @gameMode
                    join PlayerAggregate pa        on pa.PlayerID = p.PlayerID   and   pa.GameMode = @gameMode
                    where p.BattleNetRegionId = @battleNetRegionId and
                          pa.GamesPlayedWithMMR >= @minRequiredGames";

            Log("Begin Calculating Eligible League Player Count", debug: true);

            await using (var mySqlCommand =
                         new MySqlCommand(
                             eligibleLeaguePlayerCountQuery.Replace("@minRequiredGames", minRequiredGames.ToString()),
                             mySqlConnection)
                         { CommandTimeout = LongCommandTimeout })
            {
                mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                var scalarResult = await mySqlCommand.ExecuteScalarAsync(token);
                scalarResult = scalarResult == DBNull.Value ? null : scalarResult;
                eligibleLeaguePlayerCount = (int)Convert.ToInt64(scalarResult);
            }

            Log("Done Calculating Eligible League Player Count", debug: true);
        }

        var leagueSizes = new[]
        {
            (int)(0.01m * eligibleLeaguePlayerCount),
            (int)(0.09m * eligibleLeaguePlayerCount),
            (int)(0.20m * eligibleLeaguePlayerCount),
            (int)(0.20m * eligibleLeaguePlayerCount),
            (int)(0.20m * eligibleLeaguePlayerCount),
            (int)(0.35m * eligibleLeaguePlayerCount),
        };

        // Gather leaderboard opt out list
        var leaderboardOptOutDictionary = new Dictionary<int, bool>();

        // Set each player's league and leaderboard eligibility
        Log("Beginning to Set Player League, Rank, and Leaderboard Eligibility", debug: true);

        var startOfLeagueMMR = 9999;

        const string leaderboardOptOutQuery =
            @"select lo.PlayerID
                    from LeaderboardOptOut lo
                    join Player p on p.PlayerID = lo.PlayerID
                    where p.BattleNetRegionId = @battleNetRegionId
                    union
                    select pbl.PlayerID
                    from PlayerBannedLeaderboard pbl
                    join Player p on p.PlayerID = pbl.PlayerID
                    where p.BattleNetRegionId = @battleNetRegionId";

        // First set league based on MMR and minimum required games
        const string eligibleLeaderboardRankingsQuery =
            @"select lr.PlayerID, lr.CurrentMMR, lr.LeagueID
                    from LeaderboardRanking lr
                    join Player p on p.PlayerID = lr.PlayerID and p.BattleNetRegionId = @battleNetRegionId
                    join PlayerAggregate pa on pa.PlayerID = p.PlayerID and pa.GameMode = @gameMode
                    where lr.GameMode = @gameMode
                    and lr.CurrentMMR <= @startOfLeagueMMR
                    and pa.GamesPlayedWithMMR >= @minRequiredGames
                    order by lr.CurrentMMR desc
                    limit @currentLeagueSize";

        const string leaderboardRankingsUpdateQuery =
            @"update LeaderboardRanking set IsEligibleForLeaderboard = false, LeagueID = @leagueID, LeagueRank = null where GameMode = @gameMode and PlayerID in (@playerIDs)";

        await using (var mySqlConnection =
                     new MySqlConnection(_connectionString))
        {
            await mySqlConnection.OpenAsync(token);

            await using (var mySqlCommand =
                         new MySqlCommand(leaderboardOptOutQuery, mySqlConnection) { CommandTimeout = LongCommandTimeout })
            {
                mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                queryLogger.LogSqlCommand(mySqlCommand);
                await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
                {
                    while (await mySqlDataReader.ReadAsync(token))
                    {
                        leaderboardOptOutDictionary[mySqlDataReader.GetInt32("PlayerID")] = true;
                    }
                }
            }

            for (var i = 0; i < leagueSizes.Length; i++)
            {
                var playersToUpdateInLeagueList = new List<int>();
                var cmdText4 = eligibleLeaderboardRankingsQuery
                    .Replace("@currentLeagueSize", leagueSizes[i].ToString());
                await using (var mySqlCommand =
                             new MySqlCommand(cmdText4, mySqlConnection) { CommandTimeout = LongCommandTimeout })
                {
                    mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                    mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                    mySqlCommand.Parameters.AddWithValue("@startOfLeagueMMR", startOfLeagueMMR);
                    mySqlCommand.Parameters.AddWithValue("@minRequiredGames", minRequiredGames);
                    queryLogger.LogSqlCommand(mySqlCommand);
                    await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
                    {
                        while (await mySqlDataReader.ReadAsync(token))
                        {
                            var obj = new
                            {
                                CurrentMMR = mySqlDataReader.GetInt32("CurrentMMR"),
                                LeagueID = mySqlDataReader["LeagueID"] is DBNull
                                    ? null
                                    : (int?)mySqlDataReader.GetInt32("LeagueID"),
                                PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                            };

                            startOfLeagueMMR = obj.CurrentMMR;
                            if (!obj.LeagueID.HasValue || obj.LeagueID != i)
                            {
                                playersToUpdateInLeagueList.Add(obj.PlayerID);
                            }
                        }
                    }
                }

                if (!playersToUpdateInLeagueList.Any())
                {
                    continue;
                }

                for (var j = 0; j < playersToUpdateInLeagueList.Count; j += playersToProcessAtATime)
                {
                    var cmdText5 = leaderboardRankingsUpdateQuery
                        .Replace(
                            "@playerIDs",
                            string.Join(@",", playersToUpdateInLeagueList.Skip(j).Take(playersToProcessAtATime)));
                    if (!dryRun)
                    {
                        await using var mySqlCommand =
                            new MySqlCommand(cmdText5, mySqlConnection) { CommandTimeout = LongCommandTimeout };
                        mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                        mySqlCommand.Parameters.AddWithValue("@leagueID", i);
                        await mySqlCommand.ExecuteNonQueryAsync(token);
                    }

                    Log(
                        "Calculating BattleNetRegionId: " + battleNetRegionId + ", League: " + i +
                        ", Progress: " + j + " / " + playersToUpdateInLeagueList.Count, debug: true);
                }

                Log("Finished BattleNetRegionId: " + battleNetRegionId + ", League: " + i, debug: true);
            }
        }

        var leaguePlayersLeaderboardEligibilityAndRankingQuery =
            @"select lr.PlayerID
                from LeaderboardRanking lr
                join Player p on p.PlayerID = lr.PlayerID and p.BattleNetRegionId = @battleNetRegionId
                join PlayerAggregate pa on pa.PlayerID = p.PlayerID and pa.GameMode = @gameMode
                where lr.GameMode = @gameMode
                and lr.LeagueID = @leagueID
                and pa.GamesPlayedWithMMR @gamesPlayedOperator @currentLeagueRequiredGames
                @gamesPlayedComparison pa.GamesPlayedRecently @gamesPlayedOperator @recentLeagueRequiredGames
                order by lr.CurrentMMR desc";

        const string eligibleLeaguePlayersLeaderboardRankingUpdateQuery =
            @"update LeaderboardRanking set IsEligibleForLeaderboard = @isEligibleForLeaderboard, LeagueRank = @leagueRank where GameMode = @gameMode and PlayerID = @playerID";
        const string ineligibleLeaguePlayersLeaderboardRankingUpdateQuery =
            @"update LeaderboardRanking set IsEligibleForLeaderboard = false, LeagueRank = null where GameMode = @gameMode and PlayerID in (@playerIDs)";

        // Next determine league eligibility and rank
        Log(
            "Beginning Calculating Eligibility and Rank for BattleNetRegionId: " + battleNetRegionId +
            ", All Leagues", debug: true);

        var sitewideLeaderboardStatisticsList = new List<SitewideLeaderboardStatisticV3>[leagueSizes.Length];
        for (var i = 0; i < sitewideLeaderboardStatisticsList.Length; i++)
        {
            sitewideLeaderboardStatisticsList[i] = new List<SitewideLeaderboardStatisticV3>();
        }

        await using (var mySqlConnection =
                     new MySqlConnection(_connectionString))
        {
            await mySqlConnection.OpenAsync(token);
            for (var i = 0; i < leagueSizes.Length; i++)
            {
                // Add eligible players to leaderboard
                {
                    var eligibleLeaguePlayersLeaderboardRankingList = new List<int>();
                    await using (var mySqlCommand =
                                 new MySqlCommand(
                                     leaguePlayersLeaderboardEligibilityAndRankingQuery
                                         .Replace("@gamesPlayedOperator", ">=")
                                         .Replace("@gamesPlayedComparison", "and"),
                                     mySqlConnection)
                                 { CommandTimeout = LongCommandTimeout })
                    {
                        mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                        mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                        mySqlCommand.Parameters.AddWithValue("@leagueID", i);
                        mySqlCommand.Parameters.AddWithValue("@currentLeagueRequiredGames", leagueRequiredGames[i]);
                        mySqlCommand.Parameters.AddWithValue(
                            "@recentLeagueRequiredGames",
                            minRequiredGamesPlayedRecently);
                        queryLogger.LogSqlCommand(mySqlCommand);
                        await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
                        {
                            while (await mySqlDataReader.ReadAsync(token))
                            {
                                eligibleLeaguePlayersLeaderboardRankingList.Add(mySqlDataReader.GetInt32("PlayerID"));
                            }
                        }
                    }

                    if (!eligibleLeaguePlayersLeaderboardRankingList.Any())
                    {
                        continue;
                    }

                    var eligibleLeaguePlayersLeaderboardRanking =
                        eligibleLeaguePlayersLeaderboardRankingList.ToArray();
                    var currentLeagueOptOutCount = 0;
                    await using (var mySqlCommand =
                                 new MySqlCommand(eligibleLeaguePlayersLeaderboardRankingUpdateQuery, mySqlConnection)
                                 {
                                     CommandTimeout = LongCommandTimeout,
                                 })
                    {
                        for (var j = 0; j < eligibleLeaguePlayersLeaderboardRanking.Length; j++)
                        {
                            mySqlCommand.Parameters.Clear();
                            mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                            mySqlCommand.Parameters.AddWithValue(
                                "@playerID",
                                eligibleLeaguePlayersLeaderboardRanking[j]);

                            if (leaderboardOptOutDictionary.ContainsKey(eligibleLeaguePlayersLeaderboardRanking[j]))
                            {
                                mySqlCommand.Parameters.AddWithValue("@isEligibleForLeaderboard", false);
                                mySqlCommand.Parameters.AddWithValue("@leagueRank", DBNull.Value);
                                currentLeagueOptOutCount++;
                            }
                            else
                            {
                                mySqlCommand.Parameters.AddWithValue("@isEligibleForLeaderboard", true);
                                mySqlCommand.Parameters.AddWithValue("@leagueRank", j + 1 - currentLeagueOptOutCount);
                            }

                            if (!dryRun)
                            {
                                await mySqlCommand.ExecuteNonQueryAsync(token);
                            }

                            if (j % 1000 == 0)
                            {
                                Log(
                                    "Calculating Eligibility and Rank for BattleNetRegionId: " + battleNetRegionId +
                                    ", League: " + i + ", Progress: " + j + " / " +
                                    eligibleLeaguePlayersLeaderboardRanking.Length, debug: true);
                            }
                        }
                    }
                }

                // Remove ineligible players from leaderboard
                {
                    var ineligibleLeaguePlayersLeaderboardRankingList = new List<int>();
                    await using (var mySqlCommand =
                                 new MySqlCommand(
                                     leaguePlayersLeaderboardEligibilityAndRankingQuery
                                         .Replace("@gamesPlayedOperator", "<")
                                         .Replace("@gamesPlayedComparison", "or"),
                                     mySqlConnection)
                                 { CommandTimeout = LongCommandTimeout })
                    {
                        mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                        mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                        mySqlCommand.Parameters.AddWithValue("@leagueID", i);
                        mySqlCommand.Parameters.AddWithValue("@currentLeagueRequiredGames", leagueRequiredGames[i]);
                        mySqlCommand.Parameters.AddWithValue(
                            "@recentLeagueRequiredGames",
                            minRequiredGamesPlayedRecently);
                        queryLogger.LogSqlCommand(mySqlCommand);
                        await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
                        {
                            while (await mySqlDataReader.ReadAsync(token))
                            {
                                ineligibleLeaguePlayersLeaderboardRankingList.Add(mySqlDataReader.GetInt32("PlayerID"));
                            }
                        }
                    }

                    if (!ineligibleLeaguePlayersLeaderboardRankingList.Any())
                    {
                        continue;
                    }

                    for (var j = 0; j < ineligibleLeaguePlayersLeaderboardRankingList.Count; j += 1000)
                    {
                        var cmdText6 = ineligibleLeaguePlayersLeaderboardRankingUpdateQuery
                            .Replace(
                                "@playerIDs",
                                string.Join(@",", ineligibleLeaguePlayersLeaderboardRankingList.Skip(j).Take(1000)));
                        if (!dryRun)
                        {
                            await using var mySqlCommand =
                                new MySqlCommand(cmdText6, mySqlConnection) { CommandTimeout = LongCommandTimeout };
                            mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                            await mySqlCommand.ExecuteNonQueryAsync(token);
                        }

                        Log(
                            "Calculating Ineligibility for BattleNetRegionId: " + battleNetRegionId +
                            ", League: " + i + ", Progress: " + j + " / " +
                            ineligibleLeaguePlayersLeaderboardRankingList.Count, debug: true);
                    }
                }
            }

            Log("Beginning Caching the New Leaderboards for BattleNetRegionId: " + battleNetRegionId, debug: true);

            const string cacheLeaderboardQuery =
                @"select lr.LeagueID, lr.LeagueRank, p.`Name`, pa.GamesPlayedWithMMR, lr.CurrentMMR, lr.PlayerID, n48u.PremiumSupporterSince
                    from LeaderboardRanking lr
                    join Player p on p.PlayerID = lr.PlayerID and p.BattleNetRegionId = @battleNetRegionId
                    join PlayerAggregate pa on pa.PlayerID = p.PlayerID and pa.GameMode = @gameMode
                    left join net48_users n48u on n48u.playerid = p.playerid
                    where lr.GameMode = @gameMode
                    and lr.IsEligibleForLeaderboard = true
                    order by lr.LeagueID, lr.LeagueRank";

            await using (var mySqlCommand =
                         new MySqlCommand(cacheLeaderboardQuery, mySqlConnection) { CommandTimeout = LongCommandTimeout })
            {
                mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                queryLogger.LogSqlCommand(mySqlCommand);
                await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
                {
                    while (await mySqlDataReader.ReadAsync(token))
                    {
                        var obj = new
                        {
                            LeagueID = mySqlDataReader.GetInt32("LeagueID"),
                            LeagueRank = mySqlDataReader.GetInt32("LeagueRank"),
                            Name = mySqlDataReader.GetString("Name"),
                            GamesPlayedWithMMR = mySqlDataReader.GetInt32("GamesPlayedWithMMR"),
                            CurrentMMR = mySqlDataReader.GetInt32("CurrentMMR"),
                            PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                            PremiumSupporterSince = mySqlDataReader["PremiumSupporterSince"] is DBNull
                                ? null
                                : (DateTime?)mySqlDataReader.GetDateTime("PremiumSupporterSince"),
                        };

                        sitewideLeaderboardStatisticsList[obj.LeagueID].Add(
                            new SitewideLeaderboardStatisticV3
                            {
                                LR = obj.LeagueRank,
                                N = obj.Name,
                                GP = obj.GamesPlayedWithMMR,
                                R = obj.CurrentMMR,
                                PID = obj.PlayerID,
                                TSS = obj.PremiumSupporterSince,
                            });
                    }
                }
            }
        }

        if (!dryRun)
        {
            // Cache the new leaderboards
            using var scope = _svcp.CreateScope();
            var redisClient = MyDbWrapper.Create(scope);
            for (var i = 0; i < leagueSizes.Length; i++)
            {
                redisClient.TrySet(
                    "HOTSLogs:SitewideLeaderboardStatisticsV3:" + battleNetRegionId + ":" + gameMode + ":" + i,
                    new SitewideLeaderboardStatisticsV3
                    {
                        BattleNetRegionId = battleNetRegionId,
                        GameMode = gameMode,
                        League = i,
                        LastUpdated = Now,
                        SitewideLeaderboardStatisticArray = sitewideLeaderboardStatisticsList[i].ToArray(),
                    },
                    TimeSpan.FromDays(30));
            }
        }

        Log("All Done!");
    }

    private async Task ThrowIfCancellationRequested(CancellationToken token)
    {
        await Task.Yield();
        token.ThrowIfCancellationRequested();
    }

    public void AggregateReplayCharacterMMR(
        int battleNetRegionId,
        int gameMode,
        DateTime replayDate,
        bool dryRun = false)
    {
        replayDate = replayDate.Date;
        var replayDateString = replayDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var replayDateEnd = replayDate.AddDays(1);
        var previousReplayDate = replayDate.AddDays(-1);
        const double conservativeStandardDeviationMultiplier = 0.96d;
        const int databaseRecordsToUpdateInSingleQuery = 1000;
        const int numberOfDatabaseUpdateStatementsToRunConcurrently = 30;

        var events = new List<string>();
        var applicationEventFileName =
            "AggregateReplayCharacterMMR Region " + battleNetRegionId + " GameMode " + gameMode + ".txt";

        void Log(string msg, bool debug = false)
        {
            if (dryRun)
            {
                msg = $"(dryrun) {msg}";
            }

            DataHelper.LogApplicationEvents(msg, applicationEventFileName, debug);
        }

        Log(
            "Beginning AggregateReplayCharacterMMR: BattleNetRegionID: " + battleNetRegionId + " GameMode " + gameMode +
            " Date " + replayDateString);

        var queryLogger = new SqlQueryLogger(Log);

        using var scope = _svcp.CreateScope();
        var heroesEntity = HeroesdataContext.Create(scope);

        var playerMMRReset = heroesEntity.PlayerMmrResets.SingleOrDefault(i => i.ResetDate == replayDate);

        using (var mySqlConnection =
               new MySqlConnection(_connectionString))
        {
            mySqlConnection.Open();

            // Get every replay character in the selected region and replay date
            var replayCharacters = new List<(int replayId, int playerId, bool isWinner)>();
            using (var mySqlCommand = new MySqlCommand(
                       @"select rc.ReplayID, rc.PlayerID, rc.IsWinner
                    from ReplayCharacter rc
                    join Replay r on r.ReplayID = rc.ReplayID
                    join Player p on p.PlayerID = rc.PlayerID
                    where r.GameMode = @gameMode and r.TimestampReplay >= @replayDate and r.TimestampReplay < @replayDateEnd and p.BattleNetRegionId = @battleNetRegionId " +
                       (gameMode == -1 ? "and r.MapID = 1011 " : null) +
                       @"order by r.TimestampReplay asc, rc.ReplayID asc, rc.IsWinner desc",
                       mySqlConnection))
            {
                mySqlCommand.CommandTimeout = LongCommandTimeout;
                mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                mySqlCommand.Parameters.AddWithValue("@replayDate", replayDate);
                mySqlCommand.Parameters.AddWithValue("@replayDateEnd", replayDateEnd);
                mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                queryLogger.LogSqlCommand(mySqlCommand);
                using (var mySqlDataReader = mySqlCommand.ExecuteReader())
                {
                    while (mySqlDataReader.Read())
                    {
                        var obj = new
                        {
                            ReplayID = mySqlDataReader.GetInt32("ReplayID"),
                            PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                            IsWinner = mySqlDataReader.GetUInt64("IsWinner"),
                        };

                        replayCharacters.Add((obj.ReplayID, obj.PlayerID, obj.IsWinner == 1));
                    }
                }
            }

            // If there aren't any to process for this region and date, we have nothing to do
            if (playerMMRReset == null && !replayCharacters.Any())
            {
                // Log("replayCharacters.count(): " + replayCharacters.Count() + " BattleNetRegionID: " + battleNetRegionId + " GameMode: " + gameMode + " Date: " + replayDateString);
                return;
            }

            // We use whatever is the most recent MMR milestone, as some players don't have any games for certain days
            const string previousPlayerMMRMilestoneByReplayTimestamp =
                @"select playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID, outerPM.MMRMean, outerPM.MMRStandardDeviation from
                        (select playerIDs.PlayerID, max(pm.MilestoneDate) as MaxPlayerMMRMilestoneDate from
                        (select distinct(rc.PlayerID) as PlayerID
                        from ReplayCharacter rc
                        join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
                        join Player p on p.PlayerID = rc.PlayerID
                        where r.GameMode = @gameMode and r.TimestampReplay >= @replayDate and r.TimestampReplay < @replayDateEnd and p.BattleNetRegionId = @battleNetRegionId) playerIDs
                        join PlayerMMRMilestoneV3 pm on pm.PlayerID = playerIDs.PlayerID and pm.GameMode = @gameMode and pm.MilestoneDate <= @previousReplayDate
                        group by playerIDs.PlayerID) playerIDsAndMaxPlayerMMRMilestoneDate
                        join PlayerMMRMilestoneV3 outerPM on outerPM.PlayerID = playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID and outerPM.GameMode = @gameMode and outerPM.MilestoneDate = playerIDsAndMaxPlayerMMRMilestoneDate.MaxPlayerMMRMilestoneDate";

            const string previousPlayerMMRMilestoneAll =
                @"select playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID, outerPM.MMRMean, outerPM.MMRStandardDeviation from
                        (select playerIDs.PlayerID, max(pm.MilestoneDate) as MaxPlayerMMRMilestoneDate from
                        (select distinct(rc.PlayerID) as PlayerID
                        from ReplayCharacter rc
                        join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
                        join Player p on p.PlayerID = rc.PlayerID
                        where r.GameMode = @gameMode and r.TimestampReplay < @replayDateEnd and p.BattleNetRegionId = @battleNetRegionId) playerIDs
                        join PlayerMMRMilestoneV3 pm on pm.PlayerID = playerIDs.PlayerID and pm.GameMode = @gameMode and pm.MilestoneDate <= @previousReplayDate
                        group by playerIDs.PlayerID) playerIDsAndMaxPlayerMMRMilestoneDate
                        join PlayerMMRMilestoneV3 outerPM on outerPM.PlayerID = playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID and outerPM.GameMode = @gameMode and outerPM.MilestoneDate = playerIDsAndMaxPlayerMMRMilestoneDate.MaxPlayerMMRMilestoneDate";

            var previousPlayerMMRMilestones = new Dictionary<int, (double mmrMean, double mmrStdDev)>();
            using (var mySqlCommand =
                   new MySqlCommand(
                       playerMMRReset == null
                           ? previousPlayerMMRMilestoneByReplayTimestamp
                           : previousPlayerMMRMilestoneAll,
                       mySqlConnection))
            {
                mySqlCommand.CommandTimeout = LongCommandTimeout;
                mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                if (playerMMRReset == null)
                {
                    mySqlCommand.Parameters.AddWithValue("@replayDate", replayDate);
                }

                mySqlCommand.Parameters.AddWithValue("@replayDateEnd", replayDateEnd);
                mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                mySqlCommand.Parameters.AddWithValue("@previousReplayDate", previousReplayDate);
                queryLogger.LogSqlCommand(mySqlCommand);
                using (var mySqlDataReader = mySqlCommand.ExecuteReader())
                {
                    while (mySqlDataReader.Read())
                    {
                        var obj = new
                        {
                            MMRMean = mySqlDataReader.GetDouble("MMRMean"),
                            MMRStandardDeviation = mySqlDataReader.GetDouble("MMRStandardDeviation"),
                            PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                        };

                        var mmrMean = obj.MMRMean;
                        var mmrStdDev = obj.MMRStandardDeviation;
                        previousPlayerMMRMilestones[obj.PlayerID] = (mmrMean, mmrStdDev);
                    }
                }
            }

            // if ((replayDate.Year < 2019 && (gameMode == 6 /* Unranked Draft */ || gameMode == 5 /* Team League */))
            //     || (replayDate.Year == 2019 && gameMode == 8 /* Storm League */))
            if (replayDate.Year < 2019 && (gameMode == 6 /* Unranked Draft */ || gameMode == 5 /* Team League */))
            {
                // If a player is new to Unranked Draft or Team league, let's seed their MMR with their latest Quick Match or Hero league milestone respectively
                const string previousPlayerMMRMilestoneForSpecificPlayers =
                    @"select playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID, outerPM.MMRMean, outerPM.MMRStandardDeviation from
                            (select p.PlayerID, max(pm.MilestoneDate) as MaxPlayerMMRMilestoneDate
                            from Player p
                            join PlayerMMRMilestoneV3 pm on pm.PlayerID = p.PlayerID and pm.GameMode in (@gameMode) and pm.MilestoneDate <= @previousReplayDate
                            where p.PlayerID in ({0})
                            group by p.PlayerID) playerIDsAndMaxPlayerMMRMilestoneDate
                            join PlayerMMRMilestoneV3 outerPM on outerPM.PlayerID = playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID and outerPM.GameMode = @gameMode and outerPM.MilestoneDate = playerIDsAndMaxPlayerMMRMilestoneDate.MaxPlayerMMRMilestoneDate";

                var heroLeagueMMRSeedMMRReset = new PlayerMmrReset
                {
                    MmrmeanMultiplier = 0.8,
                    MmrstandardDeviationGapMultiplier = 0.2,
                    IsClampOutliers = 0,
                };
                var playerIDsWithoutMilestones = replayCharacters
                    .Select(i => i.playerId)
                    .Distinct()
                    .Where(i => !previousPlayerMMRMilestones.ContainsKey(i))
                    .ToArray();

                if (playerIDsWithoutMilestones.Length > 0)
                {
                    var cmdText7 = string.Format(
                        previousPlayerMMRMilestoneForSpecificPlayers,
                        string.Join(",", playerIDsWithoutMilestones));
                    using var mySqlCommand = new MySqlCommand(cmdText7, mySqlConnection);
                    mySqlCommand.CommandTimeout = LongCommandTimeout;
                    mySqlCommand.Parameters.AddWithValue(
                        "@gameMode",
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        gameMode == 5 ? "4" /* Hero League */ :
                        gameMode == 6 ? "3" /* Quick Match */ :
                        gameMode == 8 ? "4,5" : "-999");
                    if (playerMMRReset == null)
                    {
                        mySqlCommand.Parameters.AddWithValue("@replayDate", replayDate);
                    }

                    mySqlCommand.Parameters.AddWithValue("@replayDateEnd", replayDateEnd);
                    mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                    mySqlCommand.Parameters.AddWithValue("@previousReplayDate", previousReplayDate);
                    queryLogger.LogSqlCommand(mySqlCommand);
                    using var mySqlDataReader = mySqlCommand.ExecuteReader();
                    while (mySqlDataReader.Read())
                    {
                        var obj = new
                        {
                            MMRMean = mySqlDataReader.GetDouble("MMRMean"),
                            MMRStandardDeviation = mySqlDataReader.GetDouble("MMRStandardDeviation"),
                            PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                        };

                        var mmrMean = obj.MMRMean;
                        var mmrStdDev = obj.MMRStandardDeviation;
                        var mmrMeanNormalized = ((mmrMean - GameInfo.DefaultGameInfo.InitialMean) *
                                                 heroLeagueMMRSeedMMRReset.MmrmeanMultiplier) +
                                                GameInfo.DefaultGameInfo.InitialMean;
                        var mmrStdDevNormalized =
                            mmrStdDev + ((GameInfo.DefaultGameInfo.InitialStandardDeviation - mmrStdDev) *
                                         heroLeagueMMRSeedMMRReset.MmrstandardDeviationGapMultiplier);
                        previousPlayerMMRMilestones[obj.PlayerID] =
                            (mmrMeanNormalized, mmrStdDevNormalized);
                    }
                }
            }

            // Create the list of players and ratings from saved MMR milestone information
            Rating RatingOrNull(int i)
            {
                return previousPlayerMMRMilestones.ContainsKey(i)
                    ? new Rating(previousPlayerMMRMilestones[i].mmrMean, previousPlayerMMRMilestones[i].mmrStdDev)
                    : null;
            }

            var players = replayCharacters.Select(i => i.playerId)
                .Union(playerMMRReset != null ? previousPlayerMMRMilestones.Keys.ToArray() : Array.Empty<int>())
                .Distinct()
                .Select(i => (player: new Player(i), rating: RatingOrNull(i)))
                .ToDictionary(i => (int)i.player.Id, i => i);

            Log("Gathered Player and ReplayCharacter Entities from Database");

            if (playerMMRReset != null)
            {
                Log("Adjusting MMR for Player MMR Reset Event: " + playerMMRReset.Title);

                foreach (var player in players.Where(i => i.Value.rating != null).ToArray())
                {
                    var mmrMean = ((player.Value.rating.Mean - GameInfo.DefaultGameInfo.InitialMean) *
                                   playerMMRReset.MmrmeanMultiplier) + GameInfo.DefaultGameInfo.InitialMean;
                    var mmrStdDev = player.Value.rating.StandardDeviation +
                                    ((GameInfo.DefaultGameInfo.InitialStandardDeviation -
                                      player.Value.rating.StandardDeviation) *
                                     playerMMRReset.MmrstandardDeviationGapMultiplier);
                    var newRating = new Rating(mmrMean, mmrStdDev);

                    if (playerMMRReset.IsClampOutliers != 0)
                    {
                        if (newRating.Mean - (newRating.StandardDeviation * conservativeStandardDeviationMultiplier) >
                            35)
                        {
                            newRating = new Rating(
                                35 + (newRating.StandardDeviation * conservativeStandardDeviationMultiplier),
                                newRating.StandardDeviation);
                        }
                        else if (newRating.Mean -
                                 (newRating.StandardDeviation * conservativeStandardDeviationMultiplier) < 0)
                        {
                            newRating = new Rating(
                                newRating.StandardDeviation * conservativeStandardDeviationMultiplier,
                                newRating.StandardDeviation);
                        }
                    }

                    players[player.Key] = (player.Value.player, newRating);
                }

                Log("Done Adjusting MMR for Player MMR Reset Event: " + playerMMRReset.Title);
            }

            var sqlQueryUpdate = string.Empty;
            var replayCharactersInGame = new (int replayId, int playerId, bool isWinner)[10];
            var replayPlayersInGame = new (Player player, Rating rating)[replayCharactersInGame.Length];
            var sqlUpdateTaskList = new List<Task>();
            var ii = 0;
            var updateCountdown = databaseRecordsToUpdateInSingleQuery;
            while (ii < replayCharacters.Count)
            {
                var scannedReplayId = replayCharacters[ii].replayId;
                var jj = 0;
                while (ii < replayCharacters.Count && replayCharacters[ii].replayId == scannedReplayId)
                {
                    if (jj < 10)
                    {
                        replayCharactersInGame[jj] = replayCharacters[ii];
                        replayPlayersInGame[jj] = players[replayCharactersInGame[jj].playerId];
                    }

                    jj++;
                    ii++;
                }

                if (jj != 10)
                {
                    Log($"Bad replay found with {jj} characters. Replay Id {scannedReplayId}");
                }
                else
                {
                    for (var j = 0; j < 10; j++)
                    {
                        if (replayPlayersInGame[j].rating == null)
                        {
                            replayPlayersInGame[j] = (replayPlayersInGame[j].player,
                                GameInfo.DefaultGameInfo.DefaultRating);
                            players[(int)replayPlayersInGame[j].player.Id] = replayPlayersInGame[j];
                        }
                    }

                    var winningTeam = new Team<Player>();
                    for (var j = 0; j < 5; j++)
                    {
                        winningTeam = winningTeam.AddPlayer(
                            replayPlayersInGame[j].player,
                            replayPlayersInGame[j].rating);
                    }

                    var losingTeam = new Team<Player>();
                    for (var j = 5; j < 10; j++)
                    {
                        losingTeam = losingTeam.AddPlayer(replayPlayersInGame[j].player, replayPlayersInGame[j].rating);
                    }

                    foreach (var newRating in TrueSkillCalculator.CalculateNewRatings(
                                 GameInfo.DefaultGameInfo,
                                 Teams.Concat(winningTeam, losingTeam),
                                 1,
                                 2))
                    {
                        var (player, rating) = players[(int)newRating.Key.Id];

                        /*
                         * 5/20/2020 8:50:07 AM: 5/20/2020:8:50:07 AM:Aggregating MMR region 2 mode 8 date 11/19/2019
                         * 5/20/2020 8:52:32 AM: 5/20/2020:8:52:32 AM:While processing for mode 8 on
                         * 5/20/2020 8:52:36 AM: 5/20/2020:8:52:36 AM:Error:System.InvalidOperationException: Sequence contains more than one matching element
                         *    at System.Linq.Enumerable.Single[TSource](IEnumerable`1 source, Func`2 predicate)
                         *    at Helper.MMR.AggregateReplayCharacterMMR(Int32 battleNetRegionId, Int32 gameMode, DateTime replayDate, Boolean dryRun) in D:\StarKillerLLC\HOTSlogs\DataHelper\MMR.cs:line 945
                         *    at HOTSAPIParser.Services.MMRProcessOlderDates.<Run>b__0_3(<>f__AnonymousType0`2 threadData) in D:\StarKillerLLC\HOTSlogs\HOTSAPIParser\Services\MMRProcessOlderDates.cs:line 55
                         */
                        var cnt = replayCharactersInGame.Count(x => x.playerId == (int)player.Id);
                        if (cnt != 1)
                        {
                            Log(
                                $"Bug when processing replay {scannedReplayId} player id {player.Id} occurs {cnt} times, skipping player.");
                            continue;
                        }

                        var (replayId, playerId, isWinner) =
                            replayCharactersInGame.Single(x => x.playerId == (int)player.Id);

                        var mmrBefore =
                            (int)((rating.Mean -
                                   (rating.StandardDeviation * conservativeStandardDeviationMultiplier)) * 100);

                        // Some ridiculous matchups lead to 'NaN' being calculated, such as when one team is ~4200 MMR and the other ~1500 MMR
                        // Also, if winning results in -1 MMR change, we will instead discard the match
                        int mmrChange;
                        if (!double.IsNaN(newRating.Value.Mean))
                        {
                            mmrChange = (int)((newRating.Value.Mean -
                                               (newRating.Value.StandardDeviation *
                                                conservativeStandardDeviationMultiplier)) * 100) - mmrBefore;
                            if (isWinner && mmrChange < 0)
                            {
                                mmrChange = 0;
                            }
                            else
                            {
                                players[(int)newRating.Key.Id] = (newRating.Key, newRating.Value);
                            }
                        }
                        else
                        {
                            mmrChange = 0;
                        }

                        // Create the update query
                        sqlQueryUpdate += "update ReplayCharacter set MMRBefore=" + mmrBefore + ",MMRChange=" +
                                          mmrChange + " where ReplayID=" + replayId + " and PlayerID=" + playerId +
                                          ";";
                        updateCountdown--;
                    }
                }

                if (updateCountdown == 0 || ii == replayCharacters.Count)
                {
                    updateCountdown = databaseRecordsToUpdateInSingleQuery;
                    var commandText = sqlQueryUpdate;
                    sqlUpdateTaskList.Add(Task.Run(() => ExecuteMySqlCommandNonQuery(commandText, dryRun)));
                    sqlQueryUpdate = string.Empty;
                    Log("Calculated ReplayCharacter " + ii + " of " + replayCharacters.Count, debug: true);

                    if (sqlUpdateTaskList.Count >= numberOfDatabaseUpdateStatementsToRunConcurrently ||
                        ii == replayCharacters.Count)
                    {
                        foreach (var sqlUpdateTask in sqlUpdateTaskList)
                        {
                            sqlUpdateTask.Wait();
                        }

                        sqlUpdateTaskList.Clear();
                    }
                }
            }

            Log("Finished Processing ReplayCharacters", debug: true);

            // Calculate Milestone MMR for Each Player
            {
                sqlQueryUpdate = string.Empty;
                var i = 0;
                foreach (var player in players)
                {
                    if (player.Value.rating == null)
                    {
                        Log($"Player {player.Value.player.Id} has no previous MMR rating");
                        continue;
                    }

                    var mmrRating = (int)((player.Value.rating.Mean -
                                           (player.Value.rating.StandardDeviation *
                                            conservativeStandardDeviationMultiplier)) * 100);
                    sqlQueryUpdate +=
                        $@"
 insert into PlayerMMRMilestoneV3
 values({player.Key},{gameMode},'{replayDateString}',{player.Value.rating.Mean},{player.Value.rating.StandardDeviation},{mmrRating})
 on duplicate key update MMRMean={player.Value.rating.Mean},MMRStandardDeviation={player.Value.rating.StandardDeviation},MMRRating={mmrRating};
";

                    if (i++ % databaseRecordsToUpdateInSingleQuery == 0 || i == players.Count)
                    {
                        var commandText = sqlQueryUpdate;
                        sqlUpdateTaskList.Add(Task.Run(() => ExecuteMySqlCommandNonQuery(commandText, dryRun)));
                        sqlQueryUpdate = string.Empty;
                        Log("Saved Player " + i + " of " + players.Count, debug: true);

                        if (sqlUpdateTaskList.Count >= numberOfDatabaseUpdateStatementsToRunConcurrently ||
                            i == players.Count)
                        {
                            foreach (var sqlUpdateTask in sqlUpdateTaskList)
                            {
                                sqlUpdateTask.Wait();
                            }

                            sqlUpdateTaskList.Clear();
                        }
                    }
                }
            }
        }

        // Set Task Completed
        var dataEvent = "AggregateReplayCharacterMMR Region: " + battleNetRegionId + ", GameMode: " + gameMode +
                        ", Date: " + replayDateString;
        if (heroesEntity.DataUpdates.Any(i => i.DataEvent == dataEvent))
        {
            heroesEntity.DataUpdates.Single(i => i.DataEvent == dataEvent).LastUpdated = Now;
        }
        else
        {
            heroesEntity.DataUpdates.Add(
                new DataUpdate
                {
                    DataEvent = dataEvent,
                    LastUpdated = Now,
                });
        }

        if (!dryRun)
        {
            heroesEntity.SaveChanges();
        }

        Log("All Done!");
    }

    public async Task AggregateReplayCharacterMMRAsync(
        int battleNetRegionId,
        int gameMode,
        DateTime replayDate,
        bool dryRun = false,
        CancellationToken token = default)
    {
        replayDate = replayDate.Date;
        var replayDateString = replayDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var replayDateEnd = replayDate.AddDays(1);
        var previousReplayDate = replayDate.AddDays(-1);
        const double conservativeStandardDeviationMultiplier = 0.96d;
        const int databaseRecordsToUpdateInSingleQuery = 1000;
        const int numberOfDatabaseUpdateStatementsToRunConcurrently = 30;

        var events = new List<string>();
        var applicationEventFileName = $"MMR Calc Region {battleNetRegionId} Game Mode {gameMode}";

        void Log(string msg, bool debug = false)
        {
            if (dryRun)
            {
                msg = $"(dryrun) {msg}";
            }

            DataHelper.LogApplicationEvents(msg, applicationEventFileName, debug);
        }

        Log(
            "Beginning AggregateReplayCharacterMMR: BattleNetRegionID: " + battleNetRegionId + " GameMode " + gameMode +
            " Date " + replayDateString, debug: true);

        var queryLogger = new SqlQueryLogger(Log);

        using var scope = _svcp.CreateScope();
        var heroesEntity = HeroesdataContext.Create(scope);

        var playerMMRReset = heroesEntity.PlayerMmrResets.SingleOrDefault(i => i.ResetDate == replayDate);

        await using (var mySqlConnection =
                     new MySqlConnection(_connectionString))
        {
            await mySqlConnection.OpenAsync(token);

            // Get every replay character in the selected region and replay date
            var replayCharacters = new List<(int replayId, int playerId, bool isWinner)>();
            await using (var mySqlCommand = new MySqlCommand(
                             @"select rc.ReplayID, rc.PlayerID, rc.IsWinner
                    from ReplayCharacter rc
                    join Replay r on r.ReplayID = rc.ReplayID
                    join Player p on p.PlayerID = rc.PlayerID
                    where r.GameMode = @gameMode and
                          r.TimestampReplay >= @replayDate and
                          r.TimestampReplay < @replayDateEnd and
                          p.BattleNetRegionId = @battleNetRegionId " +
                             (gameMode == -1 ? "and r.MapID = 1011 " : null) +
                             @"order by r.TimestampReplay asc, rc.ReplayID asc, rc.IsWinner desc",
                             mySqlConnection))
            {
                mySqlCommand.CommandTimeout = LongCommandTimeout;
                mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                mySqlCommand.Parameters.AddWithValue("@replayDate", replayDate);
                mySqlCommand.Parameters.AddWithValue("@replayDateEnd", replayDateEnd);
                mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                queryLogger.LogSqlCommand(mySqlCommand);
                await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
                {
                    while (await mySqlDataReader.ReadAsync(token))
                    {
                        var obj = new
                        {
                            ReplayID = mySqlDataReader.GetInt32("ReplayID"),
                            PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                            IsWinner = mySqlDataReader.GetUInt64("IsWinner"),
                        };

                        replayCharacters.Add((obj.ReplayID, obj.PlayerID, obj.IsWinner == 1));
                    }
                }
            }

            // If there aren't any to process for this region and date, we have nothing to do
            if (playerMMRReset == null && !replayCharacters.Any())
            {
                // Log("replayCharacters.count(): " + replayCharacters.Count() + " BattleNetRegionID: " + battleNetRegionId + " GameMode: " + gameMode + " Date: " + replayDateString);
                return;
            }

            // We use whatever is the most recent MMR milestone, as some players don't have any games for certain days
            const string previousPlayerMMRMilestoneByReplayTimestamp =
                @"select playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID, outerPM.MMRMean, outerPM.MMRStandardDeviation from
                        (select playerIDs.PlayerID, max(pm.MilestoneDate) as MaxPlayerMMRMilestoneDate from
                            (select distinct(rc.PlayerID) as PlayerID
                                from ReplayCharacter rc
                                join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
                                join Player p on p.PlayerID = rc.PlayerID
                                where r.GameMode = @gameMode and
                                      r.TimestampReplay >= @replayDate and
                                      r.TimestampReplay < @replayDateEnd and
                                      p.BattleNetRegionId = @battleNetRegionId
                            ) playerIDs
                            join PlayerMMRMilestoneV3 pm on pm.PlayerID = playerIDs.PlayerID and
                                                            pm.GameMode = @gameMode and
                                                            pm.MilestoneDate <= @previousReplayDate
                            group by playerIDs.PlayerID
                        ) playerIDsAndMaxPlayerMMRMilestoneDate
                        join PlayerMMRMilestoneV3 outerPM on outerPM.PlayerID = playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID and
                                                             outerPM.GameMode = @gameMode and
                                                             outerPM.MilestoneDate = playerIDsAndMaxPlayerMMRMilestoneDate.MaxPlayerMMRMilestoneDate";

            const string previousPlayerMMRMilestoneAll =
                @"select playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID, outerPM.MMRMean, outerPM.MMRStandardDeviation from
                        (select playerIDs.PlayerID, max(pm.MilestoneDate) as MaxPlayerMMRMilestoneDate from
                            (select distinct(rc.PlayerID) as PlayerID
                                from ReplayCharacter rc
                                join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
                                join Player p on p.PlayerID = rc.PlayerID
                                where r.GameMode = @gameMode and
                                      r.TimestampReplay < @replayDateEnd and
                                      p.BattleNetRegionId = @battleNetRegionId
                            ) playerIDs
                            join PlayerMMRMilestoneV3 pm on pm.PlayerID = playerIDs.PlayerID and
                                                            pm.GameMode = @gameMode and
                                                            pm.MilestoneDate <= @previousReplayDate
                            group by playerIDs.PlayerID
                        ) playerIDsAndMaxPlayerMMRMilestoneDate
                        join PlayerMMRMilestoneV3 outerPM on outerPM.PlayerID = playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID and
                                                             outerPM.GameMode = @gameMode and
                                                             outerPM.MilestoneDate = playerIDsAndMaxPlayerMMRMilestoneDate.MaxPlayerMMRMilestoneDate";

            var previousPlayerMMRMilestones = new Dictionary<int, (double mmrMean, double mmrStdDev)>();
            await using (var mySqlCommand =
                         new MySqlCommand(
                             playerMMRReset == null
                                 ? previousPlayerMMRMilestoneByReplayTimestamp
                                 : previousPlayerMMRMilestoneAll,
                             mySqlConnection))
            {
                mySqlCommand.CommandTimeout = LongCommandTimeout;
                mySqlCommand.Parameters.AddWithValue("@gameMode", gameMode);
                if (playerMMRReset == null)
                {
                    mySqlCommand.Parameters.AddWithValue("@replayDate", replayDate);
                }

                mySqlCommand.Parameters.AddWithValue("@replayDateEnd", replayDateEnd);
                mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                mySqlCommand.Parameters.AddWithValue("@previousReplayDate", previousReplayDate);
                queryLogger.LogSqlCommand(mySqlCommand);
                await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
                {
                    while (await mySqlDataReader.ReadAsync(token))
                    {
                        var obj = new
                        {
                            MMRMean = mySqlDataReader.GetDouble("MMRMean"),
                            MMRStandardDeviation = mySqlDataReader.GetDouble("MMRStandardDeviation"),
                            PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                        };

                        var mmrMean = obj.MMRMean;
                        var mmrStdDev = obj.MMRStandardDeviation;
                        previousPlayerMMRMilestones[obj.PlayerID] = (mmrMean, mmrStdDev);
                    }
                }
            }

            // if ((replayDate.Year < 2019 && (gameMode == 6 /* Unranked Draft */ || gameMode == 5 /* Team League */))
            //     || (replayDate.Year == 2019 && gameMode == 8 /* Storm League */))
            if (replayDate.Year < 2019 && (gameMode == 6 /* Unranked Draft */ || gameMode == 5 /* Team League */))
            {
                // If a player is new to Unranked Draft or Team league, let's seed their MMR with their latest Quick Match or Hero league milestone respectively
                const string previousPlayerMMRMilestoneForSpecificPlayers =
                    @"select playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID, outerPM.MMRMean, outerPM.MMRStandardDeviation from
                            (select p.PlayerID, max(pm.MilestoneDate) as MaxPlayerMMRMilestoneDate
                                from Player p
                                join PlayerMMRMilestoneV3 pm on pm.PlayerID = p.PlayerID and
                                                                pm.GameMode in (@gameMode) and
                                                                pm.MilestoneDate <= @previousReplayDate
                                where p.PlayerID in ({0})
                                group by p.PlayerID
                            ) playerIDsAndMaxPlayerMMRMilestoneDate
                            join PlayerMMRMilestoneV3 outerPM on outerPM.PlayerID = playerIDsAndMaxPlayerMMRMilestoneDate.PlayerID and
                                                                 outerPM.GameMode = @gameMode and
                                                                 outerPM.MilestoneDate = playerIDsAndMaxPlayerMMRMilestoneDate.MaxPlayerMMRMilestoneDate";

                var heroLeagueMMRSeedMMRReset = new PlayerMmrReset
                {
                    MmrmeanMultiplier = 0.8,
                    MmrstandardDeviationGapMultiplier = 0.2,
                    IsClampOutliers = 0,
                };
                var playerIDsWithoutMilestones = replayCharacters
                    .Select(i => i.playerId)
                    .Distinct()
                    .Where(i => !previousPlayerMMRMilestones.ContainsKey(i))
                    .ToArray();

                if (playerIDsWithoutMilestones.Length > 0)
                {
                    var cmdText7 = string.Format(
                        previousPlayerMMRMilestoneForSpecificPlayers,
                        string.Join(",", playerIDsWithoutMilestones));
                    await using var mySqlCommand = new MySqlCommand(cmdText7, mySqlConnection);
                    mySqlCommand.CommandTimeout = LongCommandTimeout;
                    mySqlCommand.Parameters.AddWithValue(
                        "@gameMode",
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        gameMode == 5 ? "4" /* Hero League */ :
                        gameMode == 6 ? "3" /* Quick Match */ :
                        gameMode == 8 ? "4,5" : "-999");
                    if (playerMMRReset == null)
                    {
                        mySqlCommand.Parameters.AddWithValue("@replayDate", replayDate);
                    }

                    mySqlCommand.Parameters.AddWithValue("@replayDateEnd", replayDateEnd);
                    mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                    mySqlCommand.Parameters.AddWithValue("@previousReplayDate", previousReplayDate);
                    queryLogger.LogSqlCommand(mySqlCommand);
                    await using var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token);
                    while (await mySqlDataReader.ReadAsync(token))
                    {
                        var obj = new
                        {
                            MMRMean = mySqlDataReader.GetDouble("MMRMean"),
                            MMRStandardDeviation = mySqlDataReader.GetDouble("MMRStandardDeviation"),
                            PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                        };

                        var mmrMean = obj.MMRMean;
                        var mmrStdDev = obj.MMRStandardDeviation;
                        var mmrMeanNormalized = ((mmrMean - GameInfo.DefaultGameInfo.InitialMean) *
                                                 heroLeagueMMRSeedMMRReset.MmrmeanMultiplier) +
                                                GameInfo.DefaultGameInfo.InitialMean;
                        var mmrStdDevNormalized =
                            mmrStdDev + ((GameInfo.DefaultGameInfo.InitialStandardDeviation - mmrStdDev) *
                                         heroLeagueMMRSeedMMRReset.MmrstandardDeviationGapMultiplier);
                        previousPlayerMMRMilestones[obj.PlayerID] =
                            (mmrMeanNormalized, mmrStdDevNormalized);
                    }
                }
            }

            // Create the list of players and ratings from saved MMR milestone information
            Rating RatingOrNull(int i)
            {
                return previousPlayerMMRMilestones.ContainsKey(i)
                    ? new Rating(previousPlayerMMRMilestones[i].mmrMean, previousPlayerMMRMilestones[i].mmrStdDev)
                    : null;
            }

            var players = replayCharacters.Select(i => i.playerId)
                .Union(playerMMRReset != null ? previousPlayerMMRMilestones.Keys.ToArray() : Array.Empty<int>())
                .Distinct()
                .Select(i => (player: new Player(i), rating: RatingOrNull(i)))
                .ToDictionary(i => (int)i.player.Id, i => i);

            Log("Gathered Player and ReplayCharacter Entities from Database", debug: true);

            if (playerMMRReset != null)
            {
                Log("Adjusting MMR for Player MMR Reset Event: " + playerMMRReset.Title, debug: true);

                foreach (var player in players.Where(i => i.Value.rating != null).ToArray())
                {
                    var mmrMean = ((player.Value.rating.Mean - GameInfo.DefaultGameInfo.InitialMean) *
                                   playerMMRReset.MmrmeanMultiplier) + GameInfo.DefaultGameInfo.InitialMean;
                    var mmrStdDev = player.Value.rating.StandardDeviation +
                                    ((GameInfo.DefaultGameInfo.InitialStandardDeviation -
                                      player.Value.rating.StandardDeviation) *
                                     playerMMRReset.MmrstandardDeviationGapMultiplier);
                    var newRating = new Rating(mmrMean, mmrStdDev);

                    if (playerMMRReset.IsClampOutliers != 0)
                    {
                        if (newRating.Mean - (newRating.StandardDeviation * conservativeStandardDeviationMultiplier) >
                            35)
                        {
                            newRating = new Rating(
                                35 + (newRating.StandardDeviation * conservativeStandardDeviationMultiplier),
                                newRating.StandardDeviation);
                        }
                        else if (newRating.Mean -
                                 (newRating.StandardDeviation * conservativeStandardDeviationMultiplier) < 0)
                        {
                            newRating = new Rating(
                                newRating.StandardDeviation * conservativeStandardDeviationMultiplier,
                                newRating.StandardDeviation);
                        }
                    }

                    players[player.Key] = (player.Value.player, newRating);
                }

                Log("Done Adjusting MMR for Player MMR Reset Event: " + playerMMRReset.Title, debug: true);
            }

            var sqlQueryUpdate = string.Empty;
            var replayCharactersInGame = new (int replayId, int playerId, bool isWinner)[10];
            var replayPlayersInGame = new (Player player, Rating rating)[replayCharactersInGame.Length];
            var sqlUpdateTaskList = new List<Task>();
            var ii = 0;
            var updateCountdown = databaseRecordsToUpdateInSingleQuery;
            while (ii < replayCharacters.Count)
            {
                var scannedReplayId = replayCharacters[ii].replayId;
                var jj = 0;
                while (ii < replayCharacters.Count && replayCharacters[ii].replayId == scannedReplayId)
                {
                    if (jj < 10)
                    {
                        replayCharactersInGame[jj] = replayCharacters[ii];
                        replayPlayersInGame[jj] = players[replayCharactersInGame[jj].playerId];
                    }

                    jj++;
                    ii++;
                }

                if (jj != 10)
                {
                    Log($"Bad replay found with {jj} characters. Replay Id {scannedReplayId}");
                }
                else
                {
                    for (var j = 0; j < 10; j++)
                    {
                        if (replayPlayersInGame[j].rating == null)
                        {
                            replayPlayersInGame[j] = (replayPlayersInGame[j].player,
                                GameInfo.DefaultGameInfo.DefaultRating);
                            players[(int)replayPlayersInGame[j].player.Id] = replayPlayersInGame[j];
                        }
                    }

                    var winningTeam = new Team<Player>();
                    for (var j = 0; j < 5; j++)
                    {
                        winningTeam = winningTeam.AddPlayer(
                            replayPlayersInGame[j].player,
                            replayPlayersInGame[j].rating);
                    }

                    var losingTeam = new Team<Player>();
                    for (var j = 5; j < 10; j++)
                    {
                        losingTeam = losingTeam.AddPlayer(replayPlayersInGame[j].player, replayPlayersInGame[j].rating);
                    }

                    foreach (var newRating in TrueSkillCalculator.CalculateNewRatings(
                                 GameInfo.DefaultGameInfo,
                                 Teams.Concat(winningTeam, losingTeam),
                                 1,
                                 2))
                    {
                        var (player, rating) = players[(int)newRating.Key.Id];

                        /*
                         * 5/20/2020 8:50:07 AM: 5/20/2020:8:50:07 AM:Aggregating MMR region 2 mode 8 date 11/19/2019
                         * 5/20/2020 8:52:32 AM: 5/20/2020:8:52:32 AM:While processing for mode 8 on
                         * 5/20/2020 8:52:36 AM: 5/20/2020:8:52:36 AM:Error:System.InvalidOperationException: Sequence contains more than one matching element
                         *    at System.Linq.Enumerable.Single[TSource](IEnumerable`1 source, Func`2 predicate)
                         *    at Helper.MMR.AggregateReplayCharacterMMR(Int32 battleNetRegionId, Int32 gameMode, DateTime replayDate, Boolean dryRun) in D:\StarKillerLLC\HOTSlogs\DataHelper\MMR.cs:line 945
                         *    at HOTSAPIParser.Services.MMRProcessOlderDates.<Run>b__0_3(<>f__AnonymousType0`2 threadData) in D:\StarKillerLLC\HOTSlogs\HOTSAPIParser\Services\MMRProcessOlderDates.cs:line 55
                         */
                        var cnt = replayCharactersInGame.Count(x => x.playerId == (int)player.Id);
                        if (cnt != 1)
                        {
                            Log(
                                $"Bug when processing replay {scannedReplayId} player id {player.Id} occurs {cnt} times, skipping player.");
                            continue;
                        }

                        var (replayId, playerId, isWinner) =
                            replayCharactersInGame.Single(x => x.playerId == (int)player.Id);

                        var mmrBefore =
                            (int)((rating.Mean -
                                   (rating.StandardDeviation * conservativeStandardDeviationMultiplier)) * 100);

                        // Some ridiculous matchups lead to 'NaN' being calculated, such as when one team is ~4200 MMR and the other ~1500 MMR
                        // Also, if winning results in -1 MMR change, we will instead discard the match
                        int mmrChange;
                        if (!double.IsNaN(newRating.Value.Mean))
                        {
                            mmrChange = (int)((newRating.Value.Mean -
                                               (newRating.Value.StandardDeviation *
                                                conservativeStandardDeviationMultiplier)) * 100) - mmrBefore;
                            if (isWinner && mmrChange < 0)
                            {
                                mmrChange = 0;
                            }
                            else
                            {
                                players[(int)newRating.Key.Id] = (newRating.Key, newRating.Value);
                            }
                        }
                        else
                        {
                            mmrChange = 0;
                        }

                        // Create the update query
                        sqlQueryUpdate += "update ReplayCharacter set MMRBefore=" + mmrBefore + ",MMRChange=" +
                                          mmrChange + " where ReplayID=" + replayId + " and PlayerID=" + playerId +
                                          ";";
                        updateCountdown--;
                    }
                }

                if (updateCountdown == 0 || ii == replayCharacters.Count)
                {
                    updateCountdown = databaseRecordsToUpdateInSingleQuery;
                    var commandText = sqlQueryUpdate;
                    sqlUpdateTaskList.Add(
                        Task.Run(
                            async () =>
                            {
                                await ExecuteMySqlCommandNonQueryAsync(commandText, dryRun, token: token);
                            },
                            token));
                    sqlQueryUpdate = string.Empty;
                    Log("Calculated ReplayCharacter " + ii + " of " + replayCharacters.Count, debug: true);

                    if (sqlUpdateTaskList.Count >= numberOfDatabaseUpdateStatementsToRunConcurrently ||
                        ii == replayCharacters.Count)
                    {
                        await Task.WhenAll(sqlUpdateTaskList);
                        sqlUpdateTaskList.Clear();
                    }
                }
            }

            Log("Finished Processing ReplayCharacters", debug: true);

            // Calculate Milestone MMR for Each Player
            {
                sqlQueryUpdate = string.Empty;
                var i = 0;
                foreach (var player in players)
                {
                    if (player.Value.rating == null)
                    {
                        Log($"Player {player.Value.player.Id} has no previous MMR rating");
                        continue;
                    }

                    var mmrRating = (int)((player.Value.rating.Mean -
                                           (player.Value.rating.StandardDeviation *
                                            conservativeStandardDeviationMultiplier)) * 100);
                    sqlQueryUpdate +=
                        $@"
insert into PlayerMMRMilestoneV3
values({player.Key},{gameMode},'{replayDateString}',{player.Value.rating.Mean},{player.Value.rating.StandardDeviation},{mmrRating})
on duplicate key update MMRMean={player.Value.rating.Mean},MMRStandardDeviation={player.Value.rating.StandardDeviation},MMRRating={mmrRating};
";

                    if (i++ % databaseRecordsToUpdateInSingleQuery == 0 || i == players.Count)
                    {
                        var commandText = sqlQueryUpdate;
                        sqlUpdateTaskList.Add(
                            Task.Run(
                                async () =>
                                {
                                    await ExecuteMySqlCommandNonQueryAsync(commandText, dryRun, token: token);
                                },
                                token));
                        sqlQueryUpdate = string.Empty;
                        Log("Saved Player " + i + " of " + players.Count, debug: true);

                        if (sqlUpdateTaskList.Count >= numberOfDatabaseUpdateStatementsToRunConcurrently ||
                            i == players.Count)
                        {
                            await Task.WhenAll(sqlUpdateTaskList);
                            sqlUpdateTaskList.Clear();
                        }
                    }
                }
            }
        }

        // Initialize all MMR values for entries skipped to 1000
        var replayCharactersSkipped = await heroesEntity.ReplayCharacters
            .Include(r => r.Replay)
            .Include(r => r.Player)
            .Where(r => r.Replay.GameMode == gameMode && r.Player.BattleNetRegionId == battleNetRegionId)
            .Where(x => x.Replay.TimestampReplay >= replayDate && x.Replay.TimestampReplay < replayDate.AddDays(1))
            .Where(x => !x.Mmrbefore.HasValue || !x.Mmrchange.HasValue)
            .ToListAsync(cancellationToken: token);

        var num = replayCharactersSkipped.Count;
        if (num > 0)
        {
            Log($"Initializing MMR of {num} replay character entries region {battleNetRegionId} mode {gameMode}", debug: true);

            replayCharactersSkipped.ForEach(x => x.Mmrchange ??= 0);
            replayCharactersSkipped.ForEach(x => x.Mmrbefore ??= 1000);

            await heroesEntity.SaveChangesAsync(token);
        }

        // Set Task Completed
        var dataEvent = "AggregateReplayCharacterMMR Region: " + battleNetRegionId + ", GameMode: " + gameMode +
                        ", Date: " + replayDateString;
        if (heroesEntity.DataUpdates.Any(i => i.DataEvent == dataEvent))
        {
            heroesEntity.DataUpdates.Single(i => i.DataEvent == dataEvent).LastUpdated = Now;
        }
        else
        {
            await heroesEntity.DataUpdates.AddAsync(
                new DataUpdate
                {
                    DataEvent = dataEvent,
                    LastUpdated = Now,
                },
                token);
        }

        if (!dryRun)
        {
            await heroesEntity.SaveChangesAsync(token);
        }

        Log($"Done for region {battleNetRegionId} mode {gameMode} date {replayDate}!", debug: true);
    }

    private void ExecuteMySqlCommandNonQuery(string query, bool dryRun, int commandTimeout = LongCommandTimeout)
    {
        if (dryRun)
        {
            return;
        }

        using var mySqlConnection =
            new MySqlConnection(_connectionString);
        mySqlConnection.Open();

        using var mySqlCommand = new MySqlCommand(query, mySqlConnection) { CommandTimeout = commandTimeout };
        mySqlCommand.ExecuteNonQuery();
    }

    private async Task ExecuteMySqlCommandNonQueryAsync(
        string query,
        bool dryRun,
        int commandTimeout = LongCommandTimeout,
        CancellationToken token = default)
    {
        if (dryRun)
        {
            return;
        }

        await using var mySqlConnection =
            new MySqlConnection(_connectionString);
        await mySqlConnection.OpenAsync(token);

        await using var mySqlCommand = new MySqlCommand(query, mySqlConnection) { CommandTimeout = commandTimeout };
        await mySqlCommand.ExecuteNonQueryAsync(token);
    }
}
