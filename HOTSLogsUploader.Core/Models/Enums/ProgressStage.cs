using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOTSLogsUploader.Core.Enums
{
    public enum ProgressStage
    {
        [Description("")]
        None,
        Added,
        Uploading,
        Completed,
    }
}
