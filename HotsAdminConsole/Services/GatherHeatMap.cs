using JetBrains.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Local
#pragma warning disable IDE0051 // Remove unused private members

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Gather Heat Map", Sort = 9, Port = 17004)]
public class GatherHeatMapService : ServiceBase
{
    private const int ReplayParsingMaxDegreeOfParallelism = 4;

    private const string MySqlCommandTextSpecificHeroes =
        @"select
            r.ReplayHash
            from Replay r use index (IX_TimestampReplay)
            join ReplayCharacter rc on rc.ReplayID = r.ReplayID and rc.CharacterID = {0}
            where r.TimestampReplay > date_add(now(), interval -30 day) and r.MapID = {1}
            order by r.TimestampReplay desc
            limit {2}";

    private const string MySqlCommandTextLtMoralesMedivac =
        @"select
            r.ReplayHash
            from Replay r use index (IX_TimestampReplay)
            join ReplayCharacter rc on rc.ReplayID = r.ReplayID
            join ReplayCharacterTalent rct on rct.ReplayID = rc.ReplayID and rct.PlayerID = rc.PlayerID
            join LocalizationAlias la on la.IdentifierID = rc.CharacterID
            join HeroTalentInformation h on h.`Character` = la.PrimaryName and r.ReplayBuild >= h.ReplayBuildFirst and r.ReplayBuild <= h.ReplayBuildLast and h.TalentID = rct.TalentID
            where r.TimestampReplay > date_add(now(), interval -30 day) and r.MapID = {0} and la.PrimaryName = 'Lt. Morales' and h.TalentName = 'Medivac Dropship'
            order by r.TimestampReplay desc
            limit {1}";

    private const string MySqlCommandTextGuldanDemonicCircle =
        @"select
            r.ReplayHash
            from Replay r use index (IX_TimestampReplay)
            join ReplayCharacter rc on rc.ReplayID = r.ReplayID
            join ReplayCharacterTalent rct on rct.ReplayID = rc.ReplayID and rct.PlayerID = rc.PlayerID
            join LocalizationAlias la on la.IdentifierID = rc.CharacterID
            join HeroTalentInformation h on h.`Character` = la.PrimaryName and r.ReplayBuild >= h.ReplayBuildFirst and r.ReplayBuild <= h.ReplayBuildLast and h.TalentID = rct.TalentID
            where r.TimestampReplay > date_add(now(), interval -30 day) and r.MapID = {0} and la.PrimaryName = 'Gul''dan' and h.TalentName = 'Demonic Circle'
            order by r.TimestampReplay desc
            limit {1}";

    public GatherHeatMapService(IServiceProvider svcp) : base(svcp) { }

