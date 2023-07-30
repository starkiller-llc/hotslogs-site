using Heroes.ReplayParser;
using HOTSLogsUploader.Common;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using static Heroes.ReplayParser.DataParser;

namespace HOTSLogsUploader.Utilities
{
    public static class HotsLogsHttpClient
    {
        public static HttpResponseMessage Get(string endpoint)
        {
            HttpResponseMessage response;

            using (var client = new HttpClient())
            {
                response = client.GetAsync($"{Constants.hotslogsUrl}{endpoint}").Result;
            }

            return response;
        }


        public static HttpResponseMessage Post(string endpoint, HttpContent content, bool disposeOfContent = true)
        {
            return Post(endpoint, null, content, disposeOfContent).Result;
        }

        public static async Task<HttpResponseMessage> Post(string endpoint, NameValueCollection uri, HttpContent content, bool disposeOfContent = false)
        {
            HttpResponseMessage response;

            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                var requestUri = $"{Constants.hotslogsUrl}{endpoint}";
                if (uri != null)
                {
                    requestUri += $"?{uri}";
                }

                response = await client.PostAsync(requestUri, content);

                if (disposeOfContent)
                {
                    content.Dispose();
                }
            }

            return response;
        }
    }

    public static class ReplayUploader
    {
        /// <summary>
        /// Upload a .StormReplay file to HOTSLogs API
        /// </summary>
        /// <param name="filePath">Fully qualified path to the replay file.</param>
        /// <param name="launchMatchSummary">Should a match summary be launched once successfully uploaded.</param>
        public static async Task<object> DoUpload(string filePath, ProcessingStage onProcessStageChange = null, bool launchMatchSummary = false)
        {
            var date = DateTime.MinValue;

            onProcessStageChange?.Invoke(null, new ProcessingStageEventArgs(filePath, ProgressStage.Uploading));

            try
            {
                ReplayParseResult parseResult = ReplayParseResult.Exception;
                Guid? fingerprint = null;

                using (var replayContent = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    // Upload to hotslogs
                    var uri = HttpUtility.ParseQueryString(string.Empty);
                    uri.Add("fileName", Path.GetFileName(filePath));
                    uri.Add("uploaderVersion", Application.ProductVersion);

                    var response = await HotsLogsHttpClient.Post("/api/uploader/addreplay", uri, new StreamContent(replayContent));
                    var contents = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        UploadResponse uploadResponse = JsonConvert.DeserializeObject<UploadResponse>(contents);

                        parseResult = (ReplayParseResult)uploadResponse.Result;
                        fingerprint = uploadResponse.Fingerprint;

                        // Show match summary
                        LaunchMatchSummary(launchMatchSummary, uploadResponse.ReplayId.GetValueOrDefault(), parseResult);                        
                    }
                    else
                    {
                        // Exception uploading, not parsing
                        parseResult = ReplayParseResult.Exception;
                        Log.Error($"Error uploading {filePath}, response code: {response.StatusCode}, message: {response.ReasonPhrase}");
                    }
                }

                // Date should be set when the result is known
                date = DateTime.UtcNow;

                // notify main thread
                onProcessStageChange?.Invoke(null, new ProcessingStageEventArgs(filePath, ProgressStage.Completed, date, parseResult, fingerprint));
            }
            catch (Exception e)
            {
                // Log an exception
                Log.Error(e, "Uploading to HOTSLogs");

                // catch all exceptions
                onProcessStageChange?.Invoke(null, new ProcessingStageEventArgs(filePath, ProgressStage.Completed, DateTime.UtcNow, ReplayParseResult.Exception));
            }

            return null;
        }


        public static async Task SendToHeroesProfiles(string fileName)
        {
            try
            {
                using (var client = new HttpClient())
                using (var replayContent = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (var postContent = new MultipartFormDataContent())
                {
                    // add file content to post request
                    postContent.Add(new StreamContent(replayContent), "file", fileName);

                    // Upload to heroesprofile
                    await client.PostAsync(Constants.heroesProfilesUploadEndpoint, postContent);
                }
            }
            catch (Exception e)
            {
                // Log an exception
                Log.Error(e, "Uploading to HeroesProfile");
            }
        }


        public static (ReplayParseResult ReplayParseResult, Guid? Fingerprint) GetReplayFingerprint(string filename)
        {
            ReplayParseResult result = ReplayParseResult.Exception;
            Guid? fingerprint = null;

            try
            {
                Replay replay;
                (result, replay) = ParseReplay(filename, false, ParseOptions.MinimalParsing);

                if (result == ReplayParseResult.Success && replay != null)
                {
                    fingerprint = GetReplayFingerprint(replay);
                }
            }
            catch
            {
            }

            return (result, fingerprint);
        }


        public static Guid? GetReplayFingerprint(Replay replay)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    StringBuilder builder = new StringBuilder();

                    replay.Players
                        .Select(i => i.BattleNetId)
                        .OrderBy(i => i)
                        .ToList()
                        .ForEach(i => builder.Append(i.ToString()));

                    builder.Append(replay.RandomValue);

                    return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString())));
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Calculating fingerprint");
                return null;
            }
        }


        public static void LaunchMatchSummary(bool showSummaryInBrowser, int? replayId, ReplayParseResult parseResult)
        {
            if (showSummaryInBrowser && (parseResult == ReplayParseResult.Success || parseResult == ReplayParseResult.Duplicate) && replayId.HasValue)
            {
                try
                {
                    Process.Start($@"{Constants.hotslogsUrl}/Player/MatchSummaryContainer?ReplayID={replayId}");
                }
                catch (Exception e)
                {
                    Log.Error(e, "Displaying match summary");
                }
            }
        }


        public static List<(Guid Fingerprint, bool Exists)> GetFingerprintsToUpload(IEnumerable<Guid> fingerprints)
        {
            var responseMessage = HotsLogsHttpClient.Post("/api/uploader/checkfingerprints", new FingerprintQuery(fingerprints).ToStringContent());
            var result = new List<(Guid Fingerprint, bool Exists)>();

            if (responseMessage.IsSuccessStatusCode)
            {
                var response = FingerprintResponse.Deserialize(responseMessage.Content.ReadAsStringAsync().Result);

                foreach (var item in fingerprints)
                {
                    result.Add((item, !response.Fingerprints.Where(i => i.IsDuplicate).Any(i => i.Hash == item)));
                }
            }            

            return result;
        }


        public static bool ReplayNeedsUploading(Replay replay)
        {
            if (replay != null)
            {
                var fingerprint = GetReplayFingerprint(replay);

                if (fingerprint != null)
                {
                    var result = GetFingerprintsToUpload(new[] { fingerprint.Value });

                    if (!result.IsEmpty())
                    {
                        return result.First().Exists;
                    }
                }
            }

            return false;
        }
    }
}
