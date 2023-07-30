using HelperCore;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using ServiceStackReplacement;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Team Draft", KeepRunning = true, Sort = 11, AutoStart = false, Port = 17034)]
public class TeamDraft : ServiceBase
{
    private const decimal DaysToCache = 4m;

    private static LocalizationAlias[] _localizationAliases;

    private static ConcurrentDictionary<int,
        ConcurrentDictionary<int,
            ConcurrentDictionary<sbyte,
                ConcurrentDictionary<sbyte, List<sbyte[][]>[]>>>> _stats;

    private CancellationToken _token;
    private readonly string _connectionString;

    public TeamDraft(IServiceProvider svcp) : base(svcp)
    {
        var config = svcp.GetRequiredService<IConfiguration>();
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    protected override async Task RunOnce(CancellationToken token = default)
    {
        await using var scope = Svcp.CreateAsyncScope();
        _token = token;
        await CacheForGameModeMapAndHero(scope);
    }

    private static LocalizationAlias[] GetLocalizationAlias(IServiceScope scope)
    {
        if (_localizationAliases != null)
        {
            return _localizationAliases;
        }

        var heroesEntity = HeroesdataContext.Create(scope);
        _localizationAliases = heroesEntity.LocalizationAliases.Where(
            i =>
                i.Type == (int)DataHelper.LocalizationAliasType.Hero ||
                i.Type == (int)DataHelper.LocalizationAliasType.Map).ToArray();

        return _localizationAliases;
    }

    private void CacheForGameModeAndMap(
        int gameMode,
        int mapId,
        ConcurrentDictionary<sbyte, ConcurrentDictionary<sbyte, List<sbyte[][]>[]>> recent)
    {
        if (gameMode == -1 && mapId == -1)
        {
            return;
        }

        // We are done adding data - explicitly set the list capacity to reduce memory usage
        foreach (var outerHeroIndex in recent.Keys)
        {
            foreach (var innerHeroIndex in recent[outerHeroIndex].Keys)
            {
                for (var i = 0; i < recent[outerHeroIndex][innerHeroIndex].Length; i++)
                {
                    recent[outerHeroIndex][innerHeroIndex][i].Capacity =
                        recent[outerHeroIndex][innerHeroIndex][i].Count;
                }
            }
        }

        // Overwrite the master data
        _stats[gameMode][mapId] = recent;
    }

    private async Task CacheForGameModeMapAndHero(IServiceScope scope)
    {
        var gameModes = DataHelper.GameModeWithMMR;

        const string query =
            @"select r.ReplayID, r.GameMode, r.MapID, rc.CharacterID, rc.IsWinner, coalesce(lr.LeagueID, -1) as LeagueID
                from Replay r
                join ReplayCharacter rc on rc.ReplayID = r.ReplayID
                join Player p on p.PlayerID = rc.PlayerID
                left join LeaderboardRanking lr on lr.PlayerID = p.PlayerID and lr.GameMode = r.GameMode
                where r.GameMode in ({1}) and r.TimestampReplay > date_add(now(), interval {0} day) and r.MapID != 0
                order by r.GameMode, r.MapID, r.ReplayID asc";

        const string countQuery =
            @"select count(r.ReplayID) cnt
                from Replay r
                join ReplayCharacter rc on rc.ReplayID = r.ReplayID
                join Player p on p.PlayerID = rc.PlayerID
                left join LeaderboardRanking lr on lr.PlayerID = p.PlayerID and lr.GameMode = r.GameMode
                where r.GameMode in ({1}) and r.TimestampReplay > date_add(now(), interval {0} day) and r.MapID != 0";

        _stats ??= new ConcurrentDictionary<int,
            ConcurrentDictionary<int,
                ConcurrentDictionary<sbyte,
                    ConcurrentDictionary<sbyte, List<sbyte[][]>[]>>>>();

        // Create or update dictionaries for GameMode / MapID / CharacterID
        // Include three numbers higher for CharacterID and MapID, so our dictionaries
        // won't fail if a new Hero or Map is added during our database query
        var characterIds = GetLocalizationAlias(scope)
            .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero)
            .Select(i => (sbyte)i.IdentifierId)
            .ToList();
        for (var i = 0; i < 3; i++)
        {
            characterIds.Add((sbyte)(characterIds.Max() + 1));
        }

        var mapIds = GetLocalizationAlias(scope)
            .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map)
            .Select(i => i.IdentifierId)
            .ToList();
        for (var i = 0; i < 3; i++)
        {
            mapIds.Add(mapIds.Max() + 1);
        }