    protected override Task RunOnce(CancellationToken token = default)
    {
        //NOT WORRIED ABOUT THIS AT THE MOMENT

        //txtOutput.Text += "\n" + DateTime.Now.ToShortTimeString() + "::GatherHeatMap start...";
        //try
        //{
        //    Localizationalias[] localizationAlia = null;

        //    // Player Death Heatmap
        //    using (var heroesEntity = new HeroesdataContext())
        //    {
        //        heroesEntity.Database.CommandTimeout = MMR.LongCommandTimeout;

        //        localizationAlia = heroesEntity.Localizationaliases.ToArray();

        //        foreach (var map in localizationAlia.Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map))
        //        {
        //            // Grab recent replays for this map
        //            // Currently we limit this to 50 replays - higher than this and the map gets too cluttered
        //            var replayHashes = heroesEntity.Replays.Where(i => i.MapID == map.IdentifierID).OrderByDescending(i => i.TimestampReplay).Take(50).Select(i => i.ReplayHash).ToArray();

        //            var pointStringBag = new ConcurrentBag<string>();

        //            // Get Replay File(s) from Amazon S3
        //            Parallel.ForEach(replayHashes, new ParallelOptions { MaxDegreeOfParallelism = ReplayParsingMaxDegreeOfParallelism }, replayHash =>
        //            {
        //                using (var amazonS3Client = new AmazonS3Client(DataHelper.AWSAccessKeyID, DataHelper.AWSSecretAccessKey, Amazon.RegionEndpoint.USWest2))
        //                    try
        //                    {
        //                        using (var getObjectResponse = amazonS3Client.GetObject(new GetObjectRequest { BucketName = "heroesreplays", Key = replayHash.ToGuid().ToString() }))
        //                        {
        //                            var replayParseResult = DataParser.ParseReplay(getObjectResponse.ResponseStream.ReadFully(), ignoreErrors: false).Item2;

        //                            if (replayParseResult != null)
        //                            {
        //                                var mapOffsets = DataParser.MapOffsets.ContainsKey(map.PrimaryName) ? DataParser.MapOffsets[map.PrimaryName] : new Tuple<double, double, double, double>(0.0, 0.0, 1.0, 1.0);

        //                                foreach (var player in replayParseResult.Players)
        //                                    foreach (var playerHeroUnit in player.HeroUnits.Where(i => i.PointDied != null))
        //                                    {
        //                                        var offsetPointDied = OffsetPoint(playerHeroUnit.PointDied, mapOffsets);

        //                                        pointStringBag.Add("{" + string.Format("lat:{1},lng:{0},value:1", offsetPointDied.X, offsetPointDied.Y) + "}");
        //                                    }
        //                            }
        //                        }
        //                    }
        //                    catch (AmazonS3Exception)
        //                    {

        //                    }
        //            });

        //            // Combine the bag into the end result
        //            using (var redisClient = DataHelper.RedisManagerPool.GetClient())
        //                redisClient.TrySet("HOTSLogs:HeatMapData:PlayerDeaths:" + map.IdentifierID, string.Join(",", pointStringBag), TimeSpan.FromDays(30));
        //        }
        //    }

        //    // Lt. Morales Medivac Heatmap
        //    foreach (var map in localizationAlia.Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map))
        //    {
        //        var replayHashes = new List<string>();

        //        using (var mySqlConnection = new MySqlConnection(ConnectionString(ConnectionStringKey)))
        //        {
        //            mySqlConnection.Open();

        //            using (var mySqlCommand = new MySqlCommand(string.Format(MySqlCommandTextLtMoralesMedivac, map.IdentifierID, 50), mySqlConnection) { CommandTimeout = MMR.LongCommandTimeout })
        //            using (var mySqlDataReader = mySqlCommand.ExecuteReader())
        //                while (mySqlDataReader.Read())
        //                    replayHashes.Add(new Guid((byte[])mySqlDataReader["ReplayHash"]).ToString());
        //        }

        //        var pointStringBag = new ConcurrentBag<string>();

        //        // Get Replay File(s) from Amazon S3
        //        Parallel.ForEach(replayHashes, new ParallelOptions { MaxDegreeOfParallelism = ReplayParsingMaxDegreeOfParallelism }, replayHash =>
        //        {
        //            using (var amazonS3Client = new AmazonS3Client(DataHelper.AWSAccessKeyID, DataHelper.AWSSecretAccessKey, Amazon.RegionEndpoint.USWest2))
        //                try
        //                {
        //                    using (var getObjectResponse = amazonS3Client.GetObject(new GetObjectRequest { BucketName = "heroesreplays", Key = replayHash }))
        //                    {
        //                        var replayParseResult = DataParser.ParseReplay(getObjectResponse.ResponseStream.ReadFully(), ignoreErrors: false).Item2;

        //                        if (replayParseResult != null)
        //                        {
        //                            var mapOffsets = DataParser.MapOffsets.ContainsKey(map.PrimaryName) ? DataParser.MapOffsets[map.PrimaryName] : new Tuple<double, double, double, double>(0.0, 0.0, 1.0, 1.0);

        //                            foreach (var medivacDropoffLocationUnit in replayParseResult.Units.Where(i => i.Name == "MedicMedivacDropshipDropoffLocationUnit"))
        //                            {
        //                                var offsetPointBorn = OffsetPoint(medivacDropoffLocationUnit.PointBorn, mapOffsets);

        //                                pointStringBag.Add("{" + string.Format("lat:{1},lng:{0},value:1", offsetPointBorn.X, offsetPointBorn.Y) + "}");
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (AmazonS3Exception)
        //                {

        //                }
        //        });

        //        // Combine the bag into the end result
        //        using (var redisClient = DataHelper.RedisManagerPool.GetClient())
        //            redisClient.TrySet("HOTSLogs:HeatMapData:LtMoralesMedivac:" + map.IdentifierID, string.Join(",", pointStringBag), TimeSpan.FromDays(30));
        //    }

        //    // Gul'dan Demonic Circle Location Heatmap
        //    foreach (var map in localizationAlia.Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map))
        //    {
        //        var replayHashes = new List<string>();

        //        using (var mySqlConnection = new MySqlConnection(ConnectionString(ConnectionStringKey)))
        //        {
        //            mySqlConnection.Open();

        //            using (var mySqlCommand = new MySqlCommand(string.Format(MySqlCommandTextGuldanDemonicCircle, map.IdentifierID, 50), mySqlConnection) { CommandTimeout = MMR.LongCommandTimeout })
        //            using (var mySqlDataReader = mySqlCommand.ExecuteReader())
        //                while (mySqlDataReader.Read())
        //                    replayHashes.Add(new Guid((byte[])mySqlDataReader["ReplayHash"]).ToString());
        //        }

        //        var pointStringBag = new ConcurrentBag<string>();

        //        // Get Replay File(s) from Amazon S3
        //        Parallel.ForEach(replayHashes, new ParallelOptions { MaxDegreeOfParallelism = ReplayParsingMaxDegreeOfParallelism }, replayHash =>
        //        {
        //            using (var amazonS3Client = new AmazonS3Client(DataHelper.AWSAccessKeyID, DataHelper.AWSSecretAccessKey, Amazon.RegionEndpoint.USWest2))
        //                try
        //                {
        //                    using (var getObjectResponse = amazonS3Client.GetObject(new GetObjectRequest { BucketName = "heroesreplays", Key = replayHash }))
        //                    {
        //                        var replayParseResult = DataParser.ParseReplay(getObjectResponse.ResponseStream.ReadFully(), ignoreErrors: false).Item2;

        //                        if (replayParseResult != null)
        //                        {
        //                            var mapOffsets = DataParser.MapOffsets.ContainsKey(map.PrimaryName) ? DataParser.MapOffsets[map.PrimaryName] : new Tuple<double, double, double, double>(0.0, 0.0, 1.0, 1.0);

        //                            foreach (var unit in replayParseResult.Units.Where(i => i.Name == "GuldanDemonicCircleUnit"))
        //                            {
        //                                var offsetPointBorn = OffsetPoint(unit.PointBorn, mapOffsets);

        //                                pointStringBag.Add("{" + string.Format("lat:{1},lng:{0},value:1", offsetPointBorn.X, offsetPointBorn.Y) + "}");
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (AmazonS3Exception)
        //                {

        //                }
        //        });

        //        // Combine the bag into the end result
        //        using (var redisClient = DataHelper.RedisManagerPool.GetClient())
        //            redisClient.TrySet("HOTSLogs:HeatMapData:GuldanDemonicCircle:" + map.IdentifierID, string.Join(",", pointStringBag), TimeSpan.FromDays(30));
        //    }

        //    // Murky Egg Heatmap
        //    foreach (var map in localizationAlia.Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map))
        //    {
        //        var replayHashes = new List<string>();

        //        using (var mySqlConnection = new MySqlConnection(ConnectionString(ConnectionStringKey)))
        //        {
        //            mySqlConnection.Open();

        //            using (var mySqlCommand = new MySqlCommand(string.Format(MySqlCommandTextSpecificHeroes, localizationAlia.Single(i => i.PrimaryName == "Murky").IdentifierID, map.IdentifierID, 50), mySqlConnection) { CommandTimeout = MMR.LongCommandTimeout })
        //            using (var mySqlDataReader = mySqlCommand.ExecuteReader())
        //                while (mySqlDataReader.Read())
        //                    replayHashes.Add(new Guid((byte[])mySqlDataReader["ReplayHash"]).ToString());
        //        }

        //        var pointStringBag = new ConcurrentBag<string>();

        //        // Get Replay File(s) from Amazon S3
        //        Parallel.ForEach(replayHashes, new ParallelOptions { MaxDegreeOfParallelism = ReplayParsingMaxDegreeOfParallelism }, replayHash =>
        //        {
        //            using (var amazonS3Client = new AmazonS3Client(DataHelper.AWSAccessKeyID, DataHelper.AWSSecretAccessKey, Amazon.RegionEndpoint.USWest2))
        //                try
        //                {
        //                    using (var getObjectResponse = amazonS3Client.GetObject(new GetObjectRequest { BucketName = "heroesreplays", Key = replayHash }))
        //                    {
        //                        var replayParseResult = DataParser.ParseReplay(getObjectResponse.ResponseStream.ReadFully(), ignoreErrors: false).Item2;

        //                        if (replayParseResult != null)
        //                        {
        //                            var mapOffsets = DataParser.MapOffsets.ContainsKey(map.PrimaryName) ? DataParser.MapOffsets[map.PrimaryName] : new Tuple<double, double, double, double>(0.0, 0.0, 1.0, 1.0);

        //                            foreach (var murkyEggUnit in replayParseResult.Units.Where(i => i.Name == "MurkyRespawnEgg"))
        //                            {
        //                                var offsetPointBorn = OffsetPoint(murkyEggUnit.PointBorn, mapOffsets);

        //                                pointStringBag.Add("{" + string.Format("lat:{1},lng:{0},value:1", offsetPointBorn.X, offsetPointBorn.Y) + "}");
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (AmazonS3Exception)
        //                {

        //                }
        //        });

        //        // Combine the bag into the end result
        //        using (var redisClient = DataHelper.RedisManagerPool.GetClient())
        //            redisClient.TrySet("HOTSLogs:HeatMapData:MurkyEggs:" + map.IdentifierID, string.Join(",", pointStringBag), TimeSpan.FromDays(30));
        //    }

        //    // Abathur Location Heatmap
        //    foreach (var map in localizationAlia.Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map))
        //    {
        //        var replayHashes = new List<string>();

        //        using (var mySqlConnection = new MySqlConnection(ConnectionString(ConnectionStringKey)))
        //        {
        //            mySqlConnection.Open();

        //            using (var mySqlCommand = new MySqlCommand(string.Format(MySqlCommandTextSpecificHeroes, localizationAlia.Single(i => i.PrimaryName == "Abathur").IdentifierID, map.IdentifierID, 50), mySqlConnection) { CommandTimeout = MMR.LongCommandTimeout })
        //            using (var mySqlDataReader = mySqlCommand.ExecuteReader())
        //                while (mySqlDataReader.Read())
        //                    replayHashes.Add(new Guid((byte[])mySqlDataReader["ReplayHash"]).ToString());
        //        }

        //        var pointStringBag = new ConcurrentBag<string>();

        //        // Get Replay File(s) from Amazon S3
        //        Parallel.ForEach(replayHashes, new ParallelOptions { MaxDegreeOfParallelism = ReplayParsingMaxDegreeOfParallelism }, replayHash =>
        //        {
        //            using (var amazonS3Client = new AmazonS3Client(DataHelper.AWSAccessKeyID, DataHelper.AWSSecretAccessKey, Amazon.RegionEndpoint.USWest2))
        //                try
        //                {
        //                    using (var getObjectResponse = amazonS3Client.GetObject(new GetObjectRequest { BucketName = "heroesreplays", Key = replayHash }))
        //                    {
        //                        var replayParseResult = DataParser.ParseReplay(getObjectResponse.ResponseStream.ReadFully(), ignoreErrors: false).Item2;

        //                        if (replayParseResult != null)
        //                        {
        //                            var mapOffsets = DataParser.MapOffsets.ContainsKey(map.PrimaryName) ? DataParser.MapOffsets[map.PrimaryName] : new Tuple<double, double, double, double>(0.0, 0.0, 1.0, 1.0);

        //                            foreach (var abathurUnit in replayParseResult.Units.Where(i => i.Name == "HeroAbathur"))
        //                                foreach (var abathurPosition in abathurUnit.Positions.Where(i => !i.IsEstimated))
        //                                {
        //                                    var offsetPosition = OffsetPoint(abathurPosition.Point, mapOffsets);

        //                                    pointStringBag.Add("{" + string.Format("lat:{1},lng:{0},value:1", offsetPosition.X, offsetPosition.Y) + "}");
        //                                }
        //                        }
        //                    }
        //                }
        //                catch (AmazonS3Exception)
        //                {

        //                }
        //        });

        //        // Combine the bag into the end result
        //        using (var redisClient = DataHelper.RedisManagerPool.GetClient())
        //            redisClient.TrySet("HOTSLogs:HeatMapData:AbathurPositions:" + map.IdentifierID, string.Join(",", pointStringBag), TimeSpan.FromDays(30));
        //    }
        //}
        //catch (Exception ex)
        //{
        //    throw ex;
        //    //DataHelper.SendServerErrorEmail(@"Error gathering Heat Map data<br><br>Exception: " + ex);
        //}
        //txtOutput.Text += "\n" + DateTime.Now.ToShortTimeString() + "::GatherHeatMap stop.";

        return Task.CompletedTask;
    }
}
