using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class HomepageHeroDataController : ControllerBase
{
    private readonly string _connectionString;

    public HomepageHeroDataController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    [HttpGet]
    public object GetDataByString()
    {
        try
        {
            var heroIcons = new Dictionary<string, string>();
            using (var mySqlConnection = new MySqlConnection(_connectionString))
            {
                mySqlConnection.Open();

                using (var mySqlCommand = new MySqlCommand(
                           @"select Name, Icon from HeroIconInformation",
                           mySqlConnection))
                {
                    using (var mySqlDataReader = mySqlCommand.ExecuteReader())
                    {
                        while (mySqlDataReader.Read())
                        {
                            heroIcons.Add(mySqlDataReader["Name"].ToString(), mySqlDataReader["Icon"].ToString());
                        }
                    }
                }
            }

            var sitewideCharacterStatistics = DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                "HOTSLogs:SitewideCharacterStatisticsV2:Current:-1:" + (int)GameMode.StormLeague);
            if (sitewideCharacterStatistics != null)
            {
                var previousWeekDateTimeString = DateTime.UtcNow.StartOfWeek(DayOfWeek.Sunday).AddDays(-7)
                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var sitewideCharacterStatisticsPreviousWeek = DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                    "HOTSLogs:SitewideCharacterStatisticsV2:" + previousWeekDateTimeString + ":-1:" +
                    (int)GameMode.StormLeague);
                Dictionary<string, decimal> sitewideCharacterStatisticsPreviousWeekDictionary = null;
                if (sitewideCharacterStatisticsPreviousWeek != null)
                {
                    sitewideCharacterStatisticsPreviousWeekDictionary = sitewideCharacterStatisticsPreviousWeek
                        .SitewideCharacterStatisticArray.Where(i => i.GamesPlayed > 10)
                        .ToDictionary(i => i.Character, i => i.WinPercent);
                }

                var heroRoleConcurrentDictionary = Global.GetHeroRoleConcurrentDictionary();

                var characterStatisticsDataSource = sitewideCharacterStatistics.SitewideCharacterStatisticArray
                    .Where(i => i.GamesPlayed >= 100).OrderByDescending(i => i.WinPercent).ToArray();

                if (characterStatisticsDataSource.Length != 0)
                {
                    return characterStatisticsDataSource.Select(
                        i => new
                        {
                            hero = i.Character,
                            icon = heroIcons[i.Character],
                            games_played = i.GamesPlayed,
                            games_banned = i.GamesBanned,
                            popularity =
                                (decimal)((i.GamesPlayed + i.GamesBanned) * 13.875) /
                                sitewideCharacterStatistics.SitewideCharacterStatisticArray.Sum(
                                    j =>
                                        j.GamesPlayed + j.GamesBanned),
                            win_percent = i.WinPercent,
                            win_percent_delta =
                                sitewideCharacterStatisticsPreviousWeekDictionary != null &&
                                sitewideCharacterStatisticsPreviousWeekDictionary.ContainsKey(i.Character)
                                    ? (decimal?)i.WinPercent -
                                      sitewideCharacterStatisticsPreviousWeekDictionary[i.Character]
                                    : null,
                            role = heroRoleConcurrentDictionary.ContainsKey(i.Character)
                                ? heroRoleConcurrentDictionary[i.Character]
                                : null,
                        });
                }
            }
        }
        catch (Exception e)
        {
            return new
            {
                error = e.ToString(),
            };
        }

        return new
        {
            error = "no data",
        };
    }
}
