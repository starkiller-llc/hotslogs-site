using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using HelperCore;
using Heroes.DataAccessLayer.Data;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static Heroes.ReplayParser.DataParser;

namespace HotsLogsApi.BL.Migration.Helpers;

public class UploadHelper
{
    private readonly IServiceProvider _svcp;
    private int _counter;
    private readonly Guid _unique = Guid.NewGuid();

    public UploadHelper(IServiceProvider svcp)
    {
        _svcp = svcp;
    }

    public async Task<(ReplayParseResult Exception, Exception e, Guid? replayGuid)> AddReplay(
        Stream stream,
        string ipAddress,
        string uploaderVersion,
        int? eventId = null)
    {
        var counterSnapshot = Interlocked.Increment(ref _counter);

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var logMsg = $"Uploader {counterSnapshot,6} {ipAddress?.Trim()} (ver {uploaderVersion})";
        DataHelper.LogApplicationEvents(logMsg, "UploadLogs");

        await using var sw = new StringWriter();
        try
        {
            var (parseResult, replayGuid) = DataHelper.AddReplay(
                localizationAliases: Global.GetLocalizationAlias(),
                bytes: bytes,
                ipAddress: ipAddress,
                eventId: eventId,
                // ReSharper disable once AccessToDisposedClosure
                logFunction: x => sw.WriteLine($"{x}"));

            if (parseResult == ReplayParseResult.Success && replayGuid.HasValue)
            {
                var logMsgSuccess = $"Uploader {counterSnapshot,6} success {replayGuid.Value}";
                DataHelper.LogApplicationEvents(logMsgSuccess, "UploadLogs");
                try
                {
                    SaveReplay(replayGuid.Value, bytes);
                }
                catch (Exception e)
                {
                    var msg =
                        $"Couldn't save to bucket {replayGuid.Value} uploader - counter: {counterSnapshot}: {e}";
                    DataHelper.LogApplicationEvents(msg, "UploadLogs");
                }
            }
            else
            {
                var logMsgFail = $"Uploader {counterSnapshot,6} fail    {parseResult}";
                DataHelper.LogApplicationEvents(logMsgFail, "UploadLogs");
                if (parseResult == ReplayParseResult.Exception)
                {
                    var msg = $"Uploader {counterSnapshot,6} {_unique}:\n{sw}";
                    DataHelper.LogApplicationEvents(msg, "UploadLogs");
                }
            }

            return (parseResult, null, replayGuid);
        }
        catch (Exception e)
        {
            var logMsgFail = $"Uploader {counterSnapshot,6} fail    Outer Exception {_unique}\n{e}\n{sw}";
            DataHelper.LogApplicationEvents(logMsgFail, "UploadLogs");

            SaveFailure(bytes, $"uploader_{_unique}_{counterSnapshot}.StormReplay");

            return (ReplayParseResult.Exception, e, null);
        }
    }

    private void SaveFailure(byte[] bytes, string saveName)
    {
        using var ms = new MemoryStream(bytes);
        using var amazonS3Client = new AmazonS3Client(
            DataHelper.AWSAccessKeyID,
            DataHelper.AWSSecretAccessKey,
            RegionEndpoint.USWest2);
        amazonS3Client.PutObject(
            new PutObjectRequest
            {
                BucketName = "uploadfailures",
                Key = saveName,
                InputStream = ms,
                StorageClass = S3StorageClass.StandardInfrequentAccess,
            });
    }

    private void SaveReplay(Guid replayGuid, byte[] bytes)
    {
        var replayId = DataHelper.GetReplayID(replayGuid);
        using var scope = _svcp.CreateScope();
        var heroesEntities = HeroesdataContext.Create(scope);
        var replay = heroesEntities.Replays.Single(x => x.ReplayId == replayId);
        var replayTime = replay.TimestampReplay;

        using var ms = new MemoryStream(bytes);
        using var amazonS3Client = new AmazonS3Client(
            DataHelper.AWSAccessKeyID,
            DataHelper.AWSSecretAccessKey,
            RegionEndpoint.USWest2);
        var putObjectResponse = amazonS3Client.PutObject(
            new PutObjectRequest
            {
                BucketName = "heroesreplays",
                Key = replayGuid.ToString(),
                InputStream = ms,
                StorageClass = S3StorageClass.StandardInfrequentAccess,
                Metadata = new MetadataCollection
                {
                    { AmazonReplayMetadata.ReplayTime, replayTime.ToJson() },
                },
            });
        if (putObjectResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            DataHelper.SendServerErrorEmail(
                "Couldn't put successfully parsed, uploaded replay into S3 bucket<br><br>Http Status Code: " +
                putObjectResponse.HttpStatusCode);
        }
    }
}
