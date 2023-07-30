using HOTSLogsUploader.Core.Models.Db.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOTSLogsUploader.Core.Models
{
    public class UploadJob
    {
        public Replay Replay { get; set; }
        public Action<int?> OnUploadSuccess { get; set; }
    }
}
