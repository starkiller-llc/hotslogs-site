using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace HOTSLogsUploader.Core.Models.Api
{
    public class UploadResponse
    {
        public int Result { get; set; }
        public int? ReplayId { get; set; }
        public Guid Fingerprint { get; set; }
    }


    public class UpdateCheckResponse
    {
        public UpdateCheckResponse(bool updateAvailable, string newVersion)
        {
            this.NewVersionAvailable = updateAvailable;
            this.NewVersion = newVersion;
        }

        public bool NewVersionAvailable { get; set; }
        public string NewVersion { get; set; }
    };

    public class ReplayIdResponse
    {
        public int? ReplayId { get; set; }
    }
}
