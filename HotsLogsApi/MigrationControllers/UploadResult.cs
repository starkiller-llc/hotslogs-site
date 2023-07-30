using System;

namespace HotsLogsApi.MigrationControllers;

public class UploadResult
{
    public string Status { get; set; } = "Ok";
    public string Result { get; set; }
    public Exception Exception { get; set; }
}
