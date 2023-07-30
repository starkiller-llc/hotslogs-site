using HOTSLogsUploader.Core.Extensions;
using HOTSLogsUploader.Core.Models;
using HOTSLogsUploader.Core.Models.Db.Entity;
using HOTSLogsUploader.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace HOTSLogsUploader.Core.Utilities.Services
{
    public class CommonUploadService : ICommonUploadService
    {
        private readonly IEnumerable<IUploadService> uploadServices;
        private readonly ActionBlock<UploadJob> queue;

        public CommonUploadService(IEnumerable<IUploadService> uploadServices)
        {
            this.uploadServices = uploadServices;
            queue = new ActionBlock<UploadJob>(async r =>
            {
                PreProcessReplay(r.Replay);

                foreach (var service in uploadServices)
                {
                    await service.UploadReplayAsync(r);
                }
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });
        }

        public void AddToQueue(UploadJob job)
        {
            job.Replay.UploadStatus = "In queue ...";
            queue.Post(job);
        }

        private void PreProcessReplay(Replay replay)
        {
            if (!replay.Fingerprint.HasValue || replay.ParseResult == Heroes.ReplayParser.DataParser.ReplayParseResult.Exception)
            {
                replay.Parse();
            }
        }
    }    

    public interface ICommonUploadService
    {
        void AddToQueue(UploadJob job);
    }
}
