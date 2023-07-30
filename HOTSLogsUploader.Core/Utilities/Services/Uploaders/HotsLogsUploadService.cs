using HOTSLogsUploader.Core.Models.Db.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ReplayEntity = HOTSLogsUploader.Core.Models.Db.Entity.Replay;
using System.Text.Json;
using HOTSLogsUploader.Core.Models.Api;
using System.IO;
using HOTSLogsUploader.Core.Utilities.Extensions;
using System.Threading;
using static Heroes.ReplayParser.DataParser;
using Microsoft.Extensions.DependencyInjection;
using HOTSLogsUploader.Core.Extensions;
using System.Threading.Tasks.Dataflow;
using HOTSLogsUploader.Core.Models;

namespace HOTSLogsUploader.Core.Utilities.Services.Uploaders
{
    public class HotsLogsUploadService : IUploadService
    {
        private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri(Constants.hotslogsApiUrl) };
        private readonly ReplayDbContext replayDbContext;

        private readonly TransformBlock<UploadJob, UploadJob> checkIsDuplicate;
        private readonly ActionBlock<UploadJob> uploadReplay;
        private readonly IMatchSummaryService matchSummaryService;

        public HotsLogsUploadService(IServiceProvider serviceProvider, IMatchSummaryService matchSummaryService)
        {
            replayDbContext = serviceProvider.GetRequiredService<ReplayDbContext>();
            checkIsDuplicate = GetCheckIsDuplicateBlock();
            uploadReplay = GetUploadReplayBlock();

            checkIsDuplicate.LinkTo(uploadReplay, new DataflowLinkOptions { PropagateCompletion = true });
            this.matchSummaryService = matchSummaryService;
        }

        public async Task UploadReplayAsync(UploadJob job)
        {
            await checkIsDuplicate.SendAsync(job);
        }

        private TransformBlock<UploadJob, UploadJob> GetCheckIsDuplicateBlock()
        {
            return new TransformBlock<UploadJob, UploadJob>(async job =>
            {
                var replay = job.Replay;
                if (replay.Fingerprint is not null)
                {
                    if (!replay.Id.HasValue)
                    {
                        try
                        {
                            var requestUri = $"uploader/getreplayid?fingerprint={replay.Fingerprint}";
                            using var httpResponse = await _httpClient.GetAsync(requestUri);
                            if (httpResponse.IsSuccessStatusCode)
                            {
                                var strResult = await httpResponse.Content.ReadAsStringAsync();
                                var data = JsonSerializer.Deserialize<ReplayIdResponse>(strResult);
                                replay.Id = data.ReplayId;
                                return job;
                            }
                        }
                        catch
                        {
                        }
                    }

                    replay.UploadStatus = (replay.Id.HasValue ? ReplayParseResult.Success : ReplayParseResult.Exception).AsString();
                }
                else
                {
                    replay.UploadStatus = replay.ParseResult?.ToString() ?? ReplayParseResult.Exception.ToString();
                }
                
                replay.DateUploaded ??= DateTime.UtcNow;
                replayDbContext.SaveOrUpdate(replay);

                return null;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });
        }

        private ActionBlock<UploadJob> GetUploadReplayBlock()
        {
            return new ActionBlock<UploadJob>(async job =>
            {
                if (job != null)
                {
                    job.Replay.UploadStatus = "Uploading...";
                    
                    var replay = job.Replay;
                    using var content = new FileStream(replay.FilePath, FileMode.Open, FileAccess.Read);
                    using var streamContent = new StreamContent(content);
                    using var response = await _httpClient.PostAsync($"uploader/addreplay?{replay.GetUploadParameters()}", streamContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var strResult = await response.Content.ReadAsStringAsync();
                        var data = JsonSerializer.Deserialize<UploadResponse>(strResult);

                        if (data.Result == (int)ReplayParseResult.Success || data.Result == (int)ReplayParseResult.Duplicate)
                        {
                            replay.Id = data.ReplayId;
                            replay.ParseResult = ReplayParseResult.Success;

                            job.OnUploadSuccess?.Invoke(data.ReplayId);
                        }
                        else
                        {
                            replay.ParseResult = (ReplayParseResult)data.Result;
                        }
                    }
                    else
                    {
                        replay.ParseResult = ReplayParseResult.Exception;
                    }

                    replay.DateUploaded = DateTime.UtcNow;
                    replay.UploadStatus = replay.ParseResult.ToString();

                    // regardless, save result attempt
                    replayDbContext.SaveOrUpdate(replay);
                }
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });
        }
    }
}