        foreach (var gameMode in gameModes)
        {
            AddServiceOutput($"Processing game mode {gameMode} {Prog(gameModes, gameMode)}");
            if (!_stats.ContainsKey(gameMode))
            {
                _stats[gameMode] =
                    new ConcurrentDictionary<int,
                        ConcurrentDictionary<sbyte,
                            ConcurrentDictionary<sbyte, List<sbyte[][]>[]>>>();
            }

            foreach (var mapId in mapIds)
            {
                if (!_stats[gameMode].ContainsKey(mapId))
                {
                    _stats[gameMode][mapId] =
                        new ConcurrentDictionary<sbyte, ConcurrentDictionary<sbyte, List<sbyte[][]>[]>>();
                }

                foreach (var heroIndexOuter in characterIds)
                {
                    if (!_stats[gameMode][mapId].ContainsKey(heroIndexOuter))
                    {
                        _stats[gameMode][mapId][heroIndexOuter] = new ConcurrentDictionary<sbyte, List<sbyte[][]>[]>();
                    }

                    foreach (var heroIndexInner in characterIds)
                    {
                        if (!_stats[gameMode][mapId][heroIndexOuter].ContainsKey(heroIndexInner))
                        {
                            _stats[gameMode][mapId][heroIndexOuter][heroIndexInner] = new List<sbyte[][]>[2];

                            for (var i = 0; i < _stats[gameMode][mapId][heroIndexOuter][heroIndexInner].Length; i++)
                            {
                                _stats[gameMode][mapId][heroIndexOuter][heroIndexInner][i] = new List<sbyte[][]>();
                            }
                        }
                    }
                }
            }
        }

