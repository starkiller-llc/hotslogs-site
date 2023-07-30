using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HOTSLogsUploader.Core.Utilities.Services
{
    public class MatchSummaryService : IMatchSummaryService
    {
        public void ShowMatchSummaryAction(int? replayId)
        {
            if (replayId is null) return;
            try
            {
                Process.Start(new ProcessStartInfo($@"{Constants.hotslogsUrl}/Player/MatchSummaryContainer?ReplayID={replayId.Value}")
                {
                    UseShellExecute = true
                });
            }
            catch
            {
            }            
        }
    }

    public interface IMatchSummaryService
    {
        void ShowMatchSummaryAction(int? replayId);
    }
}
