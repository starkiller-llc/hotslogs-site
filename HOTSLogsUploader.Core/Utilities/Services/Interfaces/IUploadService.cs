using HOTSLogsUploader.Core.Models;
using System.Threading.Tasks;

namespace HOTSLogsUploader.Core.Utilities.Services
{
    public interface IUploadService
    {
        Task UploadReplayAsync(UploadJob job);
    }
}