// Copyright (c) StarkillerLLC. All rights reserved.

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Foole.Mpq;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Heroes.ReplayParser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using DataPlayer = Heroes.DataAccessLayer.Models.Player;
using DataReplay = Heroes.DataAccessLayer.Models.Replay;
using ParserPlayer = Heroes.ReplayParser.Player;
using ParserReplay = Heroes.ReplayParser.Replay;
// ReSharper disable HeuristicUnreachableCode

namespace HelperCore;

public static partial class DataHelper
{
    private static Tuple<DataParser.ReplayParseResult, Guid?> AddReplay(
        HeroesdataContext heroesEntity,
        LocalizationAlias[] localizationAliases,
        Tuple<DataParser.ReplayParseResult, ParserReplay> parsedReplay,
        byte[] replayFileBytes,
        string ipAddress,
        Action<string> logFunction = null,
        int? eventId = null)
    {
        using var transaction = heroesEntity.Database.BeginTransaction();
        var rc = AddReplayWithoutTransaction(
            heroesEntity,
            localizationAliases,
            parsedReplay,
            replayFileBytes,
            ipAddress,
            logFunction,
            eventId);
        if (rc.Item1 == DataParser.ReplayParseResult.Success)
        {
            try
            {
                transaction.Commit();
            }
            catch (Exception e)
            {
                logFunction?.Invoke("Error during commit " + e);
            }
        }
        else
        {
            logFunction?.Invoke($"Not adding replay due to result {rc.Item1}");
        }

        return rc;
    }

    public static (bool, List<int>) SanityDupCheck(HeroesdataContext heroesEntity, DataReplay replay)
    {
        var sw = new Stopwatch();
        sw.Start();
        var dt1 = replay.TimestampReplay.AddSeconds(-15);
        var dt2 = replay.TimestampReplay.AddSeconds(15);
        var sanityDupCheck = (from r in heroesEntity.Replays
                              join rc in heroesEntity.ReplayCharacters on r.ReplayId equals rc.ReplayId
                              where r.TimestampReplay >= dt1 && r.TimestampReplay <= dt2
                              select new
                              {
                                  rc.PlayerId,
                                  rc.ReplayId,
                              })
            .ToLookup(x => x.PlayerId, x => x.ReplayId);

        var dups = replay.ReplayCharacters
            .Where(x => x.Player != null) // safety, probably not necessary -- Aviad
            .Where(x => sanityDupCheck.Contains(x.Player.PlayerId))
            .SelectMany(x => sanityDupCheck[x.Player.PlayerId])
            .Distinct()
            .ToList();
        var dupDetected = dups.Any();
        sw.Stop();
        var elapsed = sw.ElapsedMilliseconds;
        Debug.Print($"sanity check {elapsed} msec");
        return (dupDetected, dups);
    }