        await using (var mySqlConnection = new MySqlConnection(_connectionString))
        {
            await mySqlConnection.OpenAsync(_token);

            // Set up counters and containers for the current set of data being processed
            ConcurrentDictionary<sbyte, ConcurrentDictionary<sbyte, List<sbyte[][]>[]>> recent = null;
            var characters = new sbyte[2][][];
            var currentIndex = new byte[2];
            var currentReplayId = -1;
            var gameMode = -1;
            var mapId = -1;

            AddServiceOutput("Running count query");
            var cmdTextCount = string.Format(countQuery, -DaysToCache, string.Join(",", gameModes));
            long count;
            await using (var mySqlCommand = new MySqlCommand(cmdTextCount, mySqlConnection)
            {
                CommandTimeout = MMR.LongCommandTimeout,
            })
            {
                var scalar = await mySqlCommand.ExecuteScalarAsync(_token);
                count = (long)scalar;
                AddServiceOutput($"Will process {count} rows");
            }

            var row = 0;
            AddServiceOutput("Running query");
            var cmdText = string.Format(query, -DaysToCache, string.Join(",", gameModes));
            await using (var mySqlCommand = new MySqlCommand(cmdText, mySqlConnection)
            {
                CommandTimeout = MMR.LongCommandTimeout,
            })
            await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(_token))
            {
                while (await mySqlDataReader.ReadAsync(_token))
                {
                    row++;
                    AddServiceOutput($"Reading query results {row}-{row + 9}/{count}");
                    // Clear temporary containers
                    for (var i = 0; i < characters.Length; i++)
                    {
                        characters[i] = new sbyte[5][];
                        for (var j = 0; j < characters[i].Length; j++)
                        {
                            characters[i][j] = new sbyte[2];
                        }
                    }

                    currentIndex[0] = 0;
                    currentIndex[1] = 0;
                    var isBadData = false;

                    // We've finished reading a GameMode/MapID combination - overwrite the old data to free up memory
                    if (mySqlDataReader.GetInt32("GameMode") != gameMode || mySqlDataReader.GetInt32("MapID") != mapId)
                    {
                        CacheForGameModeAndMap(gameMode, mapId, recent);

                        gameMode = mySqlDataReader.GetInt32("GameMode");
                        mapId = mySqlDataReader.GetInt32("MapID");

                        recent = new ConcurrentDictionary<sbyte, ConcurrentDictionary<sbyte, List<sbyte[][]>[]>>();
                        foreach (var heroIndexOuter in characterIds)
                        {
                            recent[heroIndexOuter] = new ConcurrentDictionary<sbyte, List<sbyte[][]>[]>();

                            foreach (var heroIndexInner in characterIds)
                            {
                                recent[heroIndexOuter][heroIndexInner] = new List<sbyte[][]>[2];

                                for (var i = 0; i < recent[heroIndexOuter][heroIndexInner].Length; i++)
                                {
                                    recent[heroIndexOuter][heroIndexInner][i] = new List<sbyte[][]>();
                                }
                            }
                        }
                    }

                    // Read in 10 ReplayCharacters at once
                    for (var i = 0; i <= 9; i++)
                    {
                        if (i != 0)
                        {
                            row++;
                        }

                        // Make sure all ReplayCharacters belong to one ReplayID
                        if (i == 0)
                        {
                            currentReplayId = mySqlDataReader.GetInt32("ReplayID");
                        }
                        else if (mySqlDataReader.GetInt32("ReplayID") != currentReplayId)
                        {
                            i = -1;
                            currentIndex[0] = 0;
                            currentIndex[1] = 0;
                            continue;
                        }

                        // Check for an invalid Hero or Map name
                        // This should be rare, and only apply to new Hero names in different languages that have not yet been reimported
                        if (mySqlDataReader.GetInt32("CharacterID") == 0)
                        {
                            isBadData = true;
                        }

                        // Read CharacterID and LeagueID
                        if (!isBadData)
                        {
                            var isWinner = mySqlDataReader.GetUInt64("IsWinner");
                            characters[isWinner][currentIndex[isWinner]][0] =
                                (sbyte)mySqlDataReader.GetInt32("CharacterID");
                            characters[isWinner][currentIndex[isWinner]++][1] =
                                (sbyte)mySqlDataReader.GetInt64("LeagueID");
                        }

                        // Keep reading until we have all 10 ReplayCharacters for this match
                        if (i < 9 && !await mySqlDataReader.ReadAsync(_token))
                        {
                            isBadData = true;
                            break;
                        }
                    }

                    if (isBadData)
                    {
                        AddServiceOutput("Bad data");
                        continue;
                    }

                    // Order the results by CharacterID
                    characters[0] = characters[0].OrderBy(i => i[0]).ToArray();
                    characters[1] = characters[1].OrderBy(i => i[0]).ToArray();

                    // Apply the data to the appropriate single hero dictionaries
                    for (sbyte i = 0; i < 5; i++)
                    {
                        // Add current data to the Map / Hero dictionary for the current Map and each losing Hero
                        recent[characters[0][i][0]][characters[0][i][0]][0].Add(characters[0]);

                        // Add current data to the Map / Hero dictionary for the current Map and each winning Hero
                        recent[characters[1][i][0]][characters[1][i][0]][1].Add(characters[1]);
                    }

                    // Apply the data to the appropriate two hero dictionaries
                    for (sbyte i = 0; i < 4; i++)
                    {
                        for (var j = (sbyte)(i + 1); j < 5; j++)
                        {
                            // Add current data to the Map / Hero dictionary for the current Map and each losing Hero
                            recent[characters[0][i][0]][characters[0][j][0]][0].Add(characters[0]);

                            // Add current data to the Map / Hero dictionary for the current Map and each winning Hero
                            recent[characters[1][i][0]][characters[1][j][0]][1].Add(characters[1]);
                        }
                    }
                }
            }

            CacheForGameModeAndMap(gameMode, mapId, recent);
        }

        var redisClient = MyDbWrapper.Create(scope);
        redisClient.TrySet("HOTSLogs:TeamDraft:LastUpdated", DateTime.UtcNow);
    }

    private string Prog<T>(IEnumerable<T> arr, T el)
    {
        var cnt = 0;
        var idx = 0;
        foreach (var t in arr)
        {
            cnt++;
            if (Equals(t, el))
            {
                idx = cnt;
            }
        }

        return $"{idx}/{cnt}";
    }
}
