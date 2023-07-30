using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class HomepageRecentUploadsController : ControllerBase
{
    private readonly string _connectionString;

    public HomepageRecentUploadsController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    [HttpGet]
    public object GetDataByString()
    {
        try
        {
            long count = 0;
            using (var mySqlConnection = new MySqlConnection(_connectionString))
            {
                mySqlConnection.Open();

                using (var mySqlCommand = new MySqlCommand(
                           @"select count(*) as count from Replay where TimestampCreated >= DATE(NOW()) - INTERVAL 7 DAY",
                           mySqlConnection))
                {
                    using (var mySqlDataReader = mySqlCommand.ExecuteReader())
                    {
                        if (mySqlDataReader.Read())
                        {
                            count = mySqlDataReader.GetInt64("Count");
                        }
                    }
                }
            }

            var heroIcons = new Dictionary<int, string>();
            var heroNames = new Dictionary<int, string>();
            using (var mySqlConnection = new MySqlConnection(_connectionString))
            {
                mySqlConnection.Open();

                using (var mySqlCommand = new MySqlCommand(
                           @"select la.IdentifierID as Id, Name, Icon from HeroIconInformation hii join LocalizationAlias la on hii.Name = la.PrimaryName",
                           mySqlConnection))
                {
                    using (var mySqlDataReader = mySqlCommand.ExecuteReader())
                    {
                        while (mySqlDataReader.Read())
                        {
                            var id = mySqlDataReader.GetInt32("Id");
                            heroIcons.Add(id, mySqlDataReader["Icon"].ToString());
                            heroNames.Add(id, mySqlDataReader["Name"].ToString());
                        }
                    }
                }
            }

            var list = new List<ReplayData>();
            using (var mySqlConnection = new MySqlConnection(_connectionString))
            {
                mySqlConnection.Open();

                using (var mySqlCommand = new MySqlCommand(
                           @"SELECT r.ReplayID, rc.CharacterLevel, rc.CharacterID, rc.IsWinner+0 as IsWinner FROM Replay r " +
                           "join ReplayCharacter rc on r.ReplayID = rc.ReplayID " +
                           "ORDER BY r.ReplayID desc LIMIT 50",
                           mySqlConnection))
                {
                    using (var mySqlDataReader = mySqlCommand.ExecuteReader())
                    {
                        while (mySqlDataReader.Read())
                        {
                            var id = mySqlDataReader.GetInt32("ReplayID");
                            var level = mySqlDataReader.GetInt32("CharacterLevel");
                            var characterId = mySqlDataReader.GetInt32("CharacterID");
                            var winner = long.Parse(mySqlDataReader["IsWinner"].ToString()) == 1;

                            list.Add(
                                new ReplayData
                                {
                                    Id = id,
                                    Level = level,
                                    CharacterId = characterId,
                                    Winner = winner,
                                });
                        }
                    }
                }
            }

            var replays = new List<ReplayResultContainer>();

            var winnerRows = new List<ReplayResult>();
            var loserRows = new List<ReplayResult>();
            var winnerFirstRow = false;

            for (var i = 1; i <= list.Count; i++)
            {
                var row = list[i - 1];
                if (i % 10 == 1 && row.Winner)
                {
                    winnerFirstRow = true;
                }

                var data = new ReplayResult
                {
                    hero = heroNames[row.CharacterId],
                    icon = heroIcons[row.CharacterId],
                    level = row.Level,
                };
                if (row.Winner)
                {
                    winnerRows.Add(data);
                }
                else
                {
                    loserRows.Add(data);
                }

                if (i % 10 == 0)
                {
                    var teamA = winnerFirstRow ? winnerRows : loserRows;
                    var teamB = winnerFirstRow ? loserRows : winnerRows;
                    replays.Add(
                        new ReplayResultContainer
                        {
                            replay_id = row.Id,
                            team_a = teamA,
                            team_a_winner = teamA == winnerRows,
                            team_b = teamB,
                            team_b_winner = teamB == winnerRows,
                        });
                    winnerRows = new List<ReplayResult>();
                    loserRows = new List<ReplayResult>();
                    winnerFirstRow = false;
                }
            }

            return new ReplayResultJsonResult
            {
                replays = replays,
                count = count,
            };
        }
        catch (Exception e)
        {
            return new ReplayResultJsonResultError
            {
                error = e.ToString(),
            };
        }
    }
}

public class ReplayData
{
    public int Id { get; set; }
    public int Level { get; set; }
    public int CharacterId { get; set; }
    public bool Winner { get; set; }
}

public class ReplayResult
{
    public string hero { get; set; }
    public string icon { get; set; }
    public int level { get; set; }
}

public class ReplayResultContainer
{
    public int replay_id { get; set; }
    public List<ReplayResult> team_a { get; set; }
    public bool team_a_winner { get; set; }
    public List<ReplayResult> team_b { get; set; }
    public bool team_b_winner { get; set; }
}

public class ReplayResultJsonResult
{
    public long count { get; set; }
    public List<ReplayResultContainer> replays { get; set; }
}

public class ReplayResultJsonResultError
{
    public string error { get; set; }
}
