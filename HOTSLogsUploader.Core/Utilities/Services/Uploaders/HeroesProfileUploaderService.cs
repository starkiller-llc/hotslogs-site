using HOTSLogsUploader.Core.Models;
using HOTSLogsUploader.Core.Models.Db.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HOTSLogsUploader.Core.Utilities.Services.Uploaders
{
    public class HeroesProfileUploaderService : IUploadService
    {
        private static readonly HttpClient _httpClient = new();       

        public async Task UploadReplayAsync(UploadJob job)
        {
            try
            {
                using var replayContent = new FileStream(job.Replay.FilePath, FileMode.Open, FileAccess.Read);
                using var postContent = new MultipartFormDataContent();
                // add file content to post request
                postContent.Add(new StreamContent(replayContent), "file", job.Replay.FileName);

                // Upload to heroesprofile
                var result = await _httpClient.PostAsync(Constants.heroesProfilesUploadEndpoint, postContent);
            }
            catch
            {
            }
        }
    }
}
