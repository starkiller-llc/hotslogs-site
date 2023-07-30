// Copyright (c) StarkillerLLC. All rights reserved.

using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Heroes.ReplayParser;
using Heroes.ReplayParser.MPQFiles;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using ServiceStackReplacement;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Logger = HotsLogs.Logging.Manager;
using Message = Amazon.SimpleEmail.Model.Message;
using Replay = Heroes.ReplayParser.Replay;

// ReSharper disable HeuristicUnreachableCode
#pragma warning disable SA1124 // Do not use regions

namespace HelperCore;

public static partial class DataHelper
{
#if !LOCALDEBUG
    public static DateTime Now => DateTime.UtcNow;
#else
    public static DateTime Now => new(2022, 5, 3, 12, 0, 0, DateTimeKind.Utc);
#endif

    // Renaming these will modify almost every file in the solution...
    // ...I'll do it later
    // ReSharper disable InconsistentNaming
    public const string LogDirectory = @"C:\HOTSLogs";
    public static string AWSAccessKeyID;
    public static string AWSSecretAccessKey;

    // ReSharper restore InconsistentNaming

    public static readonly Dictionary<int, string> BattleNetRegionNames =
        new()
        {
            { 1, "US Region" },
            { 2, "EU Region" },
            { 3, "KR Region" },
            { 5, "CN Region" },
        };

    public static readonly int[] GameModeWithMMR =
    {
        (int)GameMode.StormLeague, (int)GameMode.UnrankedDraft, (int)GameMode.QuickMatch,
    };

    public static readonly int[] GameModeWithStatistics =
    {
        (int)GameMode.StormLeague, (int)GameMode.UnrankedDraft, (int)GameMode.QuickMatch, (int)GameMode.ARAM,
    };

    public static readonly string[] HeroRoles =
    {
        "Tank", "Bruiser", "Ranged Assassin", "Melee Assassin", "Healer", "Support",
    };

    private static readonly string BadChars = Regex.Escape("- '‘’:,!.\"”?()");

    public static (string code, string text) GetAwardIcon(MatchAwardType awardType) =>
        awardType switch
        {
            MatchAwardType.MVP => ("MVP", "MVP"),
            MatchAwardType.HighestKillStreak => ("Dominator", "Dominator"),
            MatchAwardType.MostHeroDamageDone => ("Painbringer", "Painbringer"),
            MatchAwardType.MostStuns => ("Stunner", "Stunner"),
            MatchAwardType.MostXPContribution => ("Experienced", "Experienced"),
            MatchAwardType.MostSiegeDamageDone => ("SiegeMaster", "Siege Master"),
            MatchAwardType.MostMercCampsCaptured => ("Headhunter", "Headhunter"),
            MatchAwardType.MostDamageTaken => ("Bulwark", "Bulwark"),
            MatchAwardType.MostHealing => ("Healing", "Most Healing"),
            MatchAwardType.MostKills => ("Finisher", "Finisher"),
            MatchAwardType.HatTrick => ("HatTrick", "Hat Trick"),
            MatchAwardType.ClutchHealer => ("ClutchHealer", "Clutch Healer"),
            MatchAwardType.MostProtection => ("Protector", "Protector"),
            MatchAwardType.ZeroDeaths => ("SoleSurvivor", "Sole Survivor"),
            MatchAwardType.MostRoots => ("Trapper", "Trapper"),
            MatchAwardType.ZeroOutnumberedDeaths => ("TeamPlayer", "Team Player"),
            MatchAwardType.MostDaredevilEscapes => ("Daredevil", "Daredevil"),
            MatchAwardType.MostEscapes => ("EscapeArtist", "Escape Artist"),
            MatchAwardType.MostSilences => ("Silencer", "Silencer"),
            MatchAwardType.MostTeamfightDamageTaken => ("Guardian", "Guardian"),
            MatchAwardType.MostTeamfightHealingDone => ("CombatMedic", "Combat Medic"),
            MatchAwardType.MostTeamfightHeroDamageDone => ("Scrapper", "Scrapper"),
            MatchAwardType.MostVengeancesPerformed => ("Avenger", "Avenger"),
            MatchAwardType.MostImmortalDamage => ("ImmortalSlayer", "Immortal Slayer"),
            MatchAwardType.MostCoinsPaid => ("Moneybags", "Moneybags"),
            MatchAwardType.MostCurseDamageDone => ("MasteroftheCurse", "Master of the Curse"),
            MatchAwardType.MostDragonShrinesCaptured => ("Shriner", "Shriner"),
            MatchAwardType.MostDamageToPlants => ("GardenTerror", "Most Damage Done to Plant Minions"),
            MatchAwardType.MostDamageToMinions => ("GuardianSlayer", "Guardian Slayer"),
            MatchAwardType.MostTimeInTemple => ("TempleMaster", "Most Time Capturing Temples"),
            MatchAwardType.MostGemsTurnedIn => ("Jeweler", "Jeweler"),
            MatchAwardType.MostAltarDamage => ("Cannoneer", "Cannoneer"),
            MatchAwardType.MostDamageDoneToZerg => ("ZergCrusher", "Zerg Crusher"),
            MatchAwardType.MostNukeDamageDone => ("DaBomb", "Da Bomb"),
            MatchAwardType.MostSkullsCollected => ("Moneybags", "Most Skulls Collected"),
            MatchAwardType.MostTimePushing => ("Cannoneer", "Most Time Pushing"),
            MatchAwardType.MostTimeOnPoint => ("TempleMaster", "Most Time on Point"),
            MatchAwardType.MostInterruptedCageUnlocks => ("Guardian", "Most Interrupted Cage Unlocks"),
            MatchAwardType.MostSeedsCollected => ("GardenTerror", "Most Seeds Collected"), // TODO: find correct icon (not GardenTerror) -- Aviad, 3-Nov-2022
            _ => (null, null),
        };

    private static IServiceProvider _svcp;
    private static string _connectionString;
    private static ILogger<LogNamesake> _logger;


    public enum LocalizationAliasType
    {
        Map = 0,
        Hero = 1,
        Unknown = 9,
    }

    class LogNamesake { }

    public static void SetServiceProvider(IServiceProvider svcp)
    {
        _svcp = svcp;
        _logger = _svcp.GetRequiredService<ILogger<LogNamesake>>();
        var options = svcp.GetRequiredService<IOptions<HotsLogsOptions>>().Value;
        var config = svcp.GetRequiredService<IConfiguration>();
        _connectionString = config.GetConnectionString("DefaultConnection");
        try
        {
            AWSAccessKeyID = options.AwsAccessKeyID;
            AWSSecretAccessKey = options.AwsSecretAccessKey;
        }
        catch
        {
            /* ignored */
        }
    }

    public static int GamesPlayedRequirementForWinPercentDisplay => 33;

    #region Parse/Add Replays

    public static Tuple<DataParser.ReplayParseResult, Guid?> AddReplay(
        LocalizationAlias[] localizationAliases,
        byte[] bytes,
        string ipAddress,
        int? eventId = null,
        Action<string> logFunction = null)
    {
        using var scope = _svcp.CreateScope();
        var heroesEntity = HeroesdataContext.Create(scope);
        return AddReplay(heroesEntity, localizationAliases, bytes, ipAddress, eventId, logFunction);
    }

    public static Tuple<DataParser.ReplayParseResult, Guid?> AddReplay(
        LocalizationAlias[] localizationAliases,
        string filePath,
        bool deleteFile,
        string ipAddress,
        int? eventId = null,
        Action<string> logFunction = null)
    {
        using var scope = _svcp.CreateScope();
        var heroesEntity = HeroesdataContext.Create(scope);
        return AddReplay(
            heroesEntity,
            localizationAliases,
            filePath,
            deleteFile,
            ipAddress,
            eventId,
            logFunction);
    }

    public static Tuple<DataParser.ReplayParseResult, Guid?> AddReplay(
        HeroesdataContext heroesEntity,
        LocalizationAlias[] localizationAliases,
        byte[] bytes,
        string ipAddress,
        int? eventId = null,
        Action<string> logFunction = null)
    {
        var parsedReplay = SafeParseWrapper(
            () =>
                DataParser.ParseReplay(
                    bytes,
                    ignoreErrors: false,
                    allowPTRRegion: eventId.HasValue));

        if (parsedReplay.Item1 != DataParser.ReplayParseResult.Success)
        {
            return Tuple.Create(parsedReplay.Item1, (Guid?)null);
        }

        if (parsedReplay.Item2.GameMode == GameMode.Unknown)
        {
            LogGameReplay(ipAddress, bytes, "StormReplay");
        }

        return AddReplay(
            heroesEntity,
            localizationAliases,
            parsedReplay,
            bytes,
            ipAddress,
            logFunction,
            eventId);
    }

    public static Tuple<DataParser.ReplayParseResult, Guid?> AddReplay(
        HeroesdataContext heroesEntity,
        LocalizationAlias[] localizationAliases,
        string filePath,
        bool deleteFile,
        string ipAddress,
        int? eventId = null,
        Action<string> logFunction = null)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        var parsedReplay = SafeParseWrapper(
            () =>
                DataParser.ParseReplay(
                    filePath,
                    ignoreErrors: false,
                    deleteFile: deleteFile,
                    allowPTRRegion: eventId.HasValue));