    private static Tuple<DataParser.ReplayParseResult, Guid?> AddReplayWithoutTransaction(
        HeroesdataContext heroesEntity,
        LocalizationAlias[] localizationAliases,
        Tuple<DataParser.ReplayParseResult, ParserReplay> parsedReplay,
        byte[] replayFileBytes,
        string ipAddress,
        Action<string> logFunction = null,
        int? eventId = null)
    {
        logFunction?.Invoke("Made it into AddReplay");

        var replayParseData = parsedReplay.Item2;
        var replayParseResult = parsedReplay.Item1;
        if (replayParseResult != DataParser.ReplayParseResult.Success || replayParseData == null)
        {
            logFunction?.Invoke("Passing control back to form...");
            return Tuple.Create(replayParseResult, (Guid?)null);
        }

        // Check if replay has no talent information
        var anyTalents = replayParseData.Players.SelectMany(x => x.Talents).Any();
        if (!anyTalents)
        {
            logFunction?.Invoke("Replay has no player talent information, rejecting.");
            return Tuple.Create(DataParser.ReplayParseResult.Exception, (Guid?)null);
        }

        var repeatingTalents = replayParseData.Players
            .SelectMany(x => x.Talents, (a, b) => (a, b.TalentID))
            .GroupBy(x => x)
            .Any(x => x.Count() > 1);

        var superfluousTalents = replayParseData.Players.Any(x => x.Talents.Length > 7);

        if (repeatingTalents || superfluousTalents)
        {
            logFunction?.Invoke("Replay has bad talent information, rejecting.");
            return Tuple.Create(DataParser.ReplayParseResult.Exception, (Guid?)null);
        }

        // Set the Replay Hash
        Guid replayHash;

        if (replayParseData.HOTSAPIFingerprint.IsNullOrEmpty())
        {
            try
            {
                replayHash = replayParseData.HashReplay();
            }
            catch
            {
                logFunction?.Invoke("Couldn't hash replay, not adding it... -- Aviad");
                return Tuple.Create(replayParseResult, (Guid?)null);
            }

            replayParseData.HOTSAPIFingerprint = replayHash.ToString();
        }
        else
        {
            // If we're getting a fingerprint from the caller, then it means we should
            // use it as the replay hash, and not recalculate, because if we recalculate
            // the "Random Number" might be different and we'd miss a duplicate... -- Aviad
            replayHash = Guid.Parse(replayParseData.HOTSAPIFingerprint);
        }

        logFunction?.Invoke("Checking for duplicate...");
        // Check for duplicate replay
        var alreadyUploadedReplayId = GetReplayID(replayHash);

        // Check if we are uploading an Event replay
        if (eventId.HasValue)
        {
            // Only accept Custom games
            if (replayParseData.GameMode != GameMode.Custom)
            {
                return Tuple.Create(DataParser.ReplayParseResult.UnexpectedResult, (Guid?)null);
            }

            // If this replay has already been uploaded, let's delete the old and reimport it
            if (alreadyUploadedReplayId.HasValue)
            {
                heroesEntity.Replays.Remove(
                    heroesEntity.Replays.Single(
                        i =>
                            i.ReplayId == alreadyUploadedReplayId.Value));
                heroesEntity.SaveChanges();
            }
        }
        else if (alreadyUploadedReplayId.HasValue)
        {
            logFunction?.Invoke($"Duplicate hash: {replayHash}, id: {alreadyUploadedReplayId}");
            return Tuple.Create(DataParser.ReplayParseResult.Duplicate, (Guid?)replayHash);
        }

        // Check for funny business
        logFunction?.Invoke("Checking for 'FunnyBusiness' - shouldn't, because this is parsed...");
        var isFunnyBusiness = false;

        if (replayFileBytes != null)
        {
            if (!eventId.HasValue)
            {
                using var memoryStream = new MemoryStream(replayFileBytes);
                using var archive = new MpqArchive(memoryStream);
                archive.AddListfileFilenames();

                isFunnyBusiness =
                    archive.Header.MpqVersion != 3 ||
                    archive.Header.BlockSize != 5 ||
                    archive.Header.HashTableSize != 32 ||
                    archive.BlockSize != 16384 ||
                    /* Tolerate replays without battlelobby file - not sure why it happens, but they seem legit */
                    (archive.Count != 14 && archive.Count != 13) ||
                    !archive[^1].IsSingleUnit ||
                    !archive[^2].IsSingleUnit ||
                    archive._hashes.Length != 32 ||
                    !archive.FileExists("replay.sync.history") ||
                    archive["replay.sync.history"].IsSingleUnit ||
                    archive["replay.sync.history"].FilePos != 1024;

                if (!isFunnyBusiness)
                {
                    var matchingHashIndexes = new[]
                    {
                        2, 3, 4, 8, 9, 11, 12, 13, 17, 18, 20, 22, 23, 24, 26, 27, 28, 29,
                    };
                    foreach (var matchingHashIndex in matchingHashIndexes)
                    {
                        if (archive._hashes[matchingHashIndex].BlockIndex != 4294967295 &&
                            archive._hashes[matchingHashIndex].Locale !=
                            archive._hashes[matchingHashIndex].BlockIndex &&
                            archive._hashes[matchingHashIndex].Name1 !=
                            archive._hashes[matchingHashIndex].BlockIndex &&
                            archive._hashes[matchingHashIndex].Name2 !=
                            archive._hashes[matchingHashIndex].BlockIndex)
                        {
                            isFunnyBusiness = true;
                        }
                    }
                }

                if (!isFunnyBusiness)
                {
                    foreach (var file in archive)
                    {
                        if (!file.IsCompressed)
                        {
                            isFunnyBusiness = true;
                        }
                    }
                }
            }
        }

        var nowString = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (isFunnyBusiness)
        {
            // Funny business detected

            using var amazonS3Client = new AmazonS3Client(AWSAccessKeyID, AWSSecretAccessKey, RegionEndpoint.USWest2);
            using var memoryStream = new MemoryStream(replayFileBytes);
            amazonS3Client.PutObject(
                new PutObjectRequest
                {
                    BucketName = "heroesreplays-admin",
                    Key = $"MaliciousUser {ipAddress} {nowString} {replayHash}",
                    InputStream = memoryStream,
                    StorageClass = S3StorageClass.StandardInfrequentAccess,
                });

            return Tuple.Create(replayParseResult, (Guid?)replayHash);
        }

        logFunction?.Invoke("Passed checks, localizing data...");
        // Change Map and Hero Name to English Version
        var localizationAliasesScrubbed = localizationAliases
            .Select(
                i => new LocEntry(
                    i.IdentifierId,
                    i.Type,
                    i.PrimaryName,
                    i.AliasesCsv?.Split(',') ?? Array.Empty<string>()))
            .ToList();

        bool IsPrimaryName(LocalizationAliasType type, string name, LocEntry locEntry) =>
            locEntry.Type == (int)type && locEntry.PrimaryName == name;

        bool IsAlias(string name, string alias)
        {
            var noCommas = name.Replace(",", string.Empty);
            return string.Equals(alias, noCommas, StringComparison.InvariantCultureIgnoreCase);
        }

        bool HasAliasInSet(LocalizationAliasType type, string name, LocEntry locEntry) =>
            locEntry.Type == (int)type &&
            locEntry.Aliases.Any(alias => IsAlias(name, alias));

        bool IsPlayerPrimaryName(ParserPlayer player, LocEntry locEntry) =>
            IsPrimaryName(LocalizationAliasType.Hero, player.Character, locEntry);

        bool PlayerHasNoPrimaryName(ParserPlayer player) =>
            player != null &&
            !localizationAliasesScrubbed.Any(locEntry => IsPlayerPrimaryName(player, locEntry));

        bool PlayerHasAliasInSet(ParserPlayer player, LocEntry locEntry) =>
            HasAliasInSet(LocalizationAliasType.Hero, player.Character, locEntry);

        foreach (var player in replayParseData.Players.Where(PlayerHasNoPrimaryName))
        {
            if (localizationAliasesScrubbed.Any(locEntry => PlayerHasAliasInSet(player, locEntry)))
            {
                player.Character = localizationAliasesScrubbed
                    .Single(locEntry => PlayerHasAliasInSet(player, locEntry)).PrimaryName;
            }
        }

        // Determine map
        bool IsMapPrimaryName(LocEntry locEntry) =>
            IsPrimaryName(LocalizationAliasType.Map, replayParseData.Map, locEntry);

        bool MapHasAliasInSet(LocEntry locEntry) =>
            HasAliasInSet(LocalizationAliasType.Map, replayParseData.Map, locEntry);

        if (!localizationAliasesScrubbed.Any(IsMapPrimaryName) &&
            localizationAliasesScrubbed.Any(MapHasAliasInSet))
        {
            replayParseData.Map = localizationAliasesScrubbed.Single(MapHasAliasInSet).PrimaryName;
        }

        var locNameToId =
            localizationAliases.ToCaseInsensitiveDictionary(i => i.PrimaryName, i => i.IdentifierId);

        var locAttrDic = localizationAliases
            .Where(x => x.Type == 0)
            .ToCaseInsensitiveDictionary(x => x.AttributeName, x => x.IdentifierId);

        var mapId = locNameToId.GetValueOrDefault(replayParseData.Map);
        if (mapId == 0 && locAttrDic.ContainsKey(replayParseData.MapAlternativeName ?? string.Empty))
        {
            mapId = locAttrDic[replayParseData.MapAlternativeName];
        }

        if (mapId == 0)
        {
            logFunction?.Invoke($"Unknown map '{replayParseData.Map}'/'{replayParseData.MapAlternativeName}'");
        }

        logFunction?.Invoke("Building replay object...");

        var dt = replayParseData.Timestamp - DateTime.UtcNow;
        if (dt > TimeSpan.FromMinutes(10))
        {
            logFunction?.Invoke(
                "Rejecting replay with timestamp too far in the future " +
                $"({replayParseData.Timestamp.ToUniversalTime()} - {dt.TotalMinutes:N1} in the future) ");
            return Tuple.Create(DataParser.ReplayParseResult.Exception, (Guid?)replayHash);
        }

        var replay = new DataReplay
        {
            ReplayBuild = replayParseData.ReplayBuild,
            GameMode = (int)replayParseData.GameMode,
            MapId = mapId,
            ReplayLength = replayParseData.ReplayLength,
            ReplayHash = replayHash.ToByteArray(),
            TimestampReplay = replayParseData.Timestamp,
            TimestampCreated = DateTime.UtcNow,
            Hotsapifingerprint = replayParseData.HOTSAPIFingerprint,
        };

        if (eventId.HasValue)
        {
            replay.GameMode = eventId.Value;
        }

        logFunction?.Invoke("Parsing replay into entity...");
        heroesEntity.Replays.Add(replay);

        // If the Map is unknown, add them to the 'UnknownData' table so we can assign them later
        if (replay.MapId == 0 && !heroesEntity.UnknownData.Any(i => i.UnknownData == replayParseData.Map))
        {
            heroesEntity.UnknownData.Add(new UnknownDatum { UnknownData = replayParseData.Map });
        }

        if (eventId.HasValue)
        {
            foreach (var player in replayParseData.Players)
            {
                var playersInDatabase = heroesEntity.Players
                    .Include(x => x.PlayerDisableNameChange)
                    .Where(
                        i => i.BattleNetRegionId == player.BattleNetRegionId &&
                             i.BattleNetSubId > 0 &&
                             i.BattleNetId == player.BattleNetId)
                    .OrderByDescending(i => i.BattleNetSubId)
                    .ToArray();

                if (playersInDatabase.Any(i => i.Name == player.Name))
                {
                    // This player exists - let's grab the correct BattleNetSubId
                    var correctPlayer = playersInDatabase.First(i => i.Name == player.Name);
                    player.BattleNetSubId = correctPlayer.BattleNetSubId;

                    correctPlayer.PlayerDisableNameChange ??= new PlayerDisableNameChange();
                }
                else if (playersInDatabase.Length > 0)
                {
                    // This player does not exist - let's allocate a new BattleNetSubId
                    player.BattleNetSubId = playersInDatabase[0].BattleNetSubId + 1;
                }

                // No 'else' clause - If there are no entries in 'playersInDatabase', one will
                // be allocated automatically, as BattleNetSubId = 1 by default
            }
        }

        logFunction?.Invoke("Parsing heroes into replay entity...");

        DataPlayer[] players;
        try
        {
            var playerRelation = replayParseData.Players
                .Select(
                    replayPlayer => (replayPlayer, dbPlayer: heroesEntity.Players
                        .Include(x => x.Net48Users)
                        .SingleOrDefault(
                            j =>
                                j.BattleNetRegionId == replayPlayer.BattleNetRegionId &&
                                j.BattleNetSubId == replayPlayer.BattleNetSubId &&
                                j.BattleNetId == replayPlayer.BattleNetId)))
                .ToList();

            players = playerRelation.Select(x => x.dbPlayer).ToArray();
        }
        catch (Exception e)
        {
            logFunction?.Invoke($"Error while processing players {e}");
            return Tuple.Create(DataParser.ReplayParseResult.Exception, (Guid?)replayHash);
            // throw;
        }

        var draftOrderArray = replayParseData.DraftOrder?
            .Where(x => x.PickType == DraftPickType.Picked)
            .Select(x => x.HeroSelected)
            .ToArray() ?? Array.Empty<string>();

        var replayCharacters = new ReplayCharacter[10];
        var unknownCharacters = new List<string>();
        for (var i = 0; i < players.Length; i++)
        {
            // Don't save statistics for AI players
            if (replayParseData.Players[i].PlayerType == PlayerType.Computer)
            {
                continue;
            }

            if (players[i] == null)
            {
                players[i] = new DataPlayer
                {
                    BattleNetRegionId = replayParseData.Players[i].BattleNetRegionId,
                    BattleNetSubId = replayParseData.Players[i].BattleNetSubId,
                    BattleNetId = replayParseData.Players[i].BattleNetId,
                    Name = replayParseData.Players[i].Name,
                    BattleTag = replayParseData.Players[i].BattleTag,
                    TimestampCreated = DateTime.UtcNow,
                    PlayerDisableNameChange = eventId.HasValue ? new PlayerDisableNameChange() : null,
                };
            }
            else if (players[i].Name != replayParseData.Players[i].Name ||
                     (players[i].BattleTag != replayParseData.Players[i].BattleTag &&
                      replayParseData.Players[i].BattleTag != 0))
            {
#pragma warning disable 162
                /* If we serve eSports games in the future, remove the 'false &&' from this */
                if (false && players[i].PlayerDisableNameChange != null)
                {
                    // Tournament Account - Don't allow name changes
                    // See if we can find a previous account with this name
                    var playerEntity = players[i];

                    var playersInDatabase = heroesEntity.Players
                        .Where(
                            j => j.BattleNetRegionId == playerEntity.BattleNetRegionId &&
                                 j.BattleNetSubId > 0 &&
                                 j.BattleNetId == playerEntity.BattleNetId)
                        .OrderByDescending(j => j.BattleNetSubId)
                        .ToArray();

                    if (playersInDatabase.Any(j => j.Name == replayParseData.Players[i].Name))
                    {
                        // This player exists - let's grab the correct BattleNetSubId
                        players[i] = playersInDatabase.First(j => j.Name == replayParseData.Players[i].Name);
                    }
                    else
                    {
                        players[i] = new DataPlayer
                        {
                            BattleNetRegionId = replayParseData.Players[i].BattleNetRegionId,
                            BattleNetSubId = playersInDatabase[0].BattleNetSubId + 1,
                            BattleNetId = replayParseData.Players[i].BattleNetId,
                            Name = replayParseData.Players[i].Name,
                            BattleTag = replayParseData.Players[i].BattleTag,
                            TimestampCreated = DateTime.UtcNow,
                            PlayerDisableNameChange = new PlayerDisableNameChange(),
                        };
                    }
                }
#pragma warning restore 162

                var latestReplay = heroesEntity.ReplayCharacters
                    .Include(x => x.Replay)
                    .Where(x => x.PlayerId == players[i].PlayerId)
                    .Select(x => x.Replay.TimestampReplay)
                    .Max();

                if (latestReplay < replay.TimestampReplay)
                {
                    // Player Name or BattleTag Change
                    players[i].Name = replayParseData.Players[i].Name;
                    players[i].BattleTag = replayParseData.Players[i].BattleTag;

                    // Make sure the dictionary is updated with the new name
                    using var scope = _svcp.CreateScope();
                    var playerNameDictionary = scope.ServiceProvider.GetRequiredService<PlayerNameDictionary>();
                    playerNameDictionary.InvalidateDictionaryPlayerID(players[i].PlayerId);
                }
            }

            if (players[i].PlayerId == 0)
            {
                heroesEntity.Players.Add(players[i]);
            }

            var locHeroAttrDic = localizationAliases
                .Where(x => x.Type == 1)
                .ToCaseInsensitiveDictionary(x => x.AttributeName, x => x.IdentifierId);

            var characterId = locNameToId.GetValueOrDefault(replayParseData.Players[i].Character);

            if (characterId == 0 &&
                locHeroAttrDic.ContainsKey(replayParseData.Players[i].HeroAttributeId ?? string.Empty))
            {
                logFunction?.Invoke(
                    $"Resolving hero {replayParseData.Players[i].Character} by attribute {replayParseData.Players[i].HeroAttributeId}");
                characterId = locHeroAttrDic[replayParseData.Players[i].HeroAttributeId];
            }

            var draftOrder = Array.IndexOf(draftOrderArray, replayParseData.Players[i].HeroId);

            var replayCharacter = new ReplayCharacter
            {
                Player = players[i],
                IsAutoSelect = replayParseData.Players[i].IsAutoSelect ? 1ul : 0,
                CharacterId = characterId,
                CharacterLevel = !replayParseData.Players[i].IsAutoSelect
                    ? replayParseData.Players[i].CharacterLevel
                    : 0,
                IsWinner = replayParseData.Players[i].IsWinner ? 1ul : 0,
                Mmrbefore = null,
                Mmrchange = null,
            };

            if (draftOrder != -1)
            {
                replayCharacter.ReplayCharacterDraftOrder = new ReplayCharacterDraftOrder
                {
                    DraftOrder = draftOrder,
                };
            }

            if (!locNameToId.ContainsKey(replayParseData.Players[i].Character) &&
                unknownCharacters.All(j => j != replayParseData.Players[i].Character))
            {
                unknownCharacters.Add(replayParseData.Players[i].Character);
            }

            replayCharacters[i] = replayCharacter;
            replay.ReplayCharacters.Add(replayCharacter);
        }

        if (replayCharacters.Count(x => x?.ReplayCharacterDraftOrder != null) != 10)
        {
            // All or nothing...
            foreach (var x in replayCharacters.Where(x => x != null))
            {
                x.ReplayCharacterDraftOrder = null;
            }
        }

        logFunction?.Invoke("Checking for unknown heroes...");
        // If the Character is unknown, add them to the 'UnknownData' table so we can assign them later
        foreach (var unknownCharacter in unknownCharacters)
        {
            if (!heroesEntity.UnknownData.Any(j => j.UnknownData == unknownCharacter))
            {
                heroesEntity.UnknownData.Add(new UnknownDatum { UnknownData = unknownCharacter });
                // AddReplayOutput("Unknown Character: " + unknownCharacter); <!-- Didn't work. -DR -->
            }
        }

        var (isDup, dups) = SanityDupCheck(heroesEntity, replay);
        if (isDup)
        {
            var dupIds = string.Join(",", dups);
            logFunction?.Invoke(
                $"Replay {replayHash} rejected because these replays overlap it and have common players: {dupIds}");
            return Tuple.Create(DataParser.ReplayParseResult.Duplicate, (Guid?)replayHash);
        }

        var teamBlueHeroes = new HashSet<int>(
            replayCharacters
                .Where(x => x is { IsWinner: not 0 })
                .Select(x => x.CharacterId));
        var teamRedHeroes = new HashSet<int>(
            replayCharacters
                .Where(x => x is { IsWinner: 0 })
                .Select(x => x.CharacterId));
        var isMirror = teamBlueHeroes.Intersect(teamRedHeroes).Any();
        var hasDuplicateHeros = replayCharacters
            .Where(x => x != null)
            .GroupBy(x => x.CharacterId)
            .Any(x => x.Count() > 1);

        logFunction?.Invoke("Adding replay and heroes to EntityFramework...");
        try
        {
            heroesEntity.SaveChanges();
            if (isMirror || hasDuplicateHeros)
            {
                logFunction?.Invoke(
                    $"Marking replay as mirror match (isMirror={isMirror}, hasDuplicateHeroes={hasDuplicateHeros})");
                heroesEntity.Database.ExecuteSqlInterpolated(
                    $"INSERT IGNORE INTO replay_mirror VALUES({replay.ReplayId})");
            }
        }
        catch (DbUpdateException exception)
        {
            logFunction?.Invoke($"DbUpdateException while saving replay: {exception}");
            return exception.ToString().Contains("Duplicate")
                ? Tuple.Create(DataParser.ReplayParseResult.Duplicate, (Guid?)replayHash)
                : Tuple.Create(DataParser.ReplayParseResult.Exception, (Guid?)null);
        }
        catch (Exception exception)
        {
            logFunction?.Invoke($"Exception while saving replay: {exception}");
            return Tuple.Create(DataParser.ReplayParseResult.Exception, (Guid?)null);
        }

        logFunction?.Invoke("Checking team objectives...");
        // Add Team Objectives
        for (var i = 0; i < replayParseData.TeamObjectives.Length; i++)
        {
            // First, make sure team objectives are not on the same TimeSpan
            foreach (var nonUniqueKeyEvent in replayParseData.TeamObjectives[i]
                         .GroupBy(
                             j => new
                             {
                                 j.TeamObjectiveType,
                                 j.TimeSpan,
                             })
                         .Where(j => j.Count() > 1)
                         .SelectMany(j => j)
                         .ToArray())
            {
                while (replayParseData.TeamObjectives[i].Any(
                           j =>
                               j != nonUniqueKeyEvent &&
                               j.TeamObjectiveType == nonUniqueKeyEvent.TeamObjectiveType &&
                               j.TimeSpan == nonUniqueKeyEvent.TimeSpan))
                {
                    nonUniqueKeyEvent.TimeSpan = nonUniqueKeyEvent.TimeSpan.Add(TimeSpan.FromSeconds(1));
                }
            }

            var isWinner = replayParseData.Players.First(j => j.Team == i).IsWinner;

            foreach (var teamObjective in replayParseData.TeamObjectives[i])
            {
                var rto = new ReplayTeamObjective
                {
                    ReplayId = replay.ReplayId,
                    IsWinner = isWinner ? 1ul : 0,
                    TeamObjectiveType = (int)teamObjective.TeamObjectiveType,
                    TimeSpan = teamObjective.TimeSpan,
                    Player = teamObjective.Player == null || teamObjective.Player.BattleNetId == 0
                        ? null
                        : players
                            .Where(j => j != null)
                            .Single(
                                j =>
                                    j.BattleNetRegionId == teamObjective.Player.BattleNetRegionId &&
                                    j.BattleNetId == teamObjective.Player.BattleNetId),
                    Value = teamObjective.Value,
                };
                replay.ReplayTeamObjectives.Add(rto);
                heroesEntity.ReplayTeamObjectives.Add(rto);
            }

            heroesEntity.SaveChanges();
        }

        // We will call this a 'Recent Replay' if it was played within the past 3 days
        var isRecentReplay = replay.TimestampReplay > DateTime.UtcNow.AddDays(-3);

        logFunction?.Invoke("Parsing players/score information into entity...");
        for (var i = 0; i < players.Length; i++)
        {
            // Don't save statistics for AI players
            if (replayParseData.Players[i].PlayerType == PlayerType.Computer)
            {
                continue;
            }

            // Add Talent Information
            var thePlayer = players
                .Where(j => j != null)
                .Single(
                    j =>
                        j.BattleNetRegionId == replayParseData.Players[i].BattleNetRegionId &&
                        j.BattleNetId == replayParseData.Players[i].BattleNetId);
            if (replayParseData.Players[i].Talents != null)
            {
                // Special Fix - In the 2nd 'Mage Wars' Brawl, Nazeebo's "Superstition" talent was removed
                if (replayParseData.GameMode == GameMode.Brawl &&
                    replayParseData.Map == "Tomb of the Spider Queen" &&
                    replayParseData.Players[i].Character == "Nazeebo" &&
                    replayParseData.Timestamp > new DateTime(2017, 3, 1) &&
                    replayParseData.Timestamp < new DateTime(2017, 3, 11))
                {
                    var replayCharacterTalents = replayParseData.Players[i].Talents
                        .Where(j => j.TalentID >= 12 /* 'Superstition' */);
                    foreach (var replayCharacterTalent in replayCharacterTalents)
                    {
                        replayCharacterTalent.TalentID++;
                    }
                }

                foreach (var replayCharacterTalent in replayParseData.Players[i].Talents)
                {
                    // adding replay and player ids so can add to heroesEntity more easily later
                    var rpc = new ReplayCharacterTalent
                    {
                        ReplayId = replay.ReplayId,
                        Player = thePlayer,
                        TalentId = replayCharacterTalent.TalentID,
                    };

                    replayCharacters[i].ReplayCharacterTalents.Add(rpc);
                    heroesEntity.ReplayCharacterTalents.Add(rpc);
                    // replayCharacters[i].ReplayCharacterTalents
                    //     .Add(new ReplayCharacterTalent { TalentID = replayCharacterTalent.TalentID });
                }

                try
                {
                    heroesEntity.SaveChanges();
                }
                catch (Exception)
                {
                    logFunction?.Invoke("PRIMARY error, retrying id " + replay.ReplayId);
                    return Tuple.Create(DataParser.ReplayParseResult.RetryForBanError, (Guid?)null);
                }
            }

#pragma warning disable 162
            // Add Silenced Player Flag
            if (replayParseData.Players[i].IsSilenced)
            {
                replayCharacters[i].ReplayCharacterSilenced = new ReplayCharacterSilenced();

                if (isRecentReplay && false /* Not currently penalizing silenced players */)
                {
                    using var scope = _svcp.CreateScope();
                    var redisClient = MyDbWrapper.Create(scope);
                    redisClient.TrySet(
                        "HOTSLogs:SilencedPlayerID:" + players[i].PlayerId,
                        "1",
                        DateTime.UtcNow.AddDays(13));

                    if (players[i].PlayerBanned == null)
                    {
                        // This is a recent replay - let's apply the silenced penalty to appropriate accounts
                        players[i].PlayerBanned = new PlayerBanned();

                        if (players[i].LeaderboardOptOut == null)
                        {
                            players[i].LeaderboardOptOut = new LeaderboardOptOut();
                        }
                        else
                        {
                            redisClient.TrySet(
                                "HOTSLogs:SilencedPlayerIDAndExistingLeaderboardOptOut:" +
                                players[i].PlayerId,
                                "1",
                                DateTime.UtcNow.AddDays(60));
                        }

                        // TODO: check this refactoring
                        //foreach (var myAspnetUser in players[i].Users.Select(x => x.UserNavigation)
                        //    .ToArray())
                        //{
                        //    if (myAspnetUser.MyAspnetProfile != null)
                        //    {
                        //        heroesEntity.MyAspnetProfiles.Remove(myAspnetUser.MyAspnetProfile);
                        //    }

                        //    players[i].Users.Remove(myAspnetUser.User);
                        //}
                    }
                    else if (redisClient.ContainsKey(
                                 "HOTSLogs:SilencedPlayerIDAndExistingLeaderboardOptOut:" +
                                 players[i].PlayerId))
                    {
                        redisClient.TrySet(
                            "HOTSLogs:SilencedPlayerIDAndExistingLeaderboardOptOut:" + players[i].PlayerId,
                            "1",
                            DateTime.UtcNow.AddDays(60));
                    }
                }
            }
#pragma warning restore 162

            // Add Player Statistics
            if (replayParseData.IsStatisticsParsedSuccessfully == true)
            {
                var scoreResult = replayParseData.Players[i].ScoreResult;

                // Adjust for Blizzard bugs they haven't fixed yet
                if (replayParseData.Players[i].Character == "Thrall")
                {
                    scoreResult.Healing = null;
                }

                var rcsr = new ReplayCharacterScoreResult
                {
                    ReplayId = replay.ReplayId,
                    PlayerId = thePlayer.PlayerId,
                    Level = scoreResult.Level,
                    Takedowns = scoreResult.Takedowns,
                    SoloKills = scoreResult.SoloKills,
                    Assists = scoreResult.Assists,
                    Deaths = scoreResult.Deaths,
                    HighestKillStreak = scoreResult.HighestKillStreak,
                    HeroDamage = scoreResult.HeroDamage,
                    SiegeDamage = scoreResult.SiegeDamage,
                    StructureDamage = scoreResult.StructureDamage,
                    MinionDamage = scoreResult.MinionDamage,
                    CreepDamage = scoreResult.CreepDamage,
                    SummonDamage = scoreResult.SummonDamage,

                    // TODO: REVISIT THIS NOW THAT THERE IS MATCH AWARD FOR CC
                    // (Currently bugged, Stitches can have hours of CC time: https://github.com/Blizzard/heroprotocol/issues/21)
                    // TimeCCdEnemyHeroes = scoreResult.TimeCCdEnemyHeroes,
                    Healing = scoreResult.Healing,
                    SelfHealing = scoreResult.SelfHealing,
                    DamageTaken = scoreResult.DamageTaken,
                    ExperienceContribution = scoreResult.ExperienceContribution,
                    TownKills = scoreResult.TownKills,
                    TimeSpentDead = scoreResult.TimeSpentDead,
                    MercCampCaptures = scoreResult.MercCampCaptures,
                    WatchTowerCaptures = scoreResult.WatchTowerCaptures,
                    MetaExperience = scoreResult.MetaExperience,
                };

                replayCharacters[i].ReplayCharacterScoreResult = rcsr;
                heroesEntity.ReplayCharacterScoreResults.Add(rcsr);

                foreach (var matchAward in scoreResult.MatchAwards)
                {
                    var rcma = new ReplayCharacterMatchAward
                    {
                        ReplayId = replay.ReplayId,
                        Player = thePlayer,
                        MatchAwardType = (int)matchAward,
                    };
                    replayCharacters[i].ReplayCharacterMatchAwards.Add(rcma);
                }
            }

            logFunction?.Invoke("Adding player/score data into EntityFramework...");
            heroesEntity.SaveChanges();

            // Add Player Upgrade Events
            if (replayParseData.Players[i].UpgradeEvents.Count > 0)
            {
                foreach (var upgradeEventGroup in replayParseData.Players[i].UpgradeEvents
                             .GroupBy(j => j.UpgradeEventType)
                             .Select(
                                 j => new
                                 {
                                     UpgradeEventType = j.Key,
                                     UpgradeEvents = j.OrderBy(k => k.TimeSpan).ToArray(),
                                 })
                             .ToArray())
                {
                    /* Nova shows Snipe misses even if she did not take the
                         * Snipe Master talent - let's make sure there is at least
                         * some positive Upgrade Events
                         */
                    if (upgradeEventGroup.UpgradeEvents.Any(j => j.Value > 0))
                    {
                        switch (upgradeEventGroup.UpgradeEventType)
                        {
                            case UpgradeEventType.NovaSnipeMasterDamageUpgrade:
                            case UpgradeEventType.GallTalentDarkDescentUpgrade:

                                var valueToReplayLengthTimeSpanSumDictionary =
                                    new Dictionary<int, TimeSpan> { { 0, TimeSpan.Zero } };
                                var currentValue = 0;
                                var currentTimeSpan = TimeSpan.Zero;
                                var firstUpgradeEventTimeSpan = TimeSpan.Zero;

                                foreach (var upgradeEvent in upgradeEventGroup.UpgradeEvents)
                                {
                                    if (firstUpgradeEventTimeSpan == TimeSpan.Zero)
                                    {
                                        firstUpgradeEventTimeSpan = upgradeEvent.TimeSpan;
                                    }
                                    else
                                    {
                                        valueToReplayLengthTimeSpanSumDictionary[currentValue] +=
                                            upgradeEvent.TimeSpan - currentTimeSpan;
                                    }

                                    currentTimeSpan = upgradeEvent.TimeSpan;
                                    currentValue += upgradeEvent.Value;

                                    if (currentValue < 0)
                                    {
                                        currentValue = 0;
                                    }

                                    if (!valueToReplayLengthTimeSpanSumDictionary.ContainsKey(currentValue))
                                    {
                                        valueToReplayLengthTimeSpanSumDictionary[currentValue] = TimeSpan.Zero;
                                    }
                                }

                                valueToReplayLengthTimeSpanSumDictionary[currentValue] +=
                                    replayParseData.ReplayLength - currentTimeSpan;

                                var upgradeEventReplayLengthTotalSeconds =
                                    (decimal)(replayParseData.ReplayLength - firstUpgradeEventTimeSpan)
                                    .TotalSeconds;

                                foreach (var upgradeEventValueDictionaryEntry in
                                         valueToReplayLengthTimeSpanSumDictionary)
                                {
                                    replayCharacters[i].ReplayCharacterUpgradeEventReplayLengthPercents.Add(
                                        new ReplayCharacterUpgradeEventReplayLengthPercent
                                        {
                                            UpgradeEventType = (int)upgradeEventGroup.UpgradeEventType,
                                            UpgradeEventValue = upgradeEventValueDictionaryEntry.Key,
                                            ReplayLengthPercent =
                                                (decimal)upgradeEventValueDictionaryEntry.Value.TotalSeconds /
                                                upgradeEventReplayLengthTotalSeconds,
                                        });
                                }

                                break;

                            case UpgradeEventType.RegenMasterStacks:
                            case UpgradeEventType.MarksmanStacks:

                                foreach (var upgradeEvent in upgradeEventGroup.UpgradeEvents)
                                {
                                    replayCharacters[i].ReplayCharacterUpgradeEventReplayLengthPercents.Add(
                                        new ReplayCharacterUpgradeEventReplayLengthPercent
                                        {
                                            UpgradeEventType = (int)upgradeEventGroup.UpgradeEventType,
                                            UpgradeEventValue = upgradeEvent.Value,
                                            ReplayLengthPercent = 1m,
                                        });
                                }

                                break;

                            case UpgradeEventType.WitchDoctorPlagueofToadsPandemicTalentCompletion:

                                foreach (var upgradeEvent in upgradeEventGroup.UpgradeEvents)
                                {
                                    replayCharacters[i].ReplayCharacterUpgradeEventReplayLengthPercents.Add(
                                        new ReplayCharacterUpgradeEventReplayLengthPercent
                                        {
                                            UpgradeEventType = (int)upgradeEventGroup.UpgradeEventType,
                                            UpgradeEventValue = upgradeEvent.Value,
                                            ReplayLengthPercent =
                                                (decimal)((replay.ReplayLength - upgradeEvent.TimeSpan)
                                                          .TotalMilliseconds /
                                                          replay.ReplayLength.TotalMilliseconds),
                                        });
                                }

                                break;
                            default:
                                throw new IndexOutOfRangeException();
                        }
                    }
                }
            }
        }

        string MakeTalentSelection(ReplayCharacter rc) =>
            string.Join(",", rc.ReplayCharacterTalents.Select(x => x.TalentId));

        var talentSelections = (from rc in replayCharacters
                                where rc != null
                                let talentSelection = MakeTalentSelection(rc)
                                select (rc, talentSelection)).ToList();

        talentSelections.ForEach(
            x =>
            {
                var talentBuild = new ReplayPlayerTalentBuild
                {
                    Playerid = x.rc.PlayerId,
                    Replayid = replay.ReplayId,
                    Talentselection = x.talentSelection,
                };
                heroesEntity.ReplayPlayerTalentBuilds.Add(talentBuild);
            });

        try
        {
            heroesEntity.SaveChanges();
        }
        catch (Exception)
        {
            logFunction?.Invoke("Error inserting talent builds " + replay.ReplayId);
            return Tuple.Create(DataParser.ReplayParseResult.Exception, (Guid?)null);
        }

        logFunction?.Invoke("Parsing bans into entity...");
        // Add Replay Team Hero Bans
        if (replayParseData.TeamHeroBans.Any(i => i.Any(j => j != null)))
        {
            var attributeNames = localizationAliases.Select(x => x.AttributeName).ToCaseInsensitiveHashSet();

            var locAttributes = localizationAliases
                .Select(x => (Key: x.AttributeName, x.IdentifierId));
            var locPrimary = localizationAliases
                .Where(x => !attributeNames.Contains(x.PrimaryName))
                .Select(x => (Key: x.PrimaryName, x.IdentifierId));

            var locAttrToId =
                locAttributes.Union(locPrimary)
                    .ToCaseInsensitiveDictionary(x => x.Key, x => x.IdentifierId);

            for (var teamId = 0; teamId < replayParseData.TeamHeroBans.Length; teamId++)
            {
                var isWinner = replayParseData.Players.First(j => j.Team == teamId).IsWinner;

                for (var i = 0; i < replayParseData.TeamHeroBans[teamId].Length; i++)
                {
                    if (replayParseData.TeamHeroBans[teamId][i] != null)
                    {
                        var rthb = new ReplayTeamHeroBan
                        {
                            ReplayId = replay.ReplayId,
                            CharacterId =
                                locAttrToId.GetValueOrDefault(replayParseData.TeamHeroBans[teamId][i]),
                            IsWinner = isWinner ? 1ul : 0,
                            BanPhase = i,
                        };
                        replay.ReplayTeamHeroBans.Add(rthb);
                        heroesEntity.ReplayTeamHeroBans.Add(rthb);

                        var sBanned = replayParseData.TeamHeroBans[teamId][i];
                        if (!locAttrToId.ContainsKey(sBanned) &&
                            !heroesEntity.UnknownData.Any(j => j.UnknownData == sBanned))
                        {
                            heroesEntity.UnknownData.Add(new UnknownDatum { UnknownData = sBanned });
                        }
                    }
                }
            }
        }

        logFunction?.Invoke("Adding bans to EntityFramework...");
        try
        {
            heroesEntity.SaveChanges();
        }
        catch (Exception)
        {
            return Tuple.Create(DataParser.ReplayParseResult.RetryForBanError, (Guid?)null);
        }

        logFunction?.Invoke("Checking for unknown events...");
        // Check for Unidentified Events
        if (replayFileBytes != null)
        {
            var unidentifiedEventDictionary = new Dictionary<string, bool>();

            foreach (var player in replayParseData.Players.Where(
                         i =>
                             i.MiscellaneousUpgradeEventDictionary.Count > 0))
            {
                foreach (var unidentifiedEvent in player.MiscellaneousUpgradeEventDictionary.Keys)
                {
                    unidentifiedEventDictionary[unidentifiedEvent] = true;
                }
            }

            foreach (var unidentifiedEvent in unidentifiedEventDictionary.Keys)
            {
                if (!heroesEntity.UnknownData.Any(i => i.UnknownData == unidentifiedEvent))
                {
                    heroesEntity.UnknownData.Add(new UnknownDatum { UnknownData = unidentifiedEvent });

                    using var amazonS3Client = new AmazonS3Client(
                        AWSAccessKeyID,
                        AWSSecretAccessKey,
                        RegionEndpoint.USWest2);
                    using var memoryStream = new MemoryStream(replayFileBytes);
                    var putObjectRequest = new PutObjectRequest
                    {
                        BucketName = "heroesreplays-admin",
                        Key = $"{unidentifiedEvent} {nowString} {replayHash}",
                        InputStream = memoryStream,
                        StorageClass = S3StorageClass.StandardInfrequentAccess,
                    };
                    amazonS3Client.PutObject(putObjectRequest);
                }
            }
        }

        logFunction?.Invoke("Adding periodic xp...");
        // Add Team Periodic XP Breakdown
        if (replayParseData.IsStatisticsParsedSuccessfully == true)
        {
            for (var i = 0; i < replayParseData.TeamPeriodicXPBreakdown.Length; i++)
            {
                if (replayParseData.TeamPeriodicXPBreakdown[i].Count == 0)
                {
                    continue;
                }

                var isWinner = replayParseData.Players.First(j => j.Team == i).IsWinner;

                var teamPeriodicXpBreakdowns =
                    replayParseData.TeamPeriodicXPBreakdown[i]
                        .GroupBy(j => (int)j.TimeSpan.TotalMinutes)
                        .Select(j => j.OrderByDescending(k => k.TimeSpan).First())
                        .OrderBy(j => j.TimeSpan)
                        .ToArray();
                foreach (var teamPeriodicXpBreakdown in teamPeriodicXpBreakdowns)
                {
                    var rpxb = new ReplayPeriodicXpBreakdown
                    {
                        ReplayId = replay.ReplayId,
                        IsWinner = isWinner ? 1ul : 0,
                        GameTimeMinute = (int)teamPeriodicXpBreakdown.TimeSpan.TotalMinutes,
                        MinionXp = teamPeriodicXpBreakdown.MinionXP,
                        CreepXp = teamPeriodicXpBreakdown.CreepXP,
                        StructureXp = teamPeriodicXpBreakdown.StructureXP,
                        HeroXp = teamPeriodicXpBreakdown.HeroXP,
                        TrickleXp = teamPeriodicXpBreakdown.TrickleXP,
                    };
                    replay.ReplayPeriodicXpBreakdowns.Add(rpxb);
                    heroesEntity.ReplayPeriodicXpBreakdowns.Add(rpxb);
                }
            }
        }

        // BOOKMARK:possible breakdown in stats
        // added a bunch of heroesEntity adds above to hopefully fix stats upload
        // added to: teamobjectives, charactertalents, characterscoreresults, teamherobans, periodicxp

        // this call to SaveChanges seems unncecessary, since they haven't made any changes...
        // is this where we're losing data - not being uploaded, but saved anyway?

        heroesEntity.SaveChanges();

        // If this is an Event, recalculate the total games played for this Event
        if (eventId.HasValue)
        {
            var eventEntity = heroesEntity.Events.SingleOrDefault(i => i.EventId == eventId.Value);

            if (eventEntity != null)
            {
                eventEntity.EventGamesPlayed = heroesEntity.Replays.Count(i => i.GameMode == eventId.Value);
                heroesEntity.SaveChanges();
            }
        }

        // Cache Interesting Replay Variables that don't have a Database Column
        /*
            using (var redisClient = RedisManagerPool.GetClient())
            {
                var currentWeekDateTimeString = DateTime.UtcNow.StartOfWeek(DayOfWeek.Sunday)
                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                for (var i = 0; i < players.Length; i++)
                {
                    if (replayParseData.Players[i].SkinAndSkinTint != null)
                    {
                        redisClient.IncrementValue(
                            "HOTSLogs:SitewideCharacterSkinAndSkinTintWinRate:" +
                            $"{currentWeekDateTimeString}:" +
                            $"{(int)replayParseData.GameMode}:" +
                            $"{replayParseData.Players[i].SkinAndSkinTint}:" +
                            $"{(replayParseData.Players[i].IsWinner ? "1" : "0")}");
                    }

                    if (replayParseData.Players[i].MountAndMountTint != null)
                    {
                        redisClient.IncrementValue(
                            "HOTSLogs:SitewideCharacterMountAndMountTintWinRate:" +
                            $"{currentWeekDateTimeString}:" +
                            $"{(int)replayParseData.GameMode}:" +
                            $"{replayParseData.Players[i].MountAndMountTint}:" +
                            $"{(replayParseData.Players[i].IsWinner ? "1" : "0")}");
                    }
                }
            }
            */

        return Tuple.Create(replayParseResult, (Guid?)replayHash);
    }
}
