using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamProfileReplayPeriodicXPBreakdown
{
    public int GTM { get; set; }
    public int M { get; set; }
    public int C { get; set; }
    public int S { get; set; }
    public int H { get; set; }
    public int T { get; set; }
}