        if (parsedReplay.Item1 != DataParser.ReplayParseResult.Success)
        {
            return Tuple.Create(parsedReplay.Item1, (Guid?)null);
        }

        if (parsedReplay.Item2.GameMode == GameMode.Unknown)
        {
            LogGameReplay(ipAddress, fileBytes, "StormReplay");
        }

        return AddReplay(
            heroesEntity,
            localizationAliases,
            parsedReplay,
            fileBytes,
            ipAddress,
            logFunction,
            eventId);
    }

    private static Tuple<DataParser.ReplayParseResult, Replay> SafeParseWrapper(
        Func<Tuple<DataParser.ReplayParseResult, Replay>> parseAction)
    {
        try
        {
            var parsedReplay = parseAction();
            return parsedReplay;
        }
        catch (ArgumentOutOfRangeException) { }
        catch (DetailedParsedException) { }
        catch (EndOfStreamException) { }
        catch (OverflowException) { }
        catch (NotImplementedException) { }

        return Tuple.Create(DataParser.ReplayParseResult.Exception, (Replay)null);
    }

    public static int? GetReplayID(Guid replayHash)
    {
        using var mysqlConnection =
            new MySqlConnection(_connectionString);
        mysqlConnection.Open();
        using var mysqlCommand =
            new MySqlCommand(
                "select ReplayID from Replay where ReplayHash = @replayHash or ReplayHash = @replayHashOld",
                mysqlConnection);
        mysqlCommand.Parameters.AddWithValue("@replayHash", replayHash.ToByteArray());
        mysqlCommand.Parameters.AddWithValue(
            "@replayHashOld",
            replayHash.ConvertGuidToOldFormat().ToByteArray());

        var replayIdResult = mysqlCommand.ExecuteScalar();

        return (int?)replayIdResult;
    }

    #endregion

    #region Email and Logging

    public static void AwsEsSendEmail(List<string> toAddresses, string subject, string message)
    {
        using var amazonSimpleEmailServiceClient =
            new AmazonSimpleEmailServiceClient(AWSAccessKeyID, AWSSecretAccessKey, RegionEndpoint.USWest2);
        amazonSimpleEmailServiceClient.SendEmail(
            new SendEmailRequest
            {
                Source = "darryl.b.roman@disney.com",
                Destination = new Destination { ToAddresses = toAddresses },
                Message = new Message(
                    new Content(subject),
                    new Body { Html = new Content(message) }),
            });
    }

    public static void AwsEsSendEmail(string toAddress, string subject, string message)
    {
        AwsEsSendEmail(new List<string> { toAddress }, subject, message);
    }

    public static void SendServerErrorEmail(string message)
    {
        try
        {
            AwsEsSendEmail("darrylroman@gmail.com", "HOTSLogs.com Server Error: " + DateTime.UtcNow, message);
            AwsEsSendEmail("darryl@boundlessechoes.com", "HOTSLogs.com Server Error: " + DateTime.UtcNow, message);
            AwsEsSendEmail("darryl.b.roman@disney.com", "HOTSLogs.com Server Error: " + DateTime.UtcNow, message);
        }
        catch (Exception ex)
        {
            LogApplicationEvents("Error: " + ex.InnerException, "HOTSApplicationErrors");
        }
    }

    public static void LogGameReplay(string ipAddress, byte[] fileBytes, string fileExtension)
    {
        try
        {
            var sanitizedAddress = Regex.Replace(ipAddress, @"[.:]", "_");
            var nowString = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var fileName = $"{nowString}_{sanitizedAddress}.{fileExtension}";
            var logMsg = $"Replay parsed with GameMode -9 (saved to {fileName})";
            LogBytes(fileBytes, "UploadedGameMode-9", fileName);
            LogApplicationEvents(logMsg, "UploadLogs");
        }
        catch
        {
            // ignored
        }
    }

    public static void LogBytes(byte[] bytes, string dirName, string fileName)
    {
        try
        {
            var dirPath = Path.Combine(LogDirectory, dirName);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var filePath = Path.Combine(dirPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.WriteAllBytes(filePath, bytes);
        }
        catch
        {
            // ignored
        }
    }

    public static void LogApplicationEvents(string newEvent, string fileName, bool debug = false)
    {
        try
        {
            var source = Path.GetFileNameWithoutExtension(fileName);
            if (debug)
            {
                _logger.LogDebug("[SOURCE:{source}] {logLine}", source, newEvent);
            }
            else
            {
                _logger.LogInformation("[SOURCE:{source}] {logLine}", source, newEvent);
            }

            // just output to debug so we can at least see where we are
            Debug.WriteLine(DateTime.UtcNow + ":" + newEvent);
        }
        catch (Exception)
        {
            // don't do anything, swallow these errors. just doesn't get logged if something breaks
        }
    }

    public static void LogError(
        string absoluteUri,
        string userAgent,
        string userHostAddress,
        int? userId,
        string errorMessage,
        string referer,
        HeroesdataContext heroesEntity = null,
        bool saveChanges = true)
    {
        try
        {
            var logError = new LogError
            {
                AbsoluteUri = absoluteUri,
                UserAgent = userAgent ?? string.Empty,
                UserHostAddress = userHostAddress,
                UserId = userId,
                ErrorMessage = errorMessage,
                Referer = referer,
                DateTimeErrorOccurred = DateTime.UtcNow,
            };

            if (heroesEntity == null)
            {
                using var scope = _svcp.CreateScope();
                heroesEntity = HeroesdataContext.Create(scope);
                heroesEntity.LogErrors.Add(logError);
                if (saveChanges)
                {
                    heroesEntity.SaveChanges();
                }
            }
            else
            {
                heroesEntity.LogErrors.Add(logError);
                if (saveChanges)
                {
                    heroesEntity.SaveChanges();
                }
            }
        }
        catch
        {
            /* ignored */
        }
    }

    //public static void LogError(HttpRequest request, int? userId, string errorMessage, HeroesdataContext heroesEntity = null, bool saveChanges = true)
    //{
    //    LogError(request.Url.AbsoluteUri, request.UserAgent, request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress, userId, errorMessage, heroesEntity, saveChanges);
    //}

    #endregion

    #region Redis Access

    public static int RedisCacheGetInt(string key)
    {
        using var scope = _svcp.CreateScope();
        var redisClient = MyDbWrapper.Create(scope);
        if (!redisClient.ContainsKey(key))
        {
            return default;
        }

        var keyValue = redisClient.GetInt(key);
        return keyValue;

    }

    public static T RedisCacheGet<T>(string key)
        where T : class
    {
        using var scope = _svcp.CreateScope();
        var redisClient = MyDbWrapper.Create(scope);
        if (!redisClient.ContainsKey(key))
        {
            return default;
        }

        var keyValue = redisClient.Get<T>(key);
        return keyValue;

    }

    #endregion

    #region Helper Functions

    public static SitewideHeroTalentStatistic[] GetSitewideHeroTalentStatistics(
        DateTime datetimeBegin,
        DateTime datetimeEnd,
        int? replayBuild = null,
        int? characterId = null,
        int? mapId = null,
        int? leagueId = null,
        int[] gameModes = null,
        int[] playerIDs = null,
        bool innerJoinTalentInformation = false,
        int[] replayIDs = null,
        int? upToBuild = null)
    {
        const string mySqlCommandText =
            @"select la.PrimaryName as `Character`, max(h.TalentTier) as TalentTier, min(q.TalentID) as TalentID, coalesce(h.TalentName, concat('Talent ',  min(q.TalentID))) as TalentName, min(h.TalentDescription) as TalentDescription, sum(q.GamesPlayed) as GamesPlayed, sum(q.GamesWon) / sum(q.GamesPlayed) as WinPercent from
                (select r.ReplayBuild, rc.CharacterID, t.TalentID, count(*) as GamesPlayed, sum(rc.IsWinner) as GamesWon
                from ReplayCharacterTalent t
                join ReplayCharacter rc on rc.ReplayID = t.ReplayID and rc.PlayerID = t.PlayerID
                {10}
                join Replay r {7} on r.ReplayID = rc.ReplayID
                {0}
                where r.TimestampReplay >= @datetimeBegin and r.TimestampReplay < @datetimeEnd {4} {6} {1} {2} {3} {8} {9} {11} {12}
                group by r.ReplayBuild, rc.CharacterID, t.TalentID) q
                join LocalizationAlias la on la.IdentifierID = q.CharacterID
                {5} join HeroTalentInformation h on h.`Character` = la.PrimaryName and q.ReplayBuild >= h.ReplayBuildFirst and q.ReplayBuild <= h.ReplayBuildLast and h.TalentID = q.TalentID
                group by q.CharacterID, TalentName
                order by q.CharacterID, TalentTier, TalentID";

        const string mySqlCommandTextCurrentTalentDescription =
            @"select innerQ.`Character`, innerQ.TalentTier, innerQ.TalentID, innerQ.TalentName, h.TalentDescription, innerQ.GamesPlayed, innerQ.WinPercent from
                (select la.PrimaryName as `Character`, max(h.TalentTier) as TalentTier, min(q.TalentID) as TalentID, coalesce(h.TalentName, concat('Talent ',  min(q.TalentID))) as TalentName, sum(q.GamesPlayed) as GamesPlayed, sum(q.GamesWon) / sum(q.GamesPlayed) as WinPercent from
                (select r.ReplayBuild, rc.CharacterID, t.TalentID, count(*) as GamesPlayed, sum(rc.IsWinner) as GamesWon
                from ReplayCharacterTalent t
                join ReplayCharacter rc on rc.ReplayID = t.ReplayID and rc.PlayerID = t.PlayerID
                {10}
                join Replay r {7} on r.ReplayID = rc.ReplayID
                {0}
                where r.TimestampReplay >= @datetimeBegin and r.TimestampReplay < @datetimeEnd {4} {6} {1} {2} {3} {8} {9} {11} {12}
                group by r.ReplayBuild, rc.CharacterID, t.TalentID) q
                join LocalizationAlias la on la.IdentifierID = q.CharacterID
                {5} join HeroTalentInformation h on h.`Character` = la.PrimaryName and q.ReplayBuild >= h.ReplayBuildFirst and q.ReplayBuild <= h.ReplayBuildLast and h.TalentID = q.TalentID
                group by q.CharacterID, TalentName) innerQ
                join HeroTalentInformation h on h.`Character` = innerQ.`Character` and h.ReplayBuildLast = (select max(innerH.ReplayBuildLast) from HeroTalentInformation innerH where innerH.`Character` = innerQ.`Character` and innerH.TalentName = innerQ.TalentName) and h.TalentName = innerQ.TalentName
                order by innerQ.`Character`, innerQ.TalentTier, innerQ.TalentID";

        // If Game Modes is null, use default of Storm League
        // If Game Modes.Length = 0, select Game Modes with statistics
        if (gameModes == null)
        {
            gameModes = new[] { (int)GameMode.StormLeague };
        }
        else if (gameModes.Length == 0)
        {
            gameModes = GameModeWithStatistics;
        }

        var sitewideHeroTalentStatistics = new List<SitewideHeroTalentStatistic>();
        using var mySqlConnection =
            new MySqlConnection(_connectionString);
        mySqlConnection.Open();
        QueryBuilder queryBuilder;

        var leagueTerm = leagueId.HasValue ? "and lr.LeagueID = @leagueID" : string.Empty;
        var mapTerm = mapId.HasValue ? "and r.MapID = @mapID" : string.Empty;
        string buildNumberTerm;
        if (replayBuild.HasValue)
        {
            buildNumberTerm = upToBuild.HasValue
                ? "and r.ReplayBuild >= @replayBuild and r.ReplayBuild <= @upToBuild"
                : "and r.ReplayBuild = @replayBuild";
        }
        else
        {
            buildNumberTerm = string.Empty;
        }

        var talentsTerm = innerJoinTalentInformation ? string.Empty : "left";
        var characterTerm = characterId.HasValue ? "and rc.CharacterID = @characterID" : string.Empty;
        var gameModesTerm = gameModes.Length > 0
            ? " and r.GameMode in (" + string.Join(",", gameModes) + ")"
            : string.Empty;
        var replaysTerm = replayIDs != null
            ? $"and r.ReplayID in ({string.Join(",", replayIDs)})"
            : string.Empty;

        if (playerIDs == null)
        {
            queryBuilder = new QueryBuilder
            {
                QueryText = mySqlCommandText,
                League = leagueTerm,
                Map = mapTerm,
                BuildNumber = buildNumberTerm,
                IncludeTalents = talentsTerm,
                Character = characterTerm,
                GameModes = gameModesTerm,
                Replays = replaysTerm,
                Players = string.Empty,
                JoinRanking =
                    gameModes.Length == 1 && gameModes.Single() < 1000
                        ? "join LeaderboardRanking lr on lr.PlayerID = rc.PlayerID and (lr.GameMode = r.gameMode or (r.gameMode = 7 and lr.GameMode = 3))"
                        : string.Empty,
                UseIndex = "use index (IX_GameMode_TimestampReplay)",
                // Don't filter out levels 1-4 -- Aviad, 14-Feb-2021
                // Spec1 =
                //     gameModes.All(i => i < 1000)
                //         ? "and (rc.CharacterLevel >= 5 or r.GameMode = 7)"
                //         : string.Empty,
                Spec1 = string.Empty,
                Spec2 = gameModes.Any(i => i == (int)GameMode.QuickMatch)
                    ? "left join ReplayCharacter rcMirror on rcMirror.ReplayID = rc.ReplayID and rcMirror.PlayerID != rc.PlayerID and rcMirror.CharacterID = rc.CharacterID"
                    : string.Empty,
                Spec3 = gameModes.Any(i => i == (int)GameMode.QuickMatch)
                    ? "and rcMirror.ReplayID is null"
                    : string.Empty,
            };
        }
        else
        {
            queryBuilder = new QueryBuilder
            {
                QueryText = mySqlCommandTextCurrentTalentDescription,
                League = leagueTerm,
                Map = mapTerm,
                BuildNumber = buildNumberTerm,
                IncludeTalents = talentsTerm,
                Character = characterTerm,
                GameModes = gameModesTerm,
                Replays = replaysTerm,
                Players = "and t.PlayerID in (" + string.Join(",", playerIDs) + @")",
                JoinRanking = string.Empty,
                UseIndex = string.Empty,
                Spec1 = string.Empty,
                Spec2 = string.Empty,
                Spec3 = string.Empty,
            };
        }

        using var mySqlCommand = new MySqlCommand(queryBuilder.Build(), mySqlConnection)
        {
            CommandTimeout = MMR.LongCommandTimeout,
        };
        mySqlCommand.Parameters.AddWithValue("@datetimeBegin", datetimeBegin);
        mySqlCommand.Parameters.AddWithValue("@datetimeEnd", datetimeEnd);
        if (characterId.HasValue)
        {
            mySqlCommand.Parameters.AddWithValue("@characterID", characterId.Value);
        }

        if (replayBuild.HasValue)
        {
            mySqlCommand.Parameters.AddWithValue("@replayBuild", replayBuild);
            if (upToBuild.HasValue)
            {
                mySqlCommand.Parameters.AddWithValue("@upToBuild", upToBuild);
            }
        }

        if (leagueId.HasValue)
        {
            mySqlCommand.Parameters.AddWithValue("@leagueID", leagueId);
        }

        if (gameModes.Length == 1 && gameModes.Single() < 1000)
        {
            mySqlCommand.Parameters.AddWithValue("@gameMode", gameModes[0]);
        }

        if (mapId.HasValue)
        {
            mySqlCommand.Parameters.AddWithValue("@mapID", mapId.Value);
        }

        using (var mySqlDataReader = mySqlCommand.ExecuteReader())
        {
            while (mySqlDataReader.Read())
            {
                var entry = new SitewideHeroTalentStatistic
                {
                    Character = mySqlDataReader.GetString("Character"),
                    TalentTier =
                        mySqlDataReader["TalentTier"] != DBNull.Value
                            ? mySqlDataReader.GetInt32("TalentTier")
                            : null,
                    TalentID = mySqlDataReader.GetInt32("TalentID"),
                    TalentName = mySqlDataReader.GetString("TalentName"),
                    TalentDescription =
                        mySqlDataReader["TalentDescription"] != DBNull.Value
                            ? mySqlDataReader.GetString("TalentDescription")
                            : null,
                    GamesPlayed = (int)mySqlDataReader.GetDecimal("GamesPlayed"),
                    WinPercent = mySqlDataReader.GetDecimal("WinPercent"),
                };
                sitewideHeroTalentStatistics.Add(entry);
            }
        }

        return sitewideHeroTalentStatistics.ToArray();
    }

    public static async Task<SitewideHeroTalentStatistic[]> GetSitewideHeroTalentStatisticsAsync(
        DateTime datetimeBegin,
        DateTime datetimeEnd,
        int? replayBuild = null,
        int? characterId = null,
        int? mapId = null,
        int? leagueId = null,
        int[] gameModes = null,
        int[] playerIDs = null,
        bool innerJoinTalentInformation = false,
        int[] replayIDs = null,
        int? upToBuild = null,
        CancellationToken token = default)
    {
        const string mySqlCommandText =
            @"select la.PrimaryName as `Character`, max(h.TalentTier) as TalentTier, min(q.TalentID) as TalentID, coalesce(h.TalentName, concat('Talent ',  min(q.TalentID))) as TalentName, min(h.TalentDescription) as TalentDescription, sum(q.GamesPlayed) as GamesPlayed, sum(q.GamesWon) / sum(q.GamesPlayed) as WinPercent from
                (select r.ReplayBuild, rc.CharacterID, t.TalentID, count(*) as GamesPlayed, sum(rc.IsWinner) as GamesWon
                from ReplayCharacterTalent t
                join ReplayCharacter rc on rc.ReplayID = t.ReplayID and rc.PlayerID = t.PlayerID
                {10}
                join Replay r {7} on r.ReplayID = rc.ReplayID
                {0}
                where r.TimestampReplay >= @datetimeBegin and r.TimestampReplay < @datetimeEnd {4} {6} {1} {2} {3} {8} {9} {11} {12}
                group by r.ReplayBuild, rc.CharacterID, t.TalentID) q
                join LocalizationAlias la on la.IdentifierID = q.CharacterID
                {5} join HeroTalentInformation h on h.`Character` = la.PrimaryName and q.ReplayBuild >= h.ReplayBuildFirst and q.ReplayBuild <= h.ReplayBuildLast and h.TalentID = q.TalentID
                group by q.CharacterID, TalentName
                order by q.CharacterID, TalentTier, TalentID";

        const string mySqlCommandTextCurrentTalentDescription =
            @"select innerQ.`Character`, innerQ.TalentTier, innerQ.TalentID, innerQ.TalentName, h.TalentDescription, innerQ.GamesPlayed, innerQ.WinPercent from
                (select la.PrimaryName as `Character`, max(h.TalentTier) as TalentTier, min(q.TalentID) as TalentID, coalesce(h.TalentName, concat('Talent ',  min(q.TalentID))) as TalentName, sum(q.GamesPlayed) as GamesPlayed, sum(q.GamesWon) / sum(q.GamesPlayed) as WinPercent from
                (select r.ReplayBuild, rc.CharacterID, t.TalentID, count(*) as GamesPlayed, sum(rc.IsWinner) as GamesWon
                from ReplayCharacterTalent t
                join ReplayCharacter rc on rc.ReplayID = t.ReplayID and rc.PlayerID = t.PlayerID
                {10}
                join Replay r {7} on r.ReplayID = rc.ReplayID
                {0}
                where r.TimestampReplay >= @datetimeBegin and r.TimestampReplay < @datetimeEnd {4} {6} {1} {2} {3} {8} {9} {11} {12}
                group by r.ReplayBuild, rc.CharacterID, t.TalentID) q
                join LocalizationAlias la on la.IdentifierID = q.CharacterID
                {5} join HeroTalentInformation h on h.`Character` = la.PrimaryName and q.ReplayBuild >= h.ReplayBuildFirst and q.ReplayBuild <= h.ReplayBuildLast and h.TalentID = q.TalentID
                group by q.CharacterID, TalentName) innerQ
                join HeroTalentInformation h on h.`Character` = innerQ.`Character` and h.ReplayBuildLast = (select max(innerH.ReplayBuildLast) from HeroTalentInformation innerH where innerH.`Character` = innerQ.`Character` and innerH.TalentName = innerQ.TalentName) and h.TalentName = innerQ.TalentName
                order by innerQ.`Character`, innerQ.TalentTier, innerQ.TalentID";

        // If Game Modes is null, use default of Storm League
        // If Game Modes.Length = 0, select Game Modes with statistics
        if (gameModes == null)
        {
            gameModes = new[] { (int)GameMode.StormLeague };
        }
        else if (gameModes.Length == 0)
        {
            gameModes = GameModeWithStatistics;
        }

        var sitewideHeroTalentStatistics = new List<SitewideHeroTalentStatistic>();
        await using var mySqlConnection =
            new MySqlConnection(_connectionString);
        await mySqlConnection.OpenAsync(token);
        QueryBuilder queryBuilder;

        var leagueTerm = leagueId.HasValue ? "and lr.LeagueID = @leagueID" : string.Empty;
        var mapTerm = mapId.HasValue ? "and r.MapID = @mapID" : string.Empty;
        string buildNumberTerm;
        if (replayBuild.HasValue)
        {
            buildNumberTerm = upToBuild.HasValue
                ? "and r.ReplayBuild >= @replayBuild and r.ReplayBuild <= @upToBuild"
                : "and r.ReplayBuild = @replayBuild";
        }
        else
        {
            buildNumberTerm = string.Empty;
        }

        var talentsTerm = innerJoinTalentInformation ? string.Empty : "left";
        var characterTerm = characterId.HasValue ? "and rc.CharacterID = @characterID" : string.Empty;
        var gameModesTerm = gameModes.Length > 0
            ? " and r.GameMode in (" + string.Join(",", gameModes) + ")"
            : string.Empty;
        var replaysTerm = replayIDs != null
            ? $"and r.ReplayID in ({string.Join(",", replayIDs)})"
            : string.Empty;

        if (playerIDs == null)
        {
            queryBuilder = new QueryBuilder
            {
                QueryText = mySqlCommandText,
                League = leagueTerm,
                Map = mapTerm,
                BuildNumber = buildNumberTerm,
                IncludeTalents = talentsTerm,
                Character = characterTerm,
                GameModes = gameModesTerm,
                Replays = replaysTerm,
                Players = string.Empty,
                JoinRanking =
                    gameModes.Length == 1 && gameModes.Single() < 1000
                        ? "join LeaderboardRanking lr on lr.PlayerID = rc.PlayerID and (lr.GameMode = r.gameMode or (r.gameMode = 7 and lr.GameMode = 3))"
                        : string.Empty,
                UseIndex = "use index (IX_GameMode_TimestampReplay)",
                // Don't filter out levels 1-4 -- Aviad, 14-Feb-2021
                // Spec1 =
                //     gameModes.All(i => i < 1000)
                //         ? "and (rc.CharacterLevel >= 5 or r.GameMode = 7)"
                //         : string.Empty,
                Spec1 = string.Empty,
                Spec2 = gameModes.Any(i => i == (int)GameMode.QuickMatch)
                    ? "left join ReplayCharacter rcMirror on rcMirror.ReplayID = rc.ReplayID and rcMirror.PlayerID != rc.PlayerID and rcMirror.CharacterID = rc.CharacterID"
                    : string.Empty,
                Spec3 = gameModes.Any(i => i == (int)GameMode.QuickMatch)
                    ? "and rcMirror.ReplayID is null"
                    : string.Empty,
            };
        }
        else
        {
            queryBuilder = new QueryBuilder
            {
                QueryText = mySqlCommandTextCurrentTalentDescription,
                League = leagueTerm,
                Map = mapTerm,
                BuildNumber = buildNumberTerm,
                IncludeTalents = talentsTerm,
                Character = characterTerm,
                GameModes = gameModesTerm,
                Replays = replaysTerm,
                Players = "and t.PlayerID in (" + string.Join(",", playerIDs) + @")",
                JoinRanking = string.Empty,
                UseIndex = string.Empty,
                Spec1 = string.Empty,
                Spec2 = string.Empty,
                Spec3 = string.Empty,
            };
        }

        await using var mySqlCommand = new MySqlCommand(queryBuilder.Build(), mySqlConnection)
        {
            CommandTimeout = MMR.LongCommandTimeout,
        };
        mySqlCommand.Parameters.AddWithValue("@datetimeBegin", datetimeBegin);
        mySqlCommand.Parameters.AddWithValue("@datetimeEnd", datetimeEnd);
        if (characterId.HasValue)
        {
            mySqlCommand.Parameters.AddWithValue("@characterID", characterId.Value);
        }

        if (replayBuild.HasValue)
        {
            mySqlCommand.Parameters.AddWithValue("@replayBuild", replayBuild);
            if (upToBuild.HasValue)
            {
                mySqlCommand.Parameters.AddWithValue("@upToBuild", upToBuild);
            }
        }

        if (leagueId.HasValue)
        {
            mySqlCommand.Parameters.AddWithValue("@leagueID", leagueId);
        }

        if (gameModes.Length == 1 && gameModes.Single() < 1000)
        {
            mySqlCommand.Parameters.AddWithValue("@gameMode", gameModes[0]);
        }

        if (mapId.HasValue)
        {
            mySqlCommand.Parameters.AddWithValue("@mapID", mapId.Value);
        }

        await using (var mySqlDataReader = await mySqlCommand.ExecuteReaderAsync(token))
        {
            while (await mySqlDataReader.ReadAsync(token))
            {
                var entry = new SitewideHeroTalentStatistic
                {
                    Character = mySqlDataReader.GetString("Character"),
                    TalentTier =
                        mySqlDataReader["TalentTier"] != DBNull.Value
                            ? mySqlDataReader.GetInt32("TalentTier")
                            : null,
                    TalentID = mySqlDataReader.GetInt32("TalentID"),
                    TalentName = mySqlDataReader.GetString("TalentName"),
                    TalentDescription =
                        mySqlDataReader["TalentDescription"] != DBNull.Value
                            ? mySqlDataReader.GetString("TalentDescription")
                            : null,
                    GamesPlayed = (int)mySqlDataReader.GetDecimal("GamesPlayed"),
                    WinPercent = mySqlDataReader.GetDecimal("WinPercent"),
                };
                sitewideHeroTalentStatistics.Add(entry);
            }
        }

        return sitewideHeroTalentStatistics.ToArray();
    }

    //public static MediaTypeNames.Image RoundCorners(MediaTypeNames.Image image, int cornerRadius)
    //{
    //    cornerRadius *= 2;
    //    var roundedImage = new Bitmap(image.Width, image.Height);
    //    using (var graphicsPath = new GraphicsPath())
    //    {
    //        graphicsPath.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
    //        graphicsPath.AddArc(0 + roundedImage.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
    //        graphicsPath.AddArc(0 + roundedImage.Width - cornerRadius, 0 + roundedImage.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
    //        graphicsPath.AddArc(0, 0 + roundedImage.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
    //        using (var graphics = Graphics.FromImage(roundedImage))
    //        {
    //            graphics.SmoothingMode = SmoothingMode.HighQuality;
    //            graphics.SetClip(graphicsPath);
    //            graphics.DrawImage(image, System.Drawing.Point.Empty);
    //        }
    //    }

    //    return roundedImage;
    //}

    //public static Color InterpolateColor(Color source, Color target, double min, double max, double percent)
    //{
    //    if (percent < min)
    //    {
    //        percent = min;
    //    }
    //    else if (percent > max)
    //    {
    //        percent = max;
    //    }

    //    percent = (percent - min) / (max - min);

    //    return ControlPaint.LightLight(Color.FromArgb(
    //        (byte)(source.R + ((target.R - source.R) * percent)),
    //        (byte)(source.G + ((target.G - source.G) * percent)),
    //        (byte)(source.B + ((target.B - source.B) * percent))));
    //}

    public static TimeSpan InterpolateTimeSpan(TimeSpan source, TimeSpan target, double percent)
    {
        return TimeSpan.FromSeconds(((1d - percent) * source.TotalSeconds) + (percent * target.TotalSeconds));
    }

    public static TimeSpan InterpolateTimeSpan(
        TimeSpan source,
        TimeSpan target,
        double sourceWeight,
        double targetWeight)
    {
        return InterpolateTimeSpan(source, target, 1d - (sourceWeight / (sourceWeight + targetWeight)));
    }

    public static int InterpolateInt(int source, int target, double percent)
    {
        return (int)(((1d - percent) * source) + (percent * target));
    }

    public static int InterpolateInt(int source, int target, double sourceWeight, double targetWeight)
    {
        return InterpolateInt(source, target, 1d - (sourceWeight / (sourceWeight + targetWeight)));
    }

    public static decimal InterpolateDecimal(decimal source, decimal target, decimal percent)
    {
        return ((1m - percent) * source) + (percent * target);
    }

    public static decimal InterpolateDecimal(decimal source, decimal target, decimal sourceWeight, decimal targetWeight)
    {
        return InterpolateDecimal(source, target, 1m - (sourceWeight / (sourceWeight + targetWeight)));
    }

    public static string GetGameMode(this GameMode gameMode)
    {
        switch (gameMode)
        {
            case GameMode.TryMe:
                return "Try Me";
            case GameMode.QuickMatch:
                return "Quick Match";
            case GameMode.UnrankedDraft:
                return "Unranked Draft";
            case GameMode.HeroLeague:
                return "Hero League";
            case GameMode.TeamLeague:
                return "Team League";
            case GameMode.StormLeague:
                return "Storm League";
            case GameMode.Unknown:
            case GameMode.Event:
            case GameMode.Custom:
            case GameMode.Practice:
            case GameMode.Cooperative:
            case GameMode.Brawl:
            case GameMode.ARAM:
            default:
                return gameMode.ToString();
        }
    }

    public static string GetTeamObjectiveTypeString(this TeamObjectiveType teamObjectiveType)
    {
        switch (teamObjectiveType)
        {
            case TeamObjectiveType.BossCampCaptureWithCampID:
                return "Boss Camp";
            case TeamObjectiveType.FirstCatapultSpawn:
                return "First Catapult Spawn";
            case TeamObjectiveType.BattlefieldOfEternityImmortalFightEndWithPowerPercent:
                return "Immortal Fight Won";
            case TeamObjectiveType.BlackheartsBayGhostShipCapturedWithCoinCost:
                return "Ghost Ship Captured";
            case TeamObjectiveType.BraxisHoldoutZergRushWithLosingZergStrength:
                return "Zerg Rush Killed";
            case TeamObjectiveType.CursedHollowTributeCollectedWithTotalTeamTributes:
                return "Tribute Collected";
            case TeamObjectiveType.DragonShireDragonKnightActivatedWithDragonDurationSeconds:
                return "Dragon Knight Activated";
            case TeamObjectiveType.GardenOfTerrorGardenTerrorActivatedWithGardenTerrorDurationSeconds:
                return "Garden Terror Activated";
            case TeamObjectiveType.HauntedMinesGraveGolemSpawnedWithSkullCount:
                return "Grave Golem Spawned";
            case TeamObjectiveType.InfernalShrinesInfernalShrineCapturedWithLosingScore:
                return "Infernal Shrine Captured";
            case TeamObjectiveType.InfernalShrinesPunisherKilledWithPunisherType:
                return "Punisher Killed";
            case TeamObjectiveType.InfernalShrinesPunisherKilledWithSiegeDamageDone:
                return "Punisher Killed";
            case TeamObjectiveType.InfernalShrinesPunisherKilledWithHeroDamageDone:
                return "Punisher Killed";
            case TeamObjectiveType.SkyTempleShotsFiredWithSkyTempleShotsDamage:
                return "Temple Shots Fired";
            case TeamObjectiveType.TombOfTheSpiderQueenSoulEatersSpawnedWithTeamScore:
                return "Webweavers Spawned";
            case TeamObjectiveType.TowersOfDoomAltarCapturedWithTeamTownsOwned:
                return "Altar Captured";
            case TeamObjectiveType.TowersOfDoomSixTownEventStartWithEventDurationSeconds:
                return "All Forts Controlled";
            case TeamObjectiveType.WarheadJunctionNukeLaunch:
                return "Nuke Launch";
            case TeamObjectiveType.EscapeFromBraxisCheckpoint:
                return "Checkpoint";
            case TeamObjectiveType.EscapeFromBraxisDifficulty:
                return "Difficulty";
            default:
                return teamObjectiveType.ToString();
        }
    }

    public static string GetTeamObjectiveValueString(this TeamObjectiveType teamObjectiveType, int value)
    {
        switch (teamObjectiveType)
        {
            case TeamObjectiveType.BossCampCaptureWithCampID:
                return string.Empty;
            case TeamObjectiveType.FirstCatapultSpawn:
                return string.Empty;
            case TeamObjectiveType.BattlefieldOfEternityImmortalFightEndWithPowerPercent:
                return "Shields: " + ((decimal)value / 100).ToString("P0");
            case TeamObjectiveType.BlackheartsBayGhostShipCapturedWithCoinCost:
                return "Coin Cost: " + value;
            case TeamObjectiveType.BraxisHoldoutZergRushWithLosingZergStrength:
                return "Opposing Score: " + ((decimal)value / 100).ToString("P0");
            case TeamObjectiveType.CursedHollowTributeCollectedWithTotalTeamTributes:
                return value % 3 == 0 ? "Raven Curse Activated" : value + " Tribute" + (value > 1 ? "s" : string.Empty);
            case TeamObjectiveType.DragonShireDragonKnightActivatedWithDragonDurationSeconds:
            case TeamObjectiveType.GardenOfTerrorGardenTerrorActivatedWithGardenTerrorDurationSeconds:
                return "Duration: " + value + " seconds";
            case TeamObjectiveType.HauntedMinesGraveGolemSpawnedWithSkullCount:
                return "Skull Count: " + value;
            case TeamObjectiveType.InfernalShrinesInfernalShrineCapturedWithLosingScore:
                return "Opposing Score: " + value + " / 40";
            case TeamObjectiveType.InfernalShrinesPunisherKilledWithPunisherType:
                return "Type: " + ((TeamObjectiveInfernalShrinesPunisherType)value)
                    .GetTeamObjectiveInfernalShrinesPunisherTypeString();
            case TeamObjectiveType.InfernalShrinesPunisherKilledWithSiegeDamageDone:
                return "Siege Damage: " + value;
            case TeamObjectiveType.InfernalShrinesPunisherKilledWithHeroDamageDone:
                return "Hero Damage: " + value;
            case TeamObjectiveType.SkyTempleShotsFiredWithSkyTempleShotsDamage:
                return "Total Shot Damage: " + value;
            case TeamObjectiveType.TombOfTheSpiderQueenSoulEatersSpawnedWithTeamScore:
                return "Gem Cost: " + value;
            case TeamObjectiveType.TowersOfDoomAltarCapturedWithTeamTownsOwned:
                return "Shots Fired: " + (value + 1);
            case TeamObjectiveType.TowersOfDoomSixTownEventStartWithEventDurationSeconds:
                return "Duration: " + value + " seconds";
            case TeamObjectiveType.WarheadJunctionNukeLaunch:
                return string.Empty;
            case TeamObjectiveType.EscapeFromBraxisCheckpoint:
                return value == 9 ? "Victory" : "Stage " + value + " Complete";
            case TeamObjectiveType.EscapeFromBraxisDifficulty:
                return value == 0 ? "Normal" : value == 1 ? "Heroic" : "Difficulty " + value;
            default:
                return teamObjectiveType.ToString();
        }
    }

    public static string GetTeamObjectiveInfernalShrinesPunisherTypeString(
        this TeamObjectiveInfernalShrinesPunisherType teamObjectiveInfernalShrinesPunisherType)
    {
        switch (teamObjectiveInfernalShrinesPunisherType)
        {
            case TeamObjectiveInfernalShrinesPunisherType.BombardShrine:
                return "Bombard";
            case TeamObjectiveInfernalShrinesPunisherType.ArcaneShrine:
                return "Arcane";
            case TeamObjectiveInfernalShrinesPunisherType.FrozenShrine:
                return "Frozen";
            default:
                return teamObjectiveInfernalShrinesPunisherType.ToString();
        }
    }

    public static string GetTeamObjectiveImageString(this TeamObjectiveType teamObjectiveType, int team)
    {
        const string imageStringTemplateMapObjective = @"/Images/Awards/{0}{1}.png";

        switch (teamObjectiveType)
        {
            case TeamObjectiveType.BossCampCaptureWithCampID:
                return string.Format(imageStringTemplateMapObjective, team, "MVP");
            case TeamObjectiveType.FirstCatapultSpawn:
                return string.Format(imageStringTemplateMapObjective, team, "MVP");
            case TeamObjectiveType.BattlefieldOfEternityImmortalFightEndWithPowerPercent:
                return string.Format(imageStringTemplateMapObjective, team, "ImmortalSlayer");
            case TeamObjectiveType.BlackheartsBayGhostShipCapturedWithCoinCost:
                return string.Format(imageStringTemplateMapObjective, team, "Moneybags");
            case TeamObjectiveType.BraxisHoldoutZergRushWithLosingZergStrength:
                return string.Format(imageStringTemplateMapObjective, team, "ZergCrusher");
            case TeamObjectiveType.CursedHollowTributeCollectedWithTotalTeamTributes:
                return string.Format(imageStringTemplateMapObjective, team, "MasteroftheCurse");
            case TeamObjectiveType.DragonShireDragonKnightActivatedWithDragonDurationSeconds:
                return string.Format(imageStringTemplateMapObjective, team, "Shriner");
            case TeamObjectiveType.GardenOfTerrorGardenTerrorActivatedWithGardenTerrorDurationSeconds:
                return string.Format(imageStringTemplateMapObjective, team, "GardenTerror");
            case TeamObjectiveType.HauntedMinesGraveGolemSpawnedWithSkullCount:
                return string.Format(imageStringTemplateMapObjective, team, "MVP");
            case TeamObjectiveType.InfernalShrinesInfernalShrineCapturedWithLosingScore:
            case TeamObjectiveType.InfernalShrinesPunisherKilledWithPunisherType:
            case TeamObjectiveType.InfernalShrinesPunisherKilledWithSiegeDamageDone:
            case TeamObjectiveType.InfernalShrinesPunisherKilledWithHeroDamageDone:
                return string.Format(imageStringTemplateMapObjective, team, "GuardianSlayer");
            case TeamObjectiveType.SkyTempleShotsFiredWithSkyTempleShotsDamage:
                return string.Format(imageStringTemplateMapObjective, team, "TempleMaster");
            case TeamObjectiveType.TombOfTheSpiderQueenSoulEatersSpawnedWithTeamScore:
                return string.Format(imageStringTemplateMapObjective, team, "Jeweler");
            case TeamObjectiveType.TowersOfDoomAltarCapturedWithTeamTownsOwned:
                return string.Format(imageStringTemplateMapObjective, team, "Cannoneer");
            case TeamObjectiveType.TowersOfDoomSixTownEventStartWithEventDurationSeconds:
                return string.Format(imageStringTemplateMapObjective, team, "MVP");
            case TeamObjectiveType.WarheadJunctionNukeLaunch:
                return string.Format(imageStringTemplateMapObjective, team, "DaBomb");
            case TeamObjectiveType.EscapeFromBraxisCheckpoint:
            case TeamObjectiveType.EscapeFromBraxisDifficulty:
                return string.Format(imageStringTemplateMapObjective, team, "ZergCrusher");
            default:
                return teamObjectiveType.ToString();
        }
    }

    public static string GetMatchAwardTypeString(this MatchAwardType matchAwardType)
    {
        var (code, text) = GetAwardIcon(matchAwardType);
        if (code is null)
        {
            return matchAwardType.ToString();
        }

        return text;
    }

    public static string GetMatchAwardTypeHtmlIcon(this MatchAwardType matchAwardType, int team = 0)
    {
        var altText = "Award: " + matchAwardType.GetMatchAwardTypeString();
        var imgPath = GetMatchAwardImagePath(matchAwardType, team);

        return $@"<img alt='{altText}' title='{altText}' src='{imgPath}' style='height:30px;width:30px;'>";
    }

    public static (string code, string text) GetAwardInfo(this MatchAwardType matchAwardType, int team = 0)
    {
        var text = matchAwardType.GetMatchAwardTypeString();
        var (award, _) = GetAwardIcon(matchAwardType);
        var code = $"{team}{award}";

        return (code, text);
    }

    public static string GetMatchAwardImagePath(this MatchAwardType matchAwardType, int team = 0)
    {
        const string imgPath = "/Images/Heroes/Portraits/Unknown.png";

        var (award, _) = GetAwardIcon(matchAwardType);

        return award is null
            ? imgPath
            : $"/assets/Images/Awards/{team}{award}.png";
    }

    public static string GetTalentName(this UpgradeEventType upgradeEventType)
    {
        switch (upgradeEventType)
        {
            case UpgradeEventType.NovaSnipeMasterDamageUpgrade:
                return "Snipe Master";
            case UpgradeEventType.GallTalentDarkDescentUpgrade:
                return "Dark Descent";
            case UpgradeEventType.RegenMasterStacks:
                return "Regen Master Stacks";
            case UpgradeEventType.MarksmanStacks:
                return "Seasoned Marksman Stacks";
            case UpgradeEventType.WitchDoctorPlagueofToadsPandemicTalentCompletion:
                return "Pandemic";
            default:
                return upgradeEventType.ToString();
        }
    }

    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        var diff = dt.DayOfWeek - startOfWeek;
        if (diff < 0)
        {
            diff += 7;
        }

        return dt.AddDays(-1 * diff).Date;
    }

    public static string PrepareForImageURL(this string input)
    {
        if (input == null)
        {
            return null;
        }

        return Regex.Replace(input, $"[{BadChars}]", "");
    }

    public static string AddSpacesToSentence(this string text, bool preserveAcronyms = true)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);

        for (var i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
            {
                if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                     i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                {
                    newText.Append(' ');
                }
            }

            newText.Append(text[i]);
        }

        return newText.ToString();
    }

    #endregion

    public static MatchAwardContainer GetMatchAwardContainer(
        int[] gameModes,
        int leagueId = -1,
        int? playerId = null,
        int daysOfStatisticsToQuery = 15)
    {
        const string matchAwardSummaryQuery =
            @"select
                r.MapID,
                rc.CharacterID,
                rcma.MatchAwardType,
                count(*) as GamesPlayed,
                sum(rc.IsWinner) as GamesWon
                from Replay r
                join ReplayCharacter rc on rc.ReplayID = r.ReplayID {3}
                join LeaderboardRanking lr on lr.PlayerID = rc.PlayerID and (lr.GameMode = r.GameMode or (r.GameMode = 7 and lr.GameMode = 3)) {2}
                left join ReplayCharacterMatchAward rcma on rcma.ReplayID = rc.ReplayID and rcma.PlayerID = rc.PlayerID
                where r.GameMode in ({0}) and r.TimestampReplay > date_add(@now, interval -{1} day) and r.TimestampReplay < now()
                group by r.MapID, rc.CharacterID, rcma.MatchAwardType";

        using var scope = _svcp.CreateScope();

        // Get IdentifierID dictionary
        var heroesEntity = HeroesdataContext.Create(scope);
        var localizationAliasPrimaryNameDictionary =
            heroesEntity.LocalizationAliases.ToDictionary(i => i.IdentifierId, i => i.PrimaryName);

        // Collect Match Award Summaries
        var matchAwardSummariesList = new List<MatchAwardSummary>();

        using var mySqlConnection =
            new MySqlConnection(_connectionString);
        mySqlConnection.Open();

        var commandText = string.Format(
            matchAwardSummaryQuery,
            string.Join(",", gameModes),
            daysOfStatisticsToQuery,
            leagueId == -1
                ? null
                : "and lr.LeagueID = " + leagueId,
            playerId.HasValue ? "and rc.PlayerID = " + playerId.Value : null);

        using var mySqlCommand = new MySqlCommand(commandText, mySqlConnection)
        {
            CommandTimeout = MMR.LongCommandTimeout,
        };

        mySqlCommand.Parameters.AddWithValue("@now", Now);
        using var mySqlDataReader = mySqlCommand.ExecuteReader();

        while (mySqlDataReader.Read())
        {
            matchAwardSummariesList.Add(
                new MatchAwardSummary
                {
                    MapID = mySqlDataReader.GetInt32("MapID"),
                    CharacterID = mySqlDataReader.GetInt32("CharacterID"),
                    MatchAwardType =
                        !mySqlDataReader.IsDBNull(mySqlDataReader.GetOrdinal("MatchAwardType"))
                            ? mySqlDataReader.GetInt32("MatchAwardType")
                            : null,
                    GamesPlayed = (int)mySqlDataReader.GetInt64("GamesPlayed"),
                    GamesWon = (int)mySqlDataReader.GetDecimal("GamesWon"),
                });
        }

        return new MatchAwardContainer
        {
            DateTimeBegin = DateTime.UtcNow.AddDays(-1 * daysOfStatisticsToQuery),
            DateTimeEnd = DateTime.UtcNow,
            League = leagueId,
            GameModes = gameModes,
            PlayerID = playerId,
            LastUpdated = DateTime.UtcNow,
            StatisticStandard = matchAwardSummariesList
                .GroupBy(i => localizationAliasPrimaryNameDictionary[i.CharacterID])
                .Select(
                    i => new
                    {
                        Character = i.Key,
                        GamesPlayedTotal = i.Sum(j => j.GamesPlayed),
                        GamesPlayedWithAward = i.Where(j => j.MatchAwardType.HasValue).Sum(j => j.GamesPlayed),
                        GamesPlayedWithoutAward = i.Where(j => !j.MatchAwardType.HasValue).Sum(j => j.GamesPlayed),
                        StatisticsWithAwards = i.Where(j => j.MatchAwardType.HasValue).ToArray(),
                    })
                .Select(
                    i => new
                    {
                        i.Character,
                        i.GamesPlayedTotal,
                        i.GamesPlayedWithAward,
                        i.GamesPlayedWithoutAward,
                        i.StatisticsWithAwards,
                        GamesPlayedMVP =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MVP)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedHighestKillStreak =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.HighestKillStreak)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostXPContribution =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostXPContribution)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostHeroDamageDone =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostHeroDamageDone)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostSiegeDamageDone =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostSiegeDamageDone)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostDamageTaken =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MostDamageTaken)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostHealing =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MostHealing)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostStuns =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MostStuns)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostMercCampsCaptured =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostMercCampsCaptured)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMapSpecific =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType > 1000).Sum(j => j.GamesPlayed),
                        GamesPlayedMostKills =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MostKills)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedHatTrick =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.HatTrick)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedClutchHealer =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.ClutchHealer)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostProtection =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MostProtection)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedZeroDeaths =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.ZeroDeaths)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostRoots = i.StatisticsWithAwards
                            .Where(j => j.MatchAwardType == (int)MatchAwardType.MostRoots).Sum(j => j.GamesPlayed),
                    })
                .Select(
                    i => new MatchAwardContainerStatisticStandard
                    {
                        Character = i.Character,
                        GamesPlayedTotal = i.GamesPlayedTotal,
                        GamesPlayedWithAward = i.GamesPlayedWithAward,
                        PercentMVP = (decimal)i.GamesPlayedMVP / i.GamesPlayedTotal,
                        PercentHighestKillStreak =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedHighestKillStreak /
                                  (i.GamesPlayedHighestKillStreak + i.GamesPlayedWithoutAward),
                        PercentMostXPContribution =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedMostXPContribution /
                                  (i.GamesPlayedMostXPContribution + i.GamesPlayedWithoutAward),
                        PercentMostHeroDamageDone =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedMostHeroDamageDone /
                                  (i.GamesPlayedMostHeroDamageDone + i.GamesPlayedWithoutAward),
                        PercentMostSiegeDamageDone =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedMostSiegeDamageDone /
                                  (i.GamesPlayedMostSiegeDamageDone + i.GamesPlayedWithoutAward),
                        PercentMostDamageTaken =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedMostDamageTaken /
                                  (i.GamesPlayedMostDamageTaken + i.GamesPlayedWithoutAward),
                        PercentMostHealing =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedMostHealing /
                                  (i.GamesPlayedMostHealing + i.GamesPlayedWithoutAward),
                        PercentMostStuns =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedMostStuns /
                                  (i.GamesPlayedMostStuns + i.GamesPlayedWithoutAward),
                        PercentMostMercCampsCaptured =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedMostMercCampsCaptured /
                                  (i.GamesPlayedMostMercCampsCaptured + i.GamesPlayedWithoutAward),
                        PercentMapSpecific =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedMapSpecific /
                                  (i.GamesPlayedMapSpecific + i.GamesPlayedWithoutAward),
                        PercentMostKills =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedMostKills /
                                  (i.GamesPlayedMostKills + i.GamesPlayedWithoutAward),
                        PercentHatTrick =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedHatTrick / (i.GamesPlayedHatTrick + i.GamesPlayedWithoutAward),
                        PercentClutchHealer =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedClutchHealer /
                                  (i.GamesPlayedClutchHealer + i.GamesPlayedWithoutAward),
                        PercentMostProtection =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedMostProtection /
                                  (i.GamesPlayedMostProtection + i.GamesPlayedWithoutAward),
                        PercentZeroDeaths =
                            i.GamesPlayedWithoutAward == 0
                                ? 1m
                                : (decimal)i.GamesPlayedZeroDeaths /
                                  (i.GamesPlayedZeroDeaths + i.GamesPlayedWithoutAward),
                        PercentMostRoots = i.GamesPlayedWithoutAward == 0
                            ? 1m
                            : (decimal)i.GamesPlayedMostRoots / (i.GamesPlayedMostRoots + i.GamesPlayedWithoutAward),
                    })
                .Select(
                    i => new MatchAwardContainerStatisticStandard
                    {
                        Character = i.Character,
                        GamesPlayedTotal = i.GamesPlayedTotal,
                        GamesPlayedWithAward = i.GamesPlayedWithAward,
                        PercentMVP = i.PercentMVP,
                        PercentHighestKillStreak = i.PercentHighestKillStreak,
                        PercentMostXPContribution = i.PercentMostXPContribution,
                        PercentMostHeroDamageDone = i.PercentMostHeroDamageDone,
                        PercentMostSiegeDamageDone = i.PercentMostSiegeDamageDone,
                        PercentMostDamageTaken = i.PercentMostDamageTaken > 0 ? i.PercentMostDamageTaken : null,
                        PercentMostHealing = i.PercentMostHealing > 0 ? i.PercentMostHealing : null,
                        PercentMostStuns = i.PercentMostStuns > 0 ? i.PercentMostStuns : null,
                        PercentMostMercCampsCaptured = i.PercentMostMercCampsCaptured,
                        PercentMapSpecific = i.PercentMapSpecific,
                        PercentMostKills = i.PercentMostKills,
                        PercentHatTrick = i.PercentHatTrick,
                        PercentClutchHealer = i.PercentClutchHealer > 0 ? i.PercentClutchHealer : null,
                        PercentMostProtection = i.PercentMostProtection > 0 ? i.PercentMostProtection : null,
                        PercentZeroDeaths = i.PercentZeroDeaths,
                        PercentMostRoots = i.PercentMostRoots > 0 ? i.PercentMostRoots : null,
                    })
                .OrderBy(i => i.Character).ToArray(),
            StatisticMapObjectives = matchAwardSummariesList
                .GroupBy(i => localizationAliasPrimaryNameDictionary[i.CharacterID])
                .Select(
                    i => new
                    {
                        Character = i.Key,
                        GamesPlayedTotal = i.Sum(j => j.GamesPlayed),
                        GamesPlayedWithAward = i.Where(j => j.MatchAwardType.HasValue).Sum(j => j.GamesPlayed),
                        GamesPlayedWithoutAwardByMapIDDictionary =
                            i.Where(j => !j.MatchAwardType.HasValue && j.MapID > 1000)
                                .ToDictionary(j => j.MapID, j => j.GamesPlayed),
                        StatisticsWithAwards = i.Where(j => j.MatchAwardType.HasValue && j.MapID == j.MatchAwardType)
                            .ToArray(),
                    })
                .Select(
                    i => new
                    {
                        i.Character,
                        i.GamesPlayedTotal,
                        i.GamesPlayedWithAward,
                        i.GamesPlayedWithoutAwardByMapIDDictionary,
                        i.StatisticsWithAwards,
                        GamesPlayedMostImmortalDamage =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostImmortalDamage)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostCoinsPaid =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MostCoinsPaid)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostCurseDamageDone =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostCurseDamageDone)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostDragonShrinesCaptured =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostDragonShrinesCaptured)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostDamageToPlants =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostDamageToPlants)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostDamageToMinions =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostDamageToMinions)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostTimeInTemple =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MostTimeInTemple)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostGemsTurnedIn =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MostGemsTurnedIn)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostAltarDamage =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MostAltarDamage)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostDamageDoneToZerg =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostDamageDoneToZerg)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostNukeDamageDone =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostNukeDamageDone)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostSkullsCollected =
                            i.StatisticsWithAwards
                                .Where(j => j.MatchAwardType == (int)MatchAwardType.MostSkullsCollected)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostTimePushing =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MostTimePushing)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostTimeOnPoint =
                            i.StatisticsWithAwards.Where(j => j.MatchAwardType == (int)MatchAwardType.MostTimeOnPoint)
                                .Sum(j => j.GamesPlayed),
                        GamesPlayedMostInterruptedCageUnlocks = i.StatisticsWithAwards
                            .Where(j => j.MatchAwardType == (int)MatchAwardType.MostInterruptedCageUnlocks)
                            .Sum(j => j.GamesPlayed),
                    }).Select(
                    i => new MatchAwardContainerStatisticMapObjectives
                    {
                        Character = i.Character,
                        GamesPlayedTotal = i.GamesPlayedTotal,
                        GamesPlayedWithAward = i.GamesPlayedWithAward,
                        PercentMostImmortalDamage =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey(
                                (int)MatchAwardType.MostImmortalDamage)
                                ? 0m
                                : (decimal)i.GamesPlayedMostImmortalDamage / (i.GamesPlayedMostImmortalDamage +
                                                                              i.GamesPlayedWithoutAwardByMapIDDictionary
                                                                              [(int)MatchAwardType
                                                                                  .MostImmortalDamage]),
                        PercentMostCoinsPaid =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey((int)MatchAwardType.MostCoinsPaid)
                                ? 0m
                                : (decimal)i.GamesPlayedMostCoinsPaid / (i.GamesPlayedMostCoinsPaid +
                                                                         i.GamesPlayedWithoutAwardByMapIDDictionary[
                                                                             (int)MatchAwardType.MostCoinsPaid]),
                        PercentMostCurseDamageDone =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey(
                                (int)MatchAwardType.MostCurseDamageDone)
                                ? 0m
                                : (decimal)i.GamesPlayedMostCurseDamageDone / (i.GamesPlayedMostCurseDamageDone +
                                                                               i.GamesPlayedWithoutAwardByMapIDDictionary
                                                                               [(int)MatchAwardType
                                                                                   .MostCurseDamageDone]),
                        PercentMostDragonShrinesCaptured =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey(
                                (int)MatchAwardType.MostDragonShrinesCaptured)
                                ? 0m
                                : (decimal)i.GamesPlayedMostDragonShrinesCaptured /
                                  (i.GamesPlayedMostDragonShrinesCaptured +
                                   i.GamesPlayedWithoutAwardByMapIDDictionary[(int)MatchAwardType
                                       .MostDragonShrinesCaptured]),
                        PercentMostDamageToPlants =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey(
                                (int)MatchAwardType.MostDamageToPlants)
                                ? 0m
                                : (decimal)i.GamesPlayedMostDamageToPlants / (i.GamesPlayedMostDamageToPlants +
                                                                              i.GamesPlayedWithoutAwardByMapIDDictionary
                                                                              [(int)MatchAwardType
                                                                                  .MostDamageToPlants]),
                        PercentMostDamageToMinions =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey(
                                (int)MatchAwardType.MostDamageToMinions)
                                ? 0m
                                : (decimal)i.GamesPlayedMostDamageToMinions / (i.GamesPlayedMostDamageToMinions +
                                                                               i.GamesPlayedWithoutAwardByMapIDDictionary
                                                                               [(int)MatchAwardType
                                                                                   .MostDamageToMinions]),
                        PercentMostTimeInTemple =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey(
                                (int)MatchAwardType.MostTimeInTemple)
                                ? 0m
                                : (decimal)i.GamesPlayedMostTimeInTemple / (i.GamesPlayedMostTimeInTemple +
                                                                            i.GamesPlayedWithoutAwardByMapIDDictionary[
                                                                                (int)MatchAwardType.MostTimeInTemple]),
                        PercentMostGemsTurnedIn =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey(
                                (int)MatchAwardType.MostGemsTurnedIn)
                                ? 0m
                                : (decimal)i.GamesPlayedMostGemsTurnedIn / (i.GamesPlayedMostGemsTurnedIn +
                                                                            i.GamesPlayedWithoutAwardByMapIDDictionary[
                                                                                (int)MatchAwardType.MostGemsTurnedIn]),
                        PercentMostAltarDamage =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey((int)MatchAwardType.MostAltarDamage)
                                ? 0m
                                : (decimal)i.GamesPlayedMostAltarDamage / (i.GamesPlayedMostAltarDamage +
                                                                           i.GamesPlayedWithoutAwardByMapIDDictionary[
                                                                               (int)MatchAwardType.MostAltarDamage]),
                        PercentMostDamageDoneToZerg =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey(
                                (int)MatchAwardType.MostDamageDoneToZerg)
                                ? 0m
                                : (decimal)i.GamesPlayedMostDamageDoneToZerg / (i.GamesPlayedMostDamageDoneToZerg +
                                    i.GamesPlayedWithoutAwardByMapIDDictionary
                                        [(int)MatchAwardType.MostDamageDoneToZerg]),
                        PercentMostNukeDamageDone =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey(
                                (int)MatchAwardType.MostNukeDamageDone)
                                ? 0m
                                : (decimal)i.GamesPlayedMostNukeDamageDone / (i.GamesPlayedMostNukeDamageDone +
                                                                              i.GamesPlayedWithoutAwardByMapIDDictionary
                                                                              [(int)MatchAwardType
                                                                                  .MostNukeDamageDone]),
                        PercentMostSkullsCollected =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey(
                                (int)MatchAwardType.MostSkullsCollected)
                                ? 0m
                                : (decimal)i.GamesPlayedMostSkullsCollected / (i.GamesPlayedMostSkullsCollected +
                                                                               i.GamesPlayedWithoutAwardByMapIDDictionary
                                                                               [(int)MatchAwardType
                                                                                   .MostSkullsCollected]),
                        PercentMostTimePushing =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey((int)MatchAwardType.MostTimePushing)
                                ? 0m
                                : (decimal)i.GamesPlayedMostTimePushing / (i.GamesPlayedMostTimePushing +
                                                                           i.GamesPlayedWithoutAwardByMapIDDictionary[
                                                                               (int)MatchAwardType.MostTimePushing]),
                        PercentMostTimeOnPoint =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey((int)MatchAwardType.MostTimeOnPoint)
                                ? 0m
                                : (decimal)i.GamesPlayedMostTimeOnPoint / (i.GamesPlayedMostTimeOnPoint +
                                                                           i.GamesPlayedWithoutAwardByMapIDDictionary[
                                                                               (int)MatchAwardType.MostTimeOnPoint]),
                        PercentMostInterruptedCageUnlocks =
                            !i.GamesPlayedWithoutAwardByMapIDDictionary.ContainsKey(
                                (int)MatchAwardType.MostInterruptedCageUnlocks)
                                ? 0m
                                : (decimal)i.GamesPlayedMostInterruptedCageUnlocks /
                                  (i.GamesPlayedMostInterruptedCageUnlocks +
                                   i.GamesPlayedWithoutAwardByMapIDDictionary[(int)MatchAwardType
                                       .MostInterruptedCageUnlocks]),
                    }).Select(
                    i => new MatchAwardContainerStatisticMapObjectives
                    {
                        Character = i.Character,
                        GamesPlayedTotal = i.GamesPlayedTotal,
                        GamesPlayedWithAward = i.GamesPlayedWithAward,
                        PercentMostImmortalDamage =
                            i.PercentMostImmortalDamage != 0m ? i.PercentMostImmortalDamage : null,
                        PercentMostCoinsPaid = i.PercentMostCoinsPaid != 0m ? i.PercentMostCoinsPaid : null,
                        PercentMostCurseDamageDone =
                            i.PercentMostCurseDamageDone != 0m ? i.PercentMostCurseDamageDone : null,
                        PercentMostDragonShrinesCaptured =
                            i.PercentMostDragonShrinesCaptured != 0m ? i.PercentMostDragonShrinesCaptured : null,
                        PercentMostDamageToPlants =
                            i.PercentMostDamageToPlants != 0m ? i.PercentMostDamageToPlants : null,
                        PercentMostDamageToMinions =
                            i.PercentMostDamageToMinions != 0m ? i.PercentMostDamageToMinions : null,
                        PercentMostTimeInTemple = i.PercentMostTimeInTemple != 0m ? i.PercentMostTimeInTemple : null,
                        PercentMostGemsTurnedIn = i.PercentMostGemsTurnedIn != 0m ? i.PercentMostGemsTurnedIn : null,
                        PercentMostAltarDamage = i.PercentMostAltarDamage != 0m ? i.PercentMostAltarDamage : null,
                        PercentMostDamageDoneToZerg =
                            i.PercentMostDamageDoneToZerg != 0m ? i.PercentMostDamageDoneToZerg : null,
                        PercentMostNukeDamageDone =
                            i.PercentMostNukeDamageDone != 0m ? i.PercentMostNukeDamageDone : null,
                        PercentMostSkullsCollected =
                            i.PercentMostSkullsCollected != 0m ? i.PercentMostSkullsCollected : null,
                        PercentMostTimePushing = i.PercentMostTimePushing != 0m ? i.PercentMostTimePushing : null,
                        PercentMostTimeOnPoint = i.PercentMostTimeOnPoint != 0m ? i.PercentMostTimeOnPoint : null,
                        PercentMostInterruptedCageUnlocks = i.PercentMostInterruptedCageUnlocks != 0m
                            ? i.PercentMostInterruptedCageUnlocks
                            : null,
                    }).OrderBy(i => i.Character).ToArray(),
        };
    }

    private class MatchAwardSummary
    {
        public int MapID { get; set; }
        public int CharacterID { get; set; }
        public int? MatchAwardType { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
    }

    public class QueryBuilder
    {
        public string QueryText { get; init; }
        public string JoinRanking { get; init; }
        public string League { get; init; }
        public string Map { get; init; }
        public string Players { get; init; }
        public string BuildNumber { get; init; }
        public string IncludeTalents { get; init; }
        public string Character { get; init; }
        public string UseIndex { get; init; }
        public string GameModes { get; init; }
        public string Spec1 { get; init; }
        public string Spec2 { get; init; }
        public string Spec3 { get; init; }
        public string Replays { get; init; }

        public string Build()
        {
            return string.Format(
                QueryText,
                JoinRanking,
                League,
                Map,
                Players,
                BuildNumber,
                IncludeTalents,
                Character,
                UseIndex,
                GameModes,
                Spec1,
                Spec2,
                Spec3,
                Replays);
        }
    }

    public static async Task<int> CountGames(int replayBuild, int[] gameModes, int? upToBuildArg)
    {
        var upToBuild = upToBuildArg ?? replayBuild;
        using var scope = _svcp.CreateScope();
        var heroesEntity = HeroesdataContext.Create(scope);
        heroesEntity.Database.SetCommandTimeout(Int32.MaxValue);
        return await heroesEntity.Replays.CountAsync(
            x =>
                x.ReplayBuild >= replayBuild && x.ReplayBuild <= upToBuild && gameModes.Contains(x.GameMode));
    }
}
