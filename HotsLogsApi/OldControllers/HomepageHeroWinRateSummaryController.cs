using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.ReplayParser;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class HomepageHeroWinRateSummaryController : ControllerBase
{
    private readonly string _connectionString;

    public HomepageHeroWinRateSummaryController(IConfiguration config)
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

            // Storm League - Master (300+ games played)
            IEnumerable<WinRateSummaryData> heroLeagueMasterData = null;
            var sitewideCharacterStatistics = DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                "HOTSLogs:SitewideCharacterStatisticsV2:CurrentBuild:0:" + (int)GameMode.StormLeague);
            if (sitewideCharacterStatistics != null)
            {
                var characterStatisticsDataSource = sitewideCharacterStatistics.SitewideCharacterStatisticArray
                    .Where(i => i.GamesPlayed >= 100).OrderByDescending(i => i.WinPercent).Take(9).ToArray();

                if (characterStatisticsDataSource.Length != 0)
                {
                    heroLeagueMasterData = characterStatisticsDataSource.Select(
                        i => new WinRateSummaryData
                        {
                            hero = i.Character,
                            icon = heroIcons[i.Character],
                            win_percent = i.WinPercent,
                        });
                }
            }

            // Storm League
            IEnumerable<WinRateSummaryData> heroLeagueData = null;
            sitewideCharacterStatistics = DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                "HOTSLogs:SitewideCharacterStatisticsV2:CurrentBuild:-1:" + (int)GameMode.StormLeague);
            if (sitewideCharacterStatistics != null)
            {
                var characterStatisticsDataSource = sitewideCharacterStatistics.SitewideCharacterStatisticArray
                    .Where(i => i.GamesPlayed >= 100).OrderByDescending(i => i.WinPercent).Take(9).ToArray();

                if (characterStatisticsDataSource.Length != 0)
                {
                    heroLeagueData = characterStatisticsDataSource.Select(
                        i => new WinRateSummaryData
                        {
                            hero = i.Character,
                            icon = heroIcons[i.Character],
                            win_percent = i.WinPercent,
                        });
                }
            }

            // All Data
            var sitewideCharacterStatistics3 = DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                "HOTSLogs:SitewideCharacterStatisticsV2:CurrentBuild:-1:" + (int)GameMode.QuickMatch);
            var sitewideCharacterStatistics4 = DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                "HOTSLogs:SitewideCharacterStatisticsV2:CurrentBuild:-1:" + (int)GameMode.HeroLeague);
            var sitewideCharacterStatistics5 = DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                "HOTSLogs:SitewideCharacterStatisticsV2:CurrentBuild:-1:" + (int)GameMode.TeamLeague);
            var sitewideCharacterStatistics6 = DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                "HOTSLogs:SitewideCharacterStatisticsV2:CurrentBuild:-1:" + (int)GameMode.UnrankedDraft);
            var sitewideCharacterStatistics8 = DataHelper.RedisCacheGet<SitewideCharacterStatistics>(
                "HOTSLogs:SitewideCharacterStatisticsV2:CurrentBuild:-1:" + (int)GameMode.StormLeague);
            var heroWins = new HeroWins[sitewideCharacterStatistics3.SitewideCharacterStatisticArray.Length];
            if (sitewideCharacterStatistics3 != null)
            {
                for (var i = 0; i < sitewideCharacterStatistics3.SitewideCharacterStatisticArray.Length; i++)
                {
                    var wins = (int)(sitewideCharacterStatistics3.SitewideCharacterStatisticArray[i].GamesPlayed *
                                     sitewideCharacterStatistics3.SitewideCharacterStatisticArray[i].WinPercent);
                    heroWins[i] = new HeroWins
                    {
                        games = sitewideCharacterStatistics3.SitewideCharacterStatisticArray[i].GamesPlayed,
                        wins = wins,
                        hero = sitewideCharacterStatistics3.SitewideCharacterStatisticArray[i].Character,
                    };
                }

                for (var i = 0; i < sitewideCharacterStatistics4.SitewideCharacterStatisticArray.Length; i++)
                {
                    heroWins[i].games = sitewideCharacterStatistics4.SitewideCharacterStatisticArray[i].GamesPlayed;
                    heroWins[i].wins =
                        (int)(sitewideCharacterStatistics4.SitewideCharacterStatisticArray[i].GamesPlayed *
                              sitewideCharacterStatistics4.SitewideCharacterStatisticArray[i].WinPercent);
                    heroWins[i].hero = sitewideCharacterStatistics4.SitewideCharacterStatisticArray[i].Character;
                }

                for (var i = 0; i < sitewideCharacterStatistics5.SitewideCharacterStatisticArray.Length; i++)
                {
                    heroWins[i].games = sitewideCharacterStatistics5.SitewideCharacterStatisticArray[i].GamesPlayed;
                    heroWins[i].wins =
                        (int)(sitewideCharacterStatistics5.SitewideCharacterStatisticArray[i].GamesPlayed *
                              sitewideCharacterStatistics5.SitewideCharacterStatisticArray[i].WinPercent);
                    heroWins[i].hero = sitewideCharacterStatistics5.SitewideCharacterStatisticArray[i].Character;
                }

                for (var i = 0; i < sitewideCharacterStatistics6.SitewideCharacterStatisticArray.Length; i++)
                {
                    heroWins[i].games = sitewideCharacterStatistics6.SitewideCharacterStatisticArray[i].GamesPlayed;
                    heroWins[i].wins =
                        (int)(sitewideCharacterStatistics6.SitewideCharacterStatisticArray[i].GamesPlayed *
                              sitewideCharacterStatistics6.SitewideCharacterStatisticArray[i].WinPercent);
                    heroWins[i].hero = sitewideCharacterStatistics6.SitewideCharacterStatisticArray[i].Character;
                }

                for (var i = 0; i < sitewideCharacterStatistics8.SitewideCharacterStatisticArray.Length; i++)
                {
                    heroWins[i].games = sitewideCharacterStatistics8.SitewideCharacterStatisticArray[i].GamesPlayed;
                    heroWins[i].wins =
                        (int)(sitewideCharacterStatistics8.SitewideCharacterStatisticArray[i].GamesPlayed *
                              sitewideCharacterStatistics8.SitewideCharacterStatisticArray[i].WinPercent);
                    heroWins[i].hero = sitewideCharacterStatistics8.SitewideCharacterStatisticArray[i].Character;
                }
            }

            for (var i = 0; i < heroWins.Length; i++)
            {
                heroWins[i].win_percent = (decimal)heroWins[i].wins / heroWins[i].games;
            }

            var dataSource = heroWins.Where(i => i.games >= 100).OrderByDescending(i => i.win_percent).Take(9)
                .ToArray();

            IEnumerable<WinRateSummaryData> allData = null;
            if (dataSource.Length != 0)
            {
                allData = dataSource.Select(
                    i => new WinRateSummaryData
                    {
                        hero = i.hero,
                        icon = heroIcons[i.hero],
                        win_percent = i.win_percent,
                    });
            }

            return new
            {
                hero_league = heroLeagueData,
                hero_league_master = heroLeagueMasterData,
                all = allData,
            };
        }
        catch (Exception e)
        {
            return new
            {
                error = e.ToString(),
            };
        }
    }
}

public class WinRateSummaryData
{
    public string hero { get; set; }
    public string icon { get; set; }
    public decimal win_percent { get; set; }
}

public class HeroWins
{
    public string hero { get; set; }
    public int games { get; set; }
    public int wins { get; set; }
    public decimal win_percent { get; set; }
}
