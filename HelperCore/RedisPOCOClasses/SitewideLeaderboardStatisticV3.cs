using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideLeaderboardStatisticV3
{
    public int LR { get; set; }
    public string N { get; set; }
    public int GP { get; set; }
    public int R { get; set; }
    public int PID { get; set; }
    public DateTime? TSS { get; set; }
}