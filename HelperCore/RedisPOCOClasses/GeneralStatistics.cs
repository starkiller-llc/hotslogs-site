using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class GeneralStatistics
{
    public int TotalReplaysUploaded { get; set; }
    public DateTime LastUpdated { get; set; }
}
