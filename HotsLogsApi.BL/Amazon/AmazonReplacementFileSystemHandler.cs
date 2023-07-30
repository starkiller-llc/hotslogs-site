using Amazon.SimpleEmail.Model;
using HelperCore;
using HotsLogsApi.BL.Migration.Helpers;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace HotsLogsApi.BL.Amazon;

public class AmazonReplacementFileSystemHandler : IAmazonReplacementHandler
{
    private const string BasePath = @"d:\buckets";

    public byte[] GetObject(string bucketName, string key)
    {
        var filePath = GetFilePath(bucketName, key);
        return File.Exists(filePath)
            ? File.ReadAllBytes(filePath)
            : null;
    }

    public void PutObject(
        string bucketName,
        string key,
        byte[] bytes,
        ICollection<KeyValuePair<string, string>> metadata)
    {
        var filePath = GetFilePath(bucketName, key);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        File.WriteAllBytes(filePath, bytes);

        if (metadata == null)
        {
            return;
        }

        var replayTime = metadata.SingleOrDefault(x => x.Key == AmazonReplayMetadata.ReplayTime);
        if (!replayTime.Equals(default))
        {
            try
            {
                var time = replayTime.Value.FromJson<DateTime>();
                File.SetCreationTime(filePath, time);
                File.SetLastWriteTime(filePath, time);
            }
            catch
            {
                // ignored
            }
        }
    }

    public void DeleteObject(string bucketName, string key)
    {
        var filePath = GetFilePath(bucketName, key);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public void SendEmail(SendEmailRequest sendEmailRequest)
    {
        var logMsg =
            $"From: {sendEmailRequest.Source}, " +
            $"To: {string.Join(", ", sendEmailRequest.Destination.ToAddresses)}, " +
            $"Subject: {sendEmailRequest.Message.Content.Subject}, " +
            $"Body: {sendEmailRequest.Message.Body.Html.Subject}";
        DataHelper.LogApplicationEvents(logMsg, "EmailLog.txt");
    }

    private static string GetFilePath(string bucketName, string key)
    {
        var bucketDir = MakeValidFileName(bucketName);
        var bucketPath = Path.Combine(BasePath, bucketDir);
        if (!Directory.Exists(bucketPath))
        {
            Directory.CreateDirectory(bucketPath);
        }

        var fileName = MakeValidFileName(key);
        var filePath = Path.Combine(bucketPath, fileName);
        return filePath;
    }

    private static string MakeValidFileName(string name)
    {
        var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return Regex.Replace(name, invalidRegStr, "_");
    }
}